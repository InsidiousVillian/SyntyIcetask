using UnityEngine;

namespace GnomeGuard
{
    public class Snowball : MonoBehaviour
    {
        const float Life = 3.2f;

        Rigidbody _body;
        float _dieAt;
        Transform _owner;
        int _damage = 1;

        public static void Spawn(Vector3 position, Vector3 direction, float force, Transform owner, float charge = 0f)
        {
            bool heavy = charge >= 0.22f;
            float size = heavy ? 0.22f + charge * 0.2f : 0.22f;

            var go = GameObject.CreatePrimitive(PrimitiveType.Sphere);
            go.name = heavy ? "ChargedSnowball" : "Snowball";
            go.transform.position = position;
            go.transform.localScale = Vector3.one * size;
            Object.Destroy(go.GetComponent<Collider>());

            var col = go.AddComponent<SphereCollider>();
            col.radius = 0.5f;
            col.isTrigger = true;

            var rb = go.AddComponent<Rigidbody>();
            rb.interpolation = RigidbodyInterpolation.Interpolate;
            rb.collisionDetectionMode = CollisionDetectionMode.ContinuousDynamic;
            rb.linearDamping = heavy ? 0.04f : 0.08f;
            rb.angularDamping = 0.05f;
            rb.mass = heavy ? 0.7f : 0.35f;

            var renderer = go.GetComponent<MeshRenderer>();
            renderer.sharedMaterial = GnomeAssets.SnowballMaterial();

            var trail = go.AddComponent<TrailRenderer>();
            trail.time = heavy ? 0.42f : 0.28f;
            trail.startWidth = size * (heavy ? 0.9f : 0.55f);
            trail.endWidth = 0.01f;
            trail.material = GnomeAssets.UnlitWhite();
            trail.startColor = heavy ? new Color(0.75f, 0.95f, 1f, 1f) : new Color(1f, 1f, 1f, 0.9f);
            trail.endColor = new Color(0.8f, 0.9f, 1f, 0f);

            if (GnomeGuardGame.Instance != null)
                go.transform.SetParent(GnomeGuardGame.Instance.transform, true);

            var ball = go.AddComponent<Snowball>();
            ball._body = rb;
            ball._dieAt = Time.time + Life;
            ball._owner = owner;
            ball._damage = heavy ? 2 : 1;
            rb.AddForce(direction.normalized * force, ForceMode.VelocityChange);
            rb.AddTorque(Random.insideUnitSphere * 8f, ForceMode.VelocityChange);

            var ownerCol = owner != null ? owner.GetComponent<Collider>() : null;
            if (ownerCol != null)
                Physics.IgnoreCollision(col, ownerCol, true);
        }

        void Update()
        {
            if (Time.time >= _dieAt)
            {
                Destroy(gameObject);
                return;
            }

            if (transform.position.y < -4f)
                Destroy(gameObject);
        }

        void OnTriggerEnter(Collider other)
        {
            if (_owner != null && other.transform.IsChildOf(_owner))
                return;

            var zombie = other.GetComponentInParent<ZombieGnome>();
            if (zombie != null)
            {
                zombie.Hit(transform.position, _body != null ? _body.linearVelocity : transform.forward, _damage);
                Burst();
                return;
            }

            var power = other.GetComponentInParent<PowerUpGnome>();
            if (power != null)
            {
                power.Collect();
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
            Destroy(gameObject);
        }
    }
}
