using UnityEngine;

namespace GnomeGuard
{
    public class CameraShake : MonoBehaviour
    {
        float _trauma;
        Vector3 _home;

        void Awake()
        {
            _home = transform.localPosition;
        }

        public void Punch(float amount)
        {
            _trauma = Mathf.Clamp01(_trauma + amount);
        }

        void LateUpdate()
        {
            _trauma = Mathf.MoveTowards(_trauma, 0f, Time.unscaledDeltaTime * 1.7f);
            float shake = _trauma * _trauma;
            if (shake <= 0.001f)
            {
                transform.localPosition = _home;
                return;
            }

            transform.localPosition = _home + Random.insideUnitSphere * shake * 0.2f;
        }
    }

    public class HitBurst : MonoBehaviour
    {
        ParticleSystem _ps;
        float _until;
        bool _busy;

        public static HitBurst Create()
        {
            var go = new GameObject("Burst");
            var ps = go.AddComponent<ParticleSystem>();
            var main = ps.main;
            main.loop = false;
            main.playOnAwake = false;
            main.duration = 0.4f;
            main.startLifetime = 0.45f;
            main.startSpeed = 3.5f;
            main.startSize = 0.12f;
            main.maxParticles = 48;
            main.gravityModifier = 0.6f;
            main.stopAction = ParticleSystemStopAction.None;

            var emission = ps.emission;
            emission.rateOverTime = 0f;

            var shape = ps.shape;
            shape.shapeType = ParticleSystemShapeType.Sphere;
            shape.radius = 0.15f;

            var renderer = go.GetComponent<ParticleSystemRenderer>();
            renderer.material = GnomeAssets.ParticleMaterial();

            var burst = go.AddComponent<HitBurst>();
            burst._ps = ps;
            return burst;
        }

        public void Play(Vector3 position, Color color, int count)
        {
            if (_ps == null) _ps = GetComponent<ParticleSystem>();
            _busy = true;
            transform.position = position;
            if (GnomeGuardGame.Instance != null && GnomeGuardGame.Instance.FxRoot != null)
                transform.SetParent(GnomeGuardGame.Instance.FxRoot, true);

            var main = _ps.main;
            main.startColor = color;
            main.maxParticles = Mathf.Max(1, count);

            var emission = _ps.emission;
            emission.SetBursts(new[] { new ParticleSystem.Burst(0f, (short)count) });
            _ps.Clear();
            _ps.Play();
            _until = Time.time + 0.55f;
        }

        void Update()
        {
            if (!_busy) return;
            if (Time.time >= _until) Recycle();
        }

        void Recycle()
        {
            if (!_busy) return;
            _busy = false;
            if (_ps != null)
                _ps.Stop(true, ParticleSystemStopBehavior.StopEmittingAndClear);

            var pool = GnomeGuardGame.Instance != null ? GnomeGuardGame.Instance.Bursts : null;
            if (pool != null) pool.Release(this);
            else Destroy(gameObject);
        }
    }

    public static class HitFx
    {
        public static void Spawn(Vector3 position, Color color, int count)
        {
            var pool = GnomeGuardGame.Instance != null ? GnomeGuardGame.Instance.Bursts : null;
            HitBurst burst = pool != null ? pool.Get() : HitBurst.Create();
            burst.Play(position, color, count);
        }
    }
}
