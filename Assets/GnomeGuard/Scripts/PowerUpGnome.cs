using UnityEngine;

namespace GnomeGuard
{
    public class PowerUpGnome : MonoBehaviour, IPickup
    {
        public GnomeKind Kind;

        Vector3 _home;
        bool _taken;
        Light _glow;

        public static PowerUpGnome Spawn(GnomeKind kind, Vector3 position, Transform parent)
        {
            var root = new GameObject("PowerUp_" + kind);
            root.transform.SetParent(parent, false);
            root.transform.position = position;

            var col = root.AddComponent<CapsuleCollider>();
            col.isTrigger = true;
            col.height = 1.8f;
            col.radius = 0.7f;
            col.center = new Vector3(0f, 0.8f, 0f);

            var rb = root.AddComponent<Rigidbody>();
            rb.isKinematic = true;
            rb.useGravity = false;

            var visual = GnomeAssets.SpawnGnomeVisual(kind, root.transform);
            if (visual != null)
            {
                GnomeAssets.NormalizeHeight(visual, 1.2f);
                GnomeAssets.SnapFeetTo(visual, position.y);
            }

            var labelGo = new GameObject("Label");
            labelGo.transform.SetParent(root.transform, false);
            labelGo.transform.localPosition = new Vector3(0f, 2.05f, 0f);
            var tm = labelGo.AddComponent<TextMesh>();
            tm.text = "PICK UP\n" + GnomeAssets.DisplayName(kind);
            tm.fontSize = 42;
            tm.characterSize = 0.055f;
            tm.anchor = TextAnchor.LowerCenter;
            tm.alignment = TextAlignment.Center;
            tm.color = new Color(1f, 0.92f, 0.55f);
            tm.fontStyle = FontStyle.Bold;
            labelGo.AddComponent<Billboard>();

            var lightGo = new GameObject("Glow");
            lightGo.transform.SetParent(root.transform, false);
            lightGo.transform.localPosition = new Vector3(0f, 1.1f, 0f);
            var glow = lightGo.AddComponent<Light>();
            glow.type = LightType.Point;
            glow.color = new Color(1f, 0.85f, 0.4f);
            glow.range = 5f;
            glow.intensity = 2.4f;
            glow.shadows = LightShadows.None;

            var power = root.AddComponent<PowerUpGnome>();
            power.Kind = kind;
            power._home = position;
            power._glow = glow;
            return power;
        }

        void Update()
        {
            if (_taken) return;
            if (GnomeGuardGame.Instance != null && GnomeGuardGame.Instance.Paused) return;

            if (GnomeGuardGame.Instance != null)
            {
                Vector3 player = GnomeGuardGame.Instance.PlayerPosition;
                Vector3 toPlayer = player - _home;
                toPlayer.y = 0f;
                if (toPlayer.magnitude < 4.2f)
                    _home += toPlayer.normalized * (5.5f * Time.deltaTime);
            }

            float t = Time.time;
            transform.position = _home + Vector3.up * (0.35f + Mathf.Sin(t * 2.4f) * 0.22f);
            transform.Rotate(0f, 70f * Time.deltaTime, 0f);
            if (_glow != null)
                _glow.intensity = 2f + Mathf.Sin(t * 5f) * 0.6f;
        }

        void OnTriggerEnter(Collider other)
        {
            if (other.GetComponentInParent<FirstPersonPlayer>() != null)
                Collect();
        }

        public void Collect()
        {
            if (_taken) return;
            _taken = true;
            HitFx.Spawn(transform.position + Vector3.up, new Color(1f, 0.85f, 0.3f), 24);
            GameSfx.Pickup();
            if (GnomeGuardGame.Instance != null)
                GnomeGuardGame.Instance.ApplyPowerUp(Kind);
            Destroy(gameObject);
        }
    }

    public class Billboard : MonoBehaviour
    {
        void LateUpdate()
        {
            var cam = Camera.main;
            if (cam == null) return;
            transform.rotation = Quaternion.LookRotation(transform.position - cam.transform.position, Vector3.up);
        }
    }
}
