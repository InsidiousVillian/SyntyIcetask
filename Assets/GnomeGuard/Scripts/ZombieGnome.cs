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

    public abstract class EnemyGnome : MonoBehaviour, IHittable
    {
        public abstract EnemyRole Role { get; }
        protected virtual float Height => 1.35f;
        protected virtual GnomeKind VisualKind => GnomeKind.Zombie;
        protected virtual int TouchDamage => 1;
        protected virtual float Knockback => 7.5f;
        protected virtual float BobRate => 7.5f;
        public virtual int BonusScore => 0;

        public float Speed = 1.7f;
        public int Hp = 1;

        Transform _tree;
        Transform _visual;
        Vector3 _knockback;
        float _seed;
        bool _dead;
        float _rise;
        CapsuleCollider _col;
        GameObject _ice;
        bool _tumbling;
        float _tumble;

        public static EnemyGnome Spawn(Vector3 position, Transform tree, float speed, int hp, Transform parent, EnemyRole role)
        {
            var root = new GameObject(role + "Gnome");
            root.transform.SetParent(parent, false);

            EnemyGnome enemy = role switch
            {
                EnemyRole.Rusher => root.AddComponent<RusherGnome>(),
                EnemyRole.Tank => root.AddComponent<TankGnome>(),
                EnemyRole.Boss => root.AddComponent<KingGnome>(),
                _ => root.AddComponent<GruntGnome>()
            };

            enemy.Assemble(position, tree, speed, hp);
            HitFx.Spawn(position + Vector3.up * 0.2f, new Color(0.75f, 0.9f, 1f), 12);
            if (role == EnemyRole.Boss) GameSfx.Boss();
            return enemy;
        }

        void Assemble(Vector3 position, Transform tree, float speed, int hp)
        {
            transform.position = position + Vector3.down * 1.4f;
            Speed = speed;
            Hp = hp;
            _tree = tree;
            _seed = Random.Range(0f, 30f);

            _col = gameObject.AddComponent<CapsuleCollider>();
            _col.height = Height;
            _col.radius = Height * 0.32f;
            _col.center = new Vector3(0f, Height * 0.5f, 0f);

            var rb = gameObject.AddComponent<Rigidbody>();
            rb.isKinematic = true;
            rb.useGravity = false;
            rb.collisionDetectionMode = CollisionDetectionMode.ContinuousSpeculative;

            var visualHolder = new GameObject("Visual");
            visualHolder.transform.SetParent(transform, false);
            var visual = GnomeAssets.SpawnGnomeVisual(VisualKind, visualHolder.transform);
            if (visual != null)
            {
                GnomeAssets.NormalizeHeight(visual, Height);
                var bounds = GnomeAssets.Encapsulate(visual);
                visual.transform.position += Vector3.up * (transform.position.y - bounds.min.y);
            }

            _visual = visualHolder.transform;

            _ice = GameObject.CreatePrimitive(PrimitiveType.Sphere);
            _ice.name = "Ice";
            Destroy(_ice.GetComponent<Collider>());
            _ice.transform.SetParent(transform, false);
            _ice.transform.localPosition = new Vector3(0f, Height * 0.55f, 0f);
            _ice.transform.localScale = Vector3.one * (Height * 0.55f);
            _ice.GetComponent<MeshRenderer>().sharedMaterial = GnomeAssets.IceMaterial();
            _ice.SetActive(false);
        }

        void Update()
        {
            var game = GnomeGuardGame.Instance;
            if (game == null || game.Paused) return;

            if (_tumbling)
            {
                _tumble += Time.deltaTime;
                if (_visual != null)
                    _visual.localRotation = Quaternion.Euler(Mathf.Lerp(0f, 95f, _tumble / 0.45f), 0f, 18f);
                transform.position += Vector3.down * (0.6f * Time.deltaTime);
                if (_tumble >= 0.5f) Destroy(gameObject);
                return;
            }

            if (_dead || !game.IsPlaying) return;

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

            float speedMul = game.ZombieSpeedMultiplier;
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

            Animate(_visual, frozen);

            bool danger = dist < 4.2f;
            if (game.HighlightHits || danger)
                PulseHighlight(danger);

            if (dist <= 1.45f)
                ReachTree();
        }

        protected virtual void Animate(Transform visual, bool frozen)
        {
            if (visual == null) return;
            if (frozen)
            {
                visual.localPosition = Vector3.zero;
                return;
            }

            float bob = Mathf.Abs(Mathf.Sin((Time.time + _seed) * BobRate)) * 0.12f;
            visual.localPosition = new Vector3(0f, bob, 0f);
            visual.localRotation = Quaternion.Euler(0f, 0f, Mathf.Sin((Time.time + _seed) * 8f) * 6f);
        }

        public bool TryHit(Vector3 hitPoint, Vector3 incoming, int damage)
        {
            if (_dead || _tumbling) return false;

            Hp -= Mathf.Max(1, damage);
            Vector3 push = incoming;
            push.y = 0f;
            if (push.sqrMagnitude < 0.01f && _tree != null)
                push = transform.position - _tree.position;
            if (push.sqrMagnitude < 0.01f) push = -transform.forward;
            _knockback = push.normalized * Knockback;

            HitFx.Spawn(hitPoint, new Color(0.5f, 1f, 0.45f), 14);
            GameSfx.Hit();
            GnomeGuardGame.Instance?.NotifyHit(hitPoint);

            if (Hp <= 0) Die(true);
            else GameSfx.Thump();
            return true;
        }

        void ReachTree()
        {
            if (_dead) return;
            GnomeGuardGame.Instance?.DamageTree(TouchDamage);
            Die(false);
        }

        void Die(bool scored)
        {
            if (_dead) return;
            _dead = true;
            _tumbling = true;
            if (_col != null) _col.enabled = false;
            if (_ice != null) _ice.SetActive(false);
            HitFx.Spawn(transform.position + Vector3.up * (Height * 0.5f), new Color(0.6f, 1f, 0.5f), Role == EnemyRole.Boss ? 48 : 28);
            if (scored)
                GnomeGuardGame.Instance?.OnEnemyKilled(transform.position + Vector3.up, this);
        }

        void PulseHighlight(bool danger)
        {
            if (_visual == null) return;
            float pulse = 0.86f + Mathf.PingPong(Time.time * (danger ? 7f : 3f), danger ? 0.28f : 0.18f);
            _visual.localScale = Vector3.one * pulse;
        }
    }

    public sealed class GruntGnome : EnemyGnome
    {
        public override EnemyRole Role => EnemyRole.Grunt;
    }

    public sealed class RusherGnome : EnemyGnome
    {
        public override EnemyRole Role => EnemyRole.Rusher;
        protected override float Height => 1.05f;
        protected override GnomeKind VisualKind => GnomeKind.Beach;
        protected override float BobRate => 10f;
        public override int BonusScore => 8;
    }

    public sealed class TankGnome : EnemyGnome
    {
        public override EnemyRole Role => EnemyRole.Tank;
        protected override float Height => 1.7f;
        protected override GnomeKind VisualKind => GnomeKind.Soldier;
        protected override float Knockback => 5.2f;
        public override int BonusScore => 20;
    }

    public sealed class KingGnome : EnemyGnome
    {
        public override EnemyRole Role => EnemyRole.Boss;
        protected override float Height => 2.55f;
        protected override int TouchDamage => 2;
        protected override float Knockback => 4.2f;
        public override int BonusScore => 80;
    }
}
