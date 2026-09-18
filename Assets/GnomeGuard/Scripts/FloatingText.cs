using UnityEngine;

namespace GnomeGuard
{
    public class FloatingText : MonoBehaviour
    {
        TextMesh _mesh;
        Color _color;
        float _life = 0.85f;
        Vector3 _vel;

        public static void Spawn(Vector3 position, string text, Color color)
        {
            var go = new GameObject("Popup");
            go.transform.position = position + Vector3.up * 1.2f;
            if (GnomeGuardGame.Instance != null)
                go.transform.SetParent(GnomeGuardGame.Instance.transform, true);

            var mesh = go.AddComponent<TextMesh>();
            mesh.text = text;
            mesh.fontSize = 42;
            mesh.characterSize = 0.05f;
            mesh.anchor = TextAnchor.MiddleCenter;
            mesh.alignment = TextAlignment.Center;
            mesh.fontStyle = FontStyle.Bold;
            mesh.color = color;

            var floating = go.AddComponent<FloatingText>();
            floating._mesh = mesh;
            floating._color = color;
            floating._vel = Vector3.up * 1.6f + Random.insideUnitSphere * 0.2f;
            go.AddComponent<Billboard>();
        }

        void Update()
        {
            _life -= Time.deltaTime;
            transform.position += _vel * Time.deltaTime;
            _vel = Vector3.Lerp(_vel, Vector3.up * 0.4f, Time.deltaTime * 2f);
            float a = Mathf.Clamp01(_life / 0.25f);
            _mesh.color = new Color(_color.r, _color.g, _color.b, a);
            transform.localScale = Vector3.one * (1.05f + (0.85f - _life) * 0.35f);
            if (_life <= 0f) Destroy(gameObject);
        }
    }
}
