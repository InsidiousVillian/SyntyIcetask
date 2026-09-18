using UnityEngine;

namespace GnomeGuard
{
    public enum EnemyRole
    {
        Grunt,
        Rusher,
        Tank,
        Boss
    }

    public class ZombieGnome : MonoBehaviour
    {
        public float Speed = 1.7f;
        public int Hp = 1;
        public int Damage = 1;
        public EnemyRole Role = EnemyRole.Grunt;

        Transform _tree;
        Transform _visual;
        Vector3 _knockback;
        float _seed;
        bool _dead;
        float _rise;
        float _baseHeight = 1.35f;
        CapsuleCollider _col;
        GameObject _ice;
        bool _tumbling;
        float _tumble;

        public static ZombieGnome Spawn(Vector3 position, Transform tree, float speed, int hp, Transform parent, EnemyRole role)
        {
            float height = role switch
            {
                EnemyRole.Rusher => 1.05f,
                EnemyRole.Tank => 1.7f,
                EnemyRole.Boss => 2.55f,
                _ => 1.35f
            };

            var root = new GameObject(role == EnemyRole.Boss ? "KingGnome" : "ZombieGnome");
            root.transform.SetParent(parent, false);
            root.transform.position = position + Vector3.down * 1.4f;

            var col = root.AddComponent<CapsuleCollider>();
            col.height = height;
            col.radius = height * 0.32f;
            col.center = new Vector3(0f, height * 0.5f, 0f);

            var rb = root.AddComponent<Rigidbody>();
            rb.isKinematic = true;
            rb.useGravity = false;
            rb.collisionDetectionMode = CollisionDetectionMode.ContinuousSpeculative;

            var visualHolder = new GameObject("Visual");
            visualHolder.transform.SetParent(root.transform, false);
            var visual = GnomeAssets.SpawnGnomeVisual(GnomeKind.Zombie, visualHolder.transform);
            if (visual != null)
            {
                GnomeAssets.NormalizeHeight(visual, height);
                var bounds = GnomeAssets.Encapsulate(visual);
                visual.transform.position += Vector3.up * (root.transform.position.y - bounds.min.y);
            }

            var ice = GameObject.CreatePrimitive(PrimitiveType.Sphere);
            ice.name = "Ice";
            Object.Destroy(ice.GetComponent<Collider>());
            ice.transform.SetParent(root.transform, false);
            ice.transform.localPosition = new Vector3(0f, height * 0.55f, 0f);
            ice.transform.localScale = Vector3.one * (height * 0.55f);
            ice.GetComponent<MeshRenderer>().sharedMaterial = GnomeAssets.IceMaterial();
            ice.SetActive(false);

            var zombie = root.AddComponent<ZombieGnome>();
            zombie._tree = tree;
            zombie._visual = visualHolder.transform;
            zombie._col = col;
            zombie._ice = ice;
            zombie._baseHeight = height;
            zombie.Speed = speed;
            zombie.Hp = hp;
            zombie.Role = role;
            zombie.Damage = role == EnemyRole.Boss ? 2 : 1;
            zombie._seed = Random.Range(0f, 30f);

            HitFx.Spawn(position + Vector3.up * 0.2f, new Color(0.75f, 0.9f, 1f), 12);
            if (role == EnemyRole.Boss) GameSfx.Boss();
            return zombie;
        }

        void Update()
        {
            if (GnomeGuardGame.Instance == null) return;
            if (GnomeGuardGame.Instance.Paused) return;

            if (_tumbling)
            {
                _tumble += Time.deltaTime;
                if (_visual != null)
                    _visual.localRotation = Quaternion.Euler(Mathf.Lerp(0f, 95f, _tumble / 0.45f), 0f, 18f);
                transform.position += Vector3.down * (0.6f * Time.deltaTime);
                if (_tumble >= 0.5f) Destroy(gameObject);
                return;
            }

            if (_dead || !GnomeGuardGame.Instance.IsPlaying) return;

            if (_rise < 1f)
            {
                _rise = Mathf.MoveTowards(_rise, 1f, Time.deltaTime / 0.55f);
                Vector3 p = transform.position;
                p.y = Mathf.Lerp(-1.3f, 0f, _rise * _rise);
                transform.position = p;
                return;
            }

            Vector3 target = _tree != null ? _tree.position : Vector3.zero;
            Vector3 toTree = target - transform.position;
            toTree.y = 0f;
            float dist = toTree.magnitude;

            float speedMul = GnomeGuardGame.Instance.ZombieSpeedMultiplier;
            bool frozen = speedMul <= 0.001f;
            if (_ice != null) _ice.SetActive(frozen);

            Vector3 move = Vector3.zero;
            if (!frozen && dist > 1.45f)
                move = toTree.normalized * (Speed * speedMul);

            _knockback = Vector3.MoveTowards(_knockback, Vector3.zero, 10f * Time.deltaTime);
            transform.position += (move + _knockback) * Time.deltaTime;
            transform.position = new Vector3(transform.position.x, 0f, transform.position.z);

            if (toTree.sqrMagnitude > 0.001f)
            {
                float yaw = Quaternion.LookRotation(toTree.normalized).eulerAngles.y;
                yaw += frozen ? 0f : Mathf.Sin((Time.time + _seed) * 6.2f) * 14f;
                transform.rotation = Quaternion.Euler(0f, yaw, 0f);
            }

            if (_visual != null && !frozen)
            {
                float bob = Mathf.Abs(Mathf.Sin((Time.time + _seed) * (Role == EnemyRole.Rusher ? 10f : 7.5f))) * 0.12f;
                _visual.localPosition = new Vector3(0f, bob, 0f);
                _visual.localRotation = Quaternion.Euler(0f, 0f, Mathf.Sin((Time.time + _seed) * 8f) * 6f);
            }
            else if (_visual != null)
            {
                _visual.localPosition = Vector3.zero;
            }

            bool danger = dist < 4.2f;
            if (GnomeGuardGame.Instance.HighlightHits || danger)
                PulseHighlight(danger);

            if (dist <= 1.45f)
                ReachTree();
        }

        public void Hit(Vector3 hitPoint, Vector3 incoming, int damage = 1)
        {
            if (_dead || _tumbling) return;

            Hp -= Mathf.Max(1, damage);
            Vector3 push = incoming;
            push.y = 0f;
            if (push.sqrMagnitude < 0.01f && _tree != null)
                push = transform.position - _tree.position;
            if (push.sqrMagnitude < 0.01f) push = -transform.forward;
            _knockback = push.normalized * (Role == EnemyRole.Boss ? 4.2f : 7.5f);

            HitFx.Spawn(hitPoint, new Color(0.5f, 1f, 0.45f), 14);
            GameSfx.Hit();
            if (GnomeGuardGame.Instance != null)
                GnomeGuardGame.Instance.NotifyHit(hitPoint);

            if (Hp <= 0) Die();
            else GameSfx.Thump();
        }

        void ReachTree()
        {
            if (_dead) return;
            if (GnomeGuardGame.Instance != null)
                GnomeGuardGame.Instance.DamageTree(Damage);
            Die(false);
        }

        void Die(bool scored = true)
        {
            if (_dead) return;
            _dead = true;
            _tumbling = true;
            if (_col != null) _col.enabled = false;
            if (_ice != null) _ice.SetActive(false);
            HitFx.Spawn(transform.position + Vector3.up * (_baseHeight * 0.5f), new Color(0.6f, 1f, 0.5f), Role == EnemyRole.Boss ? 48 : 28);
            if (scored && GnomeGuardGame.Instance != null)
                GnomeGuardGame.Instance.OnZombieKilled(transform.position + Vector3.up, Role);
        }

        void PulseHighlight(bool danger)
        {
            if (_visual == null) return;
            float pulse = 0.86f + Mathf.PingPong(Time.time * (danger ? 7f : 3f), danger ? 0.28f : 0.18f);
            _visual.localScale = Vector3.one * pulse;
        }
    }
}
