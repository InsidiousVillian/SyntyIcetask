using UnityEngine;

namespace GnomeGuard
{
    public class Snowball : MonoBehaviour
    {
        const float Life = 3.2f;

        Rigidbody _body;
        SphereCollider _col;
        TrailRenderer _trail;
        float _dieAt;
        Transform _owner;
        int _damage = 1;
        bool _inUse;

        public static void Spawn(Vector3 position, Vector3 direction, float force, Transform owner, float charge = 0f)
        {
            Snowball ball;
            var pool = GnomeGuardGame.Instance != null ? GnomeGuardGame.Instance.Snowballs : null;
            if (pool != null)
                ball = pool.Get();
            else
                ball = Create();

            ball.Launch(position, direction, force, owner, charge);
        }

        public static Snowball Create()
        {
            var go = GameObject.CreatePrimitive(PrimitiveType.Sphere);
            go.name = "Snowball";
            Object.Destroy(go.GetComponent<Collider>());

            var col = go.AddComponent<SphereCollider>();
            col.radius = 0.5f;
            col.isTrigger = true;

            var rb = go.AddComponent<Rigidbody>();
            rb.interpolation = RigidbodyInterpolation.Interpolate;
            rb.collisionDetectionMode = CollisionDetectionMode.ContinuousDynamic;

            go.GetComponent<MeshRenderer>().sharedMaterial = GnomeAssets.SnowballMaterial();

            var trail = go.AddComponent<TrailRenderer>();
            trail.endWidth = 0.01f;
            trail.material = GnomeAssets.UnlitWhite();
            trail.endColor = new Color(0.8f, 0.9f, 1f, 0f);

            var ball = go.AddComponent<Snowball>();
            ball._body = rb;
            ball._col = col;
            ball._trail = trail;
            return ball;
        }

        void Awake()
        {
            if (_body == null) _body = GetComponent<Rigidbody>();
            if (_col == null) _col = GetComponent<SphereCollider>();
            if (_trail == null) _trail = GetComponent<TrailRenderer>();
        }

        public void Launch(Vector3 position, Vector3 direction, float force, Transform owner, float charge)
        {
            bool heavy = charge >= 0.22f;
            float size = heavy ? 0.22f + charge * 0.2f : 0.22f;
            _inUse = true;
            _owner = owner;
            _damage = heavy ? 2 : 1;
            _dieAt = Time.time + Life;

            transform.SetParent(GnomeGuardGame.Instance != null ? GnomeGuardGame.Instance.Projectiles : null, true);
            transform.position = position;
            transform.localScale = Vector3.one * size;
            gameObject.SetActive(true);

            _body.linearDamping = heavy ? 0.04f : 0.08f;
            _body.angularDamping = 0.05f;
            _body.mass = heavy ? 0.7f : 0.35f;
            _body.linearVelocity = Vector3.zero;
            _body.angularVelocity = Vector3.zero;

            _trail.Clear();
            _trail.time = heavy ? 0.42f : 0.28f;
            _trail.startWidth = size * (heavy ? 0.9f : 0.55f);
            _trail.startColor = heavy ? new Color(0.75f, 0.95f, 1f, 1f) : new Color(1f, 1f, 1f, 0.9f);

            _body.AddForce(direction.normalized * force, ForceMode.VelocityChange);
            _body.AddTorque(Random.insideUnitSphere * 8f, ForceMode.VelocityChange);

            var ownerCol = owner != null ? owner.GetComponent<Collider>() : null;
            if (ownerCol != null && _col != null)
                Physics.IgnoreCollision(_col, ownerCol, true);
        }

        void Update()
        {
            if (!_inUse) return;
            if (Time.time >= _dieAt || transform.position.y < -4f)
                Recycle();
        }

        void OnTriggerEnter(Collider other)
        {
            if (!_inUse) return;
            if (_owner != null && other.transform.IsChildOf(_owner))
                return;

            var hittable = other.GetComponentInParent<IHittable>();
            if (hittable != null)
            {
                hittable.TryHit(transform.position, _body != null ? _body.linearVelocity : transform.forward, _damage);
                Burst();
                return;
            }

            var pickup = other.GetComponentInParent<IPickup>();
            if (pickup != null)
            {
                pickup.Collect();
                Burst();
                return;
            }

            if (other.GetComponentInParent<FirstPersonPlayer>() != null)
                return;

            if (other.isTrigger)
                return;

            Burst();
        }

        void Burst()
        {
            HitFx.Spawn(transform.position, new Color(0.9f, 0.95f, 1f), _damage > 1 ? 28 : 18);
            Recycle();
        }

        void Recycle()
        {
            if (!_inUse) return;
            _inUse = false;
            _body.linearVelocity = Vector3.zero;
            _body.angularVelocity = Vector3.zero;
            var pool = GnomeGuardGame.Instance != null ? GnomeGuardGame.Instance.Snowballs : null;
            if (pool != null) pool.Release(this);
            else Destroy(gameObject);
        }
    }
}
