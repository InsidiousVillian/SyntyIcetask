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

    public static class HitFx
    {
        public static void Spawn(Vector3 position, Color color, int count)
        {
            var go = new GameObject("Burst");
            go.transform.position = position;
            if (GnomeGuardGame.Instance != null)
                go.transform.SetParent(GnomeGuardGame.Instance.transform, true);

            var ps = go.AddComponent<ParticleSystem>();
            var main = ps.main;
            main.loop = false;
            main.duration = 0.4f;
            main.startLifetime = 0.45f;
            main.startSpeed = 3.5f;
            main.startSize = 0.12f;
            main.startColor = color;
            main.maxParticles = count;
            main.gravityModifier = 0.6f;
            main.stopAction = ParticleSystemStopAction.Destroy;

            var emission = ps.emission;
            emission.rateOverTime = 0f;
            emission.SetBursts(new[] { new ParticleSystem.Burst(0f, (short)count) });

            var shape = ps.shape;
            shape.shapeType = ParticleSystemShapeType.Sphere;
            shape.radius = 0.15f;

            var renderer = go.GetComponent<ParticleSystemRenderer>();
            renderer.material = GnomeAssets.ParticleMaterial();
            ps.Play();
        }
    }
}
