using System.Collections.Generic;
using UnityEngine;
#if UNITY_EDITOR
using UnityEditor;
#endif

namespace GnomeGuard
{
    public enum GnomeKind
    {
        Zombie,
        Wizard,
        Soldier,
        Santa,
        Dwarf,
        Beach,
        Theorist,
        Basalt
    }

    public static class GnomeAssets
    {
        const string Root = "Assets/POLYGON_Holiday_Gnomes_SourceFiles_v2/";

        static readonly Dictionary<int, Material> Palette = new Dictionary<int, Material>();
        static Material _unlitWhite;
        static Material _snowball;
        static Material _sky;
        static Material _particle;
        static Material _ice;
        static Shader _lit;

        public static bool Ready { get; private set; }
        public static string LastError { get; private set; }

        public static bool Init()
        {
            LastError = null;
            Ready = false;
#if !UNITY_EDITOR
            LastError = "Play Gnome Guard from the Unity Editor so the FBX pack can load.";
            return false;
#else
            _lit = Shader.Find("Universal Render Pipeline/Lit");
            if (_lit == null)
            {
                LastError = "URP Lit shader missing.";
                return false;
            }

            if (LoadTexture("Textures/PolygonXmasGnomes_Texture_01.png") == null)
            {
                LastError = "Holiday Gnome textures are still importing. Wait for Unity, then press Play again.";
                return false;
            }

            if (LoadModel("FBX/SM_Prop_Gnome_Zombie_01.fbx") == null)
            {
                LastError = "Holiday Gnome FBX files are still importing. Wait for Unity, then press Play again.";
                return false;
            }

            Ready = true;
            return true;
#endif
        }

        public static GameObject SpawnFbx(string relativePath, Material material, Transform parent)
        {
#if UNITY_EDITOR
            var source = LoadModel(relativePath);
            if (source == null) return null;
            var go = Object.Instantiate(source, parent);
            go.name = source.name;
            ApplyMaterial(go, material);
            StripAnimation(go);
            StripColliders(go);
            return go;
#else
            return null;
#endif
        }

        public static GameObject SpawnGnomeVisual(GnomeKind kind, Transform parent)
        {
            var go = SpawnFbx(FbxFor(kind), MaterialFor(kind), parent);
            return go;
        }

        public static Material MaterialFor(GnomeKind kind)
        {
            int index = kind switch
            {
                GnomeKind.Wizard => 2,
                GnomeKind.Dwarf => 7,
                GnomeKind.Soldier => 3,
                GnomeKind.Santa => 1,
                GnomeKind.Beach => 5,
                GnomeKind.Theorist => 4,
                GnomeKind.Basalt => 6,
                GnomeKind.Zombie => 8,
                _ => 1
            };
            return GetPalette(index, kind == GnomeKind.Zombie);
        }

        public static Material GroundMaterial() => GetPalette(1, false);
        public static Material TreeMaterial() => GetPalette(1, false);

        public static Material SkyMaterial()
        {
            if (_sky != null) return _sky;
            var shader = Shader.Find("Universal Render Pipeline/Unlit");
            _sky = new Material(shader != null ? shader : _lit);
            _sky.name = "GnomeSky";
            var tex = LoadTexture("Textures/PolygonXmasGnomes_Texture_04.png");
            if (tex != null && shader != null) _sky.SetTexture("_BaseMap", tex);
            Color night = new Color(0.35f, 0.48f, 0.72f);
            _sky.SetColor("_BaseColor", night);
            _sky.SetFloat("_Cull", 0f);
            _sky.renderQueue = 1000;
            return _sky;
        }

        public static Material SnowballMaterial()
        {
            if (_snowball != null) return _snowball;
            _snowball = new Material(_lit);
            _snowball.name = "Snowball";
            _snowball.SetColor("_BaseColor", new Color(0.92f, 0.97f, 1f));
            _snowball.SetFloat("_Smoothness", 0.35f);
            return _snowball;
        }

        public static Material UnlitWhite()
        {
            if (_unlitWhite != null) return _unlitWhite;
            var shader = Shader.Find("Universal Render Pipeline/Unlit");
            _unlitWhite = new Material(shader != null ? shader : _lit);
            _unlitWhite.SetColor("_BaseColor", Color.white);
            return _unlitWhite;
        }

        public static Material ParticleMaterial()
        {
            if (_particle != null) return _particle;
            var shader = Shader.Find("Universal Render Pipeline/Particles/Unlit");
            if (shader == null) shader = Shader.Find("Sprites/Default");
            _particle = new Material(shader);
            _particle.SetColor("_BaseColor", Color.white);
            _particle.color = Color.white;
            return _particle;
        }

        public static Material IceMaterial()
        {
            if (_ice != null) return _ice;
            var shader = Shader.Find("Universal Render Pipeline/Unlit");
            _ice = new Material(shader != null ? shader : _lit);
            _ice.name = "IceShell";
            _ice.SetColor("_BaseColor", new Color(0.55f, 0.9f, 1f, 1f));
            _ice.SetFloat("_Cull", 0f);
            return _ice;
        }

        public static string DisplayName(GnomeKind kind)
        {
            return kind switch
            {
                GnomeKind.Wizard => "WIZARD  Freeze",
                GnomeKind.Soldier => "SOLDIER  Rapid snowballs",
                GnomeKind.Santa => "SANTA  Repair the tree",
                GnomeKind.Dwarf => "DWARF  Score burst",
                GnomeKind.Beach => "BEACH  Slow-mo",
                GnomeKind.Theorist => "THEORIST  Bonus hits",
                GnomeKind.Basalt => "BASALT  Tree shield",
                _ => "ZOMBIE"
            };
        }

        public static string FbxFor(GnomeKind kind)
        {
            return kind switch
            {
                GnomeKind.Wizard => "FBX/SM_Prop_Gnome_Wizard_01.fbx",
                GnomeKind.Soldier => "FBX/SM_Prop_Gnome_Soldier_01.fbx",
                GnomeKind.Santa => "FBX/SM_Prop_Gnome_Santa_01.fbx",
                GnomeKind.Dwarf => "FBX/SM_Prop_Gnome_Dwarf_01.fbx",
                GnomeKind.Beach => "FBX/SM_Prop_Gnome_Beach_01.fbx",
                GnomeKind.Theorist => "FBX/SM_Prop_Gnome_Theorist_01.fbx",
                GnomeKind.Basalt => "FBX/SM_Prop_Gnome_Basalt_01.fbx",
                _ => "FBX/SM_Prop_Gnome_Zombie_01.fbx"
            };
        }

        public static Bounds Encapsulate(GameObject go)
        {
            var renderers = go.GetComponentsInChildren<Renderer>();
            if (renderers.Length == 0) return new Bounds(go.transform.position, Vector3.one);
            var bounds = renderers[0].bounds;
            for (int i = 1; i < renderers.Length; i++) bounds.Encapsulate(renderers[i].bounds);
            return bounds;
        }

        public static void NormalizeHeight(GameObject go, float targetHeight)
        {
            var bounds = Encapsulate(go);
            if (bounds.size.y < 0.0001f) return;
            go.transform.localScale *= targetHeight / bounds.size.y;
        }

        public static void SnapFeetTo(GameObject go, float groundY)
        {
            var bounds = Encapsulate(go);
            go.transform.position += Vector3.up * (groundY - bounds.min.y);
        }

        public static void SnapTopTo(GameObject go, float y)
        {
            var bounds = Encapsulate(go);
            go.transform.position += Vector3.up * (y - bounds.max.y);
        }

        public static void FitFootprint(GameObject go, float targetSize)
        {
            var bounds = Encapsulate(go);
            float size = Mathf.Max(bounds.size.x, bounds.size.z);
            if (size < 0.0001f) return;
            go.transform.localScale *= targetSize / size;
        }

        static Material GetPalette(int index, bool zombieTint)
        {
            int key = index * 10 + (zombieTint ? 1 : 0);
            if (Palette.TryGetValue(key, out var cached) && cached != null) return cached;

            var mat = new Material(_lit);
            mat.name = "GnomePalette_" + index;
            var tex = LoadTexture($"Textures/PolygonXmasGnomes_Texture_{index:00}.png");
            if (tex != null) mat.SetTexture("_BaseMap", tex);

            var emit = LoadTexture("Textures/PolygonXmasGnomes_Emissive_01_A.png");
            if (emit != null)
            {
                mat.EnableKeyword("_EMISSION");
                mat.SetTexture("_EmissionMap", emit);
                mat.SetColor("_EmissionColor", zombieTint ? new Color(0.35f, 1.2f, 0.4f) : new Color(1.2f, 0.85f, 0.35f));
                mat.globalIlluminationFlags = MaterialGlobalIlluminationFlags.RealtimeEmissive;
            }

            mat.SetFloat("_Smoothness", 0.22f);
            mat.SetColor("_BaseColor", zombieTint ? new Color(0.7f, 1f, 0.72f) : Color.white);
            Palette[key] = mat;
            return mat;
        }

        static void ApplyMaterial(GameObject go, Material material)
        {
            if (material == null) return;
            foreach (var renderer in go.GetComponentsInChildren<Renderer>())
            {
                var slots = renderer.sharedMaterials;
                for (int i = 0; i < slots.Length; i++) slots[i] = material;
                renderer.sharedMaterials = slots;
            }
        }

        static void StripAnimation(GameObject go)
        {
            foreach (var animator in go.GetComponentsInChildren<Animator>())
                Object.Destroy(animator);
            foreach (var anim in go.GetComponentsInChildren<Animation>())
                Object.Destroy(anim);
        }

        static void StripColliders(GameObject go)
        {
            foreach (var col in go.GetComponentsInChildren<Collider>())
                Object.Destroy(col);
        }

#if UNITY_EDITOR
        static Texture2D LoadTexture(string relative)
        {
            return AssetDatabase.LoadAssetAtPath<Texture2D>(Root + relative);
        }

        static GameObject LoadModel(string relative)
        {
            return AssetDatabase.LoadAssetAtPath<GameObject>(Root + relative);
        }
#else
        static Texture2D LoadTexture(string relative) => null;
        static GameObject LoadModel(string relative) => null;
#endif
    }
}
