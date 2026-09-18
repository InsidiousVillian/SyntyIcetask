using System.Collections;
using UnityEngine;
using UnityEngine.InputSystem;

namespace GnomeGuard
{
    public class GnomeGuardGame : MonoBehaviour
    {
        public static GnomeGuardGame Instance { get; private set; }

        public const int MaxTreeHp = 8;

        public bool IsPlaying { get; private set; }
        public bool IsGameOver => _gameOver;
        public bool Paused { get; private set; }
        public bool RapidFire => _rapidUntil > Time.time;
        public bool HighlightHits => _highlightUntil > Time.time;
        public CameraShake Shake => _world != null ? _world.Shake : null;
        public Vector3 PlayerPosition => _world?.Player != null ? _world.Player.transform.position : Vector3.zero;
        public float ZombieSpeedMultiplier
        {
            get
            {
                if (_freezeUntil > Time.time) return 0f;
                if (_slowUntil > Time.time) return 0.4f;
                return 1f;
            }
        }

        WorldContext _world;
        GameHud _hud;
        int _score;
        int _best;
        int _wave;
        int _treeHp = MaxTreeHp;
        int _combo = 1;
        float _comboUntil;
        int _alive;
        float _freezeUntil;
        float _slowUntil;
        float _rapidUntil;
        float _highlightUntil;
        float _shieldUntil;
        bool _gameOver;
        bool _armed;
        bool _comboPulse;
        bool _waveDamage;
        bool _lastCall;
        bool _spawning;

        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterSceneLoad)]
        static void Boot()
        {
            if (!Application.isPlaying) return;
            if (FindFirstObjectByType<GnomeGuardGame>() != null) return;
            var go = new GameObject("GnomeGuard");
            go.AddComponent<GnomeGuardGame>();
        }

        void Awake()
        {
            if (Instance != null && Instance != this)
            {
                Destroy(gameObject);
                return;
            }

            Instance = this;
            _best = PlayerPrefs.GetInt("GnomeGuard.Best", 0);
        }

        void Start()
        {
            if (!GnomeAssets.Init())
            {
                BuildError(GnomeAssets.LastError);
                return;
            }

            _world = WorldBuilder.Build(transform);
            gameObject.AddComponent<GameSfx>();
            _hud = GameHud.Create(transform);
            _hud.SetPlayingHud(false);
            _hud.ShowTitle(true);
            _armed = true;
            SetCursor(false);
        }

        void OnDestroy()
        {
            if (Instance == this) Instance = null;
            if (_world != null && _world.SceneCamera != null)
            {
                _world.SceneCamera.enabled = true;
                var listener = _world.SceneCamera.GetComponent<AudioListener>();
                if (listener != null) listener.enabled = true;
            }

            SetCursor(false);
        }

        void Update()
        {
            if (!_armed) return;

            var kb = Keyboard.current;
            var mouse = Mouse.current;

            if (_gameOver)
            {
                if (kb != null && kb.rKey.wasPressedThisFrame)
                    Restart();
                return;
            }

            if (!IsPlaying)
            {
                bool start = (mouse != null && mouse.leftButton.wasPressedThisFrame)
                             || (kb != null && (kb.spaceKey.wasPressedThisFrame || kb.enterKey.wasPressedThisFrame));
                if (start) BeginRun();
                return;
            }

            if (kb != null && kb.escapeKey.wasPressedThisFrame)
                TogglePause();

            if (Paused) return;

            if (Time.time > _comboUntil)
            {
                _combo = 1;
                _comboPulse = false;
            }

            if (_alive == 1 && !_spawning && !_lastCall)
            {
                _lastCall = true;
                _hud.Banner("LAST GNOME", 1.1f);
            }

            float charge = _world?.Player != null ? _world.Player.Charge01 : 0f;
            _hud.SetStats(_score, _combo, _wave, _alive, _treeHp, MaxTreeHp, StatusText(), charge);

            if (_world != null && _world.TreeFx != null)
            {
                _world.TreeFx.SetHealth(_treeHp / (float)MaxTreeHp);
                _world.TreeFx.SetShield(_shieldUntil > Time.time);
            }
        }

        void BeginRun()
        {
            IsPlaying = true;
            _gameOver = false;
            _score = 0;
            _wave = 0;
            _treeHp = MaxTreeHp;
            _combo = 1;
            _comboPulse = false;
            _freezeUntil = _slowUntil = _rapidUntil = _highlightUntil = _shieldUntil = 0f;
            _hud.ShowTitle(false);
            _hud.HideGameOver();
            _hud.SetPlayingHud(true);
            if (_world.Player != null) _world.Player.ResetToSpawn();
            SetCursor(true);
            StartCoroutine(EnablePlayerNextFrame());
            StartCoroutine(RunWaves());
        }

        IEnumerator EnablePlayerNextFrame()
        {
            yield return null;
            if (_world != null && _world.Player != null && IsPlaying && !_gameOver)
                _world.Player.CanAct = true;
        }

        IEnumerator RunWaves()
        {
            yield return new WaitForSeconds(0.4f);
            while (IsPlaying && !_gameOver)
            {
                _wave++;
                _waveDamage = false;
                _lastCall = false;
                GameSfx.Wave();

                bool bossWave = _wave % 5 == 0;
                _hud.Banner(bossWave ? $"WAVE {_wave}  KING GNOME" : $"WAVE {_wave}", 1.7f);

                int count = 4 + _wave * 2;
                float spawnGap = Mathf.Max(0.28f, 0.82f - _wave * 0.05f);
                float speed = 1.45f + _wave * 0.16f;

                _spawning = true;
                if (bossWave)
                {
                    SpawnZombie(speed * 0.62f, 6 + _wave / 5 * 2, EnemyRole.Boss);
                    yield return new WaitForSeconds(1.1f);
                }

                for (int i = 0; i < count && IsPlaying && !_gameOver; i++)
                {
                    while (Paused) yield return null;
                    EnemyRole role = EnemyRole.Grunt;
                    int hp = _wave >= 5 ? 2 : 1;
                    float gnomeSpeed = speed;
                    if (_wave >= 4 && i % 6 == 5)
                    {
                        role = EnemyRole.Tank;
                        hp = 3;
                        gnomeSpeed = speed * 0.7f;
                    }
                    else if (_wave >= 3 && i % 4 == 3)
                    {
                        role = EnemyRole.Rusher;
                        hp = 1;
                        gnomeSpeed = speed * 1.45f;
                    }

                    SpawnZombie(gnomeSpeed, hp, role);
                    yield return new WaitForSeconds(spawnGap);
                }

                _spawning = false;
                while (IsPlaying && !_gameOver && _alive > 0)
                    yield return null;

                if (!IsPlaying || _gameOver) yield break;

                if (!_waveDamage)
                {
                    _treeHp = Mathf.Min(MaxTreeHp, _treeHp + 1);
                    _score += 150;
                    GameSfx.Perfect();
                    if (_world.TreeFx != null) _world.TreeFx.Celebrate();
                    _hud.Banner("PERFECT WAVE  +150", 1.8f);
                }

                SpawnPowerUp();
                if (_waveDamage)
                    _hud.Banner("NICE  Grab a holiday gnome", 1.8f);
                yield return new WaitForSeconds(2.4f);
            }
        }

        void SpawnZombie(float speed, int hp, EnemyRole role)
        {
            Vector3 pos = EdgePoint(_world.ArenaRadius - 1.2f);
            if (_world.Player != null)
            {
                int guard = 0;
                while (Vector3.Distance(pos, _world.Player.transform.position) < 6f && guard++ < 8)
                    pos = EdgePoint(_world.ArenaRadius - 1.2f);
            }

            var zombie = ZombieGnome.Spawn(pos, _world.Tree, speed, hp, transform, role);
            if (zombie != null) _alive++;
        }

        void SpawnPowerUp()
        {
            var kinds = new[]
            {
                GnomeKind.Wizard, GnomeKind.Soldier, GnomeKind.Santa, GnomeKind.Dwarf,
                GnomeKind.Beach, GnomeKind.Theorist, GnomeKind.Basalt
            };
            var kind = kinds[Random.Range(0, kinds.Length)];
            Vector3 pos = EdgePoint(_world.ArenaRadius * 0.55f);
            if (_world.Player != null)
            {
                int guard = 0;
                while ((Vector3.Distance(pos, _world.Player.transform.position) < 4f
                        || Vector3.Distance(pos, _world.Tree.position) < 3.5f) && guard++ < 10)
                    pos = EdgePoint(_world.ArenaRadius * 0.55f);
            }

            PowerUpGnome.Spawn(kind, pos, transform);
        }

        Vector3 EdgePoint(float radius)
        {
            float ang = Random.Range(0f, Mathf.PI * 2f);
            return new Vector3(Mathf.Cos(ang) * radius, 0f, Mathf.Sin(ang) * radius);
        }

        public void NotifyHit(Vector3 worldPoint)
        {
            if (_hud != null) _hud.HitMarker();
            if (_world != null && _world.Shake != null) _world.Shake.Punch(0.08f);
        }

        public void OnZombieKilled(Vector3 popupAt, EnemyRole role)
        {
            _alive = Mathf.Max(0, _alive - 1);
            if (!IsPlaying) return;

            if (Time.time <= _comboUntil) _combo++;
            else _combo = 1;
            _comboUntil = Time.time + 1.85f;

            int gain = 10 * _combo;
            if (role == EnemyRole.Rusher) gain += 8;
            if (role == EnemyRole.Tank) gain += 20;
            if (role == EnemyRole.Boss) gain += 80;
            if (HighlightHits) gain += 5;
            _score += gain;

            Color color = role == EnemyRole.Boss ? new Color(1f, 0.75f, 0.2f) : new Color(1f, 1f, 0.75f);
            FloatingText.Spawn(popupAt, "+" + gain, color);

            if (_combo == 5 || _combo == 10 || _combo == 15)
                GameSfx.Combo();

            if (_combo >= 10 && !_comboPulse)
            {
                _comboPulse = true;
                _freezeUntil = Mathf.Max(_freezeUntil, Time.time + 2.2f);
                _hud.Banner("COMBO STUN!", 1.3f);
            }
        }

        public void DamageTree(int amount)
        {
            if (!IsPlaying || _gameOver) return;
            _alive = Mathf.Max(0, _alive - 1);

            if (_shieldUntil > Time.time)
            {
                _hud.Banner("SHIELD HELD", 0.8f);
                return;
            }

            _waveDamage = true;
            _treeHp -= amount;
            _hud.DamageFlash();
            GameSfx.Hurt();
            if (_world.Shake != null) _world.Shake.Punch(0.55f);
            if (_world.TreeFx != null) _world.TreeFx.Hit();
            if (_treeHp <= 0) EndRun();
        }

        public void ApplyPowerUp(GnomeKind kind)
        {
            if (!IsPlaying) return;
            switch (kind)
            {
                case GnomeKind.Wizard:
                    _freezeUntil = Time.time + 4.5f;
                    _hud.Banner("FREEZE!", 1.4f);
                    break;
                case GnomeKind.Soldier:
                    _rapidUntil = Time.time + 8f;
                    _hud.Banner("RAPID FIRE!", 1.4f);
                    break;
                case GnomeKind.Santa:
                    _treeHp = Mathf.Min(MaxTreeHp, _treeHp + 2);
                    _score += 50;
                    _hud.Banner("SANTA REPAIRS THE TREE", 1.5f);
                    if (_world.TreeFx != null) _world.TreeFx.Celebrate();
                    break;
                case GnomeKind.Dwarf:
                    _score += 120;
                    _hud.Banner("GOLDEN NUGGET +120", 1.4f);
                    break;
                case GnomeKind.Beach:
                    _slowUntil = Time.time + 6.5f;
                    _hud.Banner("VACATION SLOW-MO", 1.4f);
                    break;
                case GnomeKind.Theorist:
                    _highlightUntil = Time.time + 8f;
                    _hud.Banner("THEORY: BONUS HITS", 1.4f);
                    break;
                case GnomeKind.Basalt:
                    _shieldUntil = Time.time + 6.5f;
                    _hud.Banner("BASALT SHIELD", 1.4f);
                    break;
            }
        }

        string StatusText()
        {
            if (_shieldUntil > Time.time) return "TREE SHIELD";
            if (_freezeUntil > Time.time) return "ZOMBIES FROZEN";
            if (_rapidUntil > Time.time) return "RAPID FIRE";
            if (_slowUntil > Time.time) return "SLOW-MO";
            if (_highlightUntil > Time.time) return "BONUS HITS";
            return "";
        }

        void EndRun()
        {
            if (_gameOver) return;
            _gameOver = true;
            IsPlaying = false;
            if (_world.Player != null) _world.Player.CanAct = false;
            _best = Mathf.Max(_best, _score);
            PlayerPrefs.SetInt("GnomeGuard.Best", _best);
            PlayerPrefs.Save();
            _hud.SetPlayingHud(false);
            _hud.ShowGameOver(_wave, _score, _best);
            GameSfx.GameOver();
            SetCursor(false);
            StopAllCoroutines();
        }

        void TogglePause()
        {
            Paused = !Paused;
            if (_world.Player != null) _world.Player.CanAct = !Paused;
            SetCursor(!Paused);
            _hud.Banner(Paused ? "PAUSED  Esc to resume" : "", Paused ? 999f : 0.05f);
        }

        void Restart()
        {
            StopAllCoroutines();
            if (_world != null && _world.SceneCamera != null)
            {
                _world.SceneCamera.enabled = true;
                var listener = _world.SceneCamera.GetComponent<AudioListener>();
                if (listener != null) listener.enabled = true;
            }

            Destroy(gameObject);
            var go = new GameObject("GnomeGuard");
            go.AddComponent<GnomeGuardGame>();
        }

        void SetCursor(bool locked)
        {
            Cursor.lockState = locked ? CursorLockMode.Locked : CursorLockMode.None;
            Cursor.visible = !locked;
        }

        void BuildError(string message)
        {
            _hud = GameHud.Create(transform);
            _hud.ShowTitle(true);
            _hud.Banner(string.IsNullOrEmpty(message) ? "Could not load Holiday Gnomes assets." : message, 30f);
        }
    }
}
