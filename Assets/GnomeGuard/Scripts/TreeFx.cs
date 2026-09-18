using UnityEngine;

namespace GnomeGuard
{
    public class TreeFx : MonoBehaviour
    {
        Light[] _fairy;
        Light _core;
        ParticleSystem _shield;
        ParticleSystem _sparkle;
        Transform _visual;
        Quaternion _visualBaseRot = Quaternion.identity;
        float _hp = 1f;
        float _hitPulse;
        bool _shieldOn;
        Vector3 _visualScale = Vector3.one;

        public static TreeFx Add(Transform holder)
        {
            var fx = holder.gameObject.AddComponent<TreeFx>();
            fx.Build();
            return fx;
        }

        void Build()
        {
            if (transform.childCount > 0)
            {
                _visual = transform.GetChild(0);
                _visualBaseRot = _visual.localRotation;
                _visualScale = _visual.localScale;
            }

            _core = transform.Find("TreeLight")?.GetComponent<Light>();

            _fairy = new Light[6];
            Color[] colors =
            {
                new Color(1f, 0.2f, 0.15f),
                new Color(0.2f, 1f, 0.28f),
                new Color(1f, 0.82f, 0.25f),
                new Color(0.35f, 0.7f, 1f)
            };

            for (int i = 0; i < _fairy.Length; i++)
            {
                float ang = i / (float)_fairy.Length * Mathf.PI * 2f;
                var go = new GameObject("Fairy_" + i);
                go.transform.SetParent(transform, false);
                go.transform.localPosition = new Vector3(Mathf.Cos(ang) * 1.35f, 1.1f + (i % 4) * 0.7f, Mathf.Sin(ang) * 1.35f);
                var light = go.AddComponent<Light>();
                light.type = LightType.Point;
                light.color = colors[i % colors.Length];
                light.range = 3.6f;
                light.intensity = 1.4f;
                light.shadows = LightShadows.None;
                _fairy[i] = light;
            }

            _sparkle = MakeBurst("Sparkle", new Color(1f, 0.85f, 0.4f, 0.9f), 18f, 0.08f, true);
            _sparkle.transform.localPosition = new Vector3(0f, 2.6f, 0f);
            var sparkleShape = _sparkle.shape;
            sparkleShape.shapeType = ParticleSystemShapeType.Hemisphere;
            sparkleShape.radius = 1.4f;

            _shield = MakeBurst("Shield", new Color(0.35f, 0.9f, 1f, 0.7f), 0f, 0.18f, true);
            var shieldMain = _shield.main;
            shieldMain.startLifetime = 1.2f;
            shieldMain.startSpeed = 0.15f;
            var shieldEmission = _shield.emission;
            shieldEmission.rateOverTime = 0f;
            var shieldShape = _shield.shape;
            shieldShape.shapeType = ParticleSystemShapeType.Circle;
            shieldShape.radius = 2.1f;
            shieldShape.rotation = new Vector3(-90f, 0f, 0f);
            _shield.Stop(true, ParticleSystemStopBehavior.StopEmittingAndClear);
        }

        ParticleSystem MakeBurst(string name, Color color, float rate, float size, bool loop)
        {
            var go = new GameObject(name);
            go.transform.SetParent(transform, false);
            var ps = go.AddComponent<ParticleSystem>();
            var main = ps.main;
            main.loop = loop;
            main.startLifetime = 1.4f;
            main.startSpeed = 0.4f;
            main.startSize = size;
            main.startColor = color;
            main.maxParticles = 80;
            main.simulationSpace = ParticleSystemSimulationSpace.World;
            var emission = ps.emission;
            emission.rateOverTime = rate;
            var renderer = go.GetComponent<ParticleSystemRenderer>();
            renderer.material = GnomeAssets.ParticleMaterial();
            renderer.renderMode = ParticleSystemRenderMode.Billboard;
            return ps;
        }

        public void SetHealth(float ratio)
        {
            _hp = Mathf.Clamp01(ratio);
        }

        public void Hit()
        {
            _hitPulse = 1f;
        }

        public void SetShield(bool on)
        {
            if (_shieldOn == on) return;
            _shieldOn = on;
            if (on) _shield.Play();
            else _shield.Stop(true, ParticleSystemStopBehavior.StopEmitting);
            var emission = _shield.emission;
            emission.rateOverTime = on ? 28f : 0f;
        }

        public void Celebrate()
        {
            HitFx.Spawn(transform.position + Vector3.up * 2.4f, new Color(1f, 0.85f, 0.3f), 40);
        }

        void Update()
        {
            _hitPulse = Mathf.MoveTowards(_hitPulse, 0f, Time.deltaTime * 2.4f);
            float pulse = 0.72f + 0.28f * (0.5f + 0.5f * Mathf.Sin(Time.time * 4.2f));
            float healthMul = 0.35f + 0.65f * _hp;
            float hitMul = 1f + _hitPulse * 1.4f;

            if (_core != null)
                _core.intensity = (4.2f + 2.6f * _hp) * (1f + _hitPulse);

            if (_fairy != null)
            {
                for (int i = 0; i < _fairy.Length; i++)
                {
                    if (_fairy[i] == null) continue;
                    float flicker = 0.75f + 0.25f * Mathf.Sin(Time.time * (3.5f + i * 0.37f) + i);
                    _fairy[i].intensity = 1.15f * pulse * healthMul * hitMul * flicker;
                    _fairy[i].enabled = _hp > 0.02f;
                }
            }

            if (_visual != null)
            {
                float sway = Mathf.Sin(Time.time * 0.7f) * 1.6f;
                _visual.localRotation = _visualBaseRot * Quaternion.Euler(0f, 0f, sway);
                float squash = 1f - _hitPulse * 0.08f;
                _visual.localScale = new Vector3(_visualScale.x * (2f - squash), _visualScale.y * squash, _visualScale.z * (2f - squash));
            }

            var sparkleEmission = _sparkle.emission;
            sparkleEmission.rateOverTime = 6f + 16f * _hp;
        }
    }
}
