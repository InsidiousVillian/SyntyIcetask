using UnityEngine;

namespace GnomeGuard
{
    public class WorldContext
    {
        public Transform Root;
        public Transform Tree;
        public FirstPersonPlayer Player;
        public CameraShake Shake;
        public float ArenaRadius = 18f;
        public Camera GameCamera;
        public Camera SceneCamera;
        public TreeFx TreeFx;
        public Transform Enemies;
        public Transform Projectiles;
        public Transform Pickups;
        public Transform Fx;
    }

    public static class WorldBuilder
    {
        public static WorldContext Build(Transform root)
        {
            var world = new WorldContext { Root = root };

            StyleEnvironment();
            BuildGround(root, world.ArenaRadius);
            BuildWalls(root, world.ArenaRadius);
            world.Tree = BuildTree(root, out world.TreeFx);
            BuildSky(root);
            BuildSnow(root, world.ArenaRadius);
            BuildYardLights(root, world.Tree);
            world.Player = BuildPlayer(root, world);
            world.Enemies = ActorFolder.Create(root, "Enemies");
            world.Projectiles = ActorFolder.Create(root, "Projectiles");
            world.Pickups = ActorFolder.Create(root, "Pickups");
            world.Fx = ActorFolder.Create(root, "Fx");

            return world;
        }

        static void StyleEnvironment()
        {
            RenderSettings.fog = true;
            RenderSettings.fogMode = FogMode.ExponentialSquared;
            RenderSettings.fogColor = new Color(0.55f, 0.66f, 0.82f);
            RenderSettings.fogDensity = 0.018f;
            RenderSettings.ambientMode = UnityEngine.Rendering.AmbientMode.Trilight;
            RenderSettings.ambientSkyColor = new Color(0.45f, 0.55f, 0.75f);
            RenderSettings.ambientEquatorColor = new Color(0.32f, 0.36f, 0.42f);
            RenderSettings.ambientGroundColor = new Color(0.18f, 0.2f, 0.24f);

            var light = Object.FindFirstObjectByType<Light>();
            if (light != null && light.type == LightType.Directional)
            {
                light.color = new Color(0.72f, 0.82f, 1f);
                light.intensity = 1.15f;
                light.transform.rotation = Quaternion.Euler(38f, -35f, 0f);
            }
        }

        static void BuildGround(Transform root, float radius)
        {
            var floor = new GameObject("Floor");
            floor.transform.SetParent(root, false);
            floor.transform.position = new Vector3(0f, -0.5f, 0f);
            var box = floor.AddComponent<BoxCollider>();
            box.size = new Vector3(radius * 3f, 1f, radius * 3f);

            var source = GnomeAssets.SpawnFbx("FBX/SM_Env_Ground_Snow__01.fbx", GnomeAssets.GroundMaterial(), root);
            if (source == null) return;

            GnomeAssets.FitFootprint(source, 10f);
            GnomeAssets.SnapTopTo(source, 0.02f);
            source.name = "SnowTile_0";

            var tileBounds = GnomeAssets.Encapsulate(source);
            float tile = Mathf.Max(tileBounds.size.x, tileBounds.size.z);
            if (tile < 0.5f) tile = 10f;

            int count = Mathf.CeilToInt((radius * 2.4f) / tile);
            Vector3 origin = source.transform.position;

            for (int x = -count; x <= count; x++)
            {
                for (int z = -count; z <= count; z++)
                {
                    if (x == 0 && z == 0) continue;
                    var tileGo = Object.Instantiate(source, root);
                    tileGo.name = $"SnowTile_{x}_{z}";
                    tileGo.transform.position = origin + new Vector3(x * tile, 0f, z * tile);
                }
            }
        }

        static void BuildWalls(Transform root, float radius)
        {
            float h = 4f;
            float t = 1.2f;
            float span = radius * 2f + 4f;
            MakeWall(root, new Vector3(0f, h * 0.5f, radius + t * 0.5f), new Vector3(span, h, t));
            MakeWall(root, new Vector3(0f, h * 0.5f, -radius - t * 0.5f), new Vector3(span, h, t));
            MakeWall(root, new Vector3(radius + t * 0.5f, h * 0.5f, 0f), new Vector3(t, h, span));
            MakeWall(root, new Vector3(-radius - t * 0.5f, h * 0.5f, 0f), new Vector3(t, h, span));
        }

        static void MakeWall(Transform root, Vector3 pos, Vector3 size)
        {
            var go = new GameObject("Wall");
            go.transform.SetParent(root, false);
            go.transform.position = pos;
            var box = go.AddComponent<BoxCollider>();
            box.size = size;
        }

        static Transform BuildTree(Transform root, out TreeFx fx)
        {
            var holder = new GameObject("ChristmasTree");
            holder.transform.SetParent(root, false);
            holder.transform.position = Vector3.zero;

            var tree = GnomeAssets.SpawnFbx("FBX/SM_Prop_Christmas_Tree_01.fbx", GnomeAssets.TreeMaterial(), holder.transform);
            if (tree == null)
            {
                tree = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
                tree.transform.SetParent(holder.transform, false);
                tree.transform.localScale = new Vector3(1.4f, 3f, 1.4f);
                tree.transform.localPosition = new Vector3(0f, 3f, 0f);
                Object.Destroy(tree.GetComponent<Collider>());
            }
            else
            {
                GnomeAssets.NormalizeHeight(tree, 6.2f);
                GnomeAssets.SnapFeetTo(tree, 0f);
            }

            var trigger = holder.AddComponent<SphereCollider>();
            trigger.isTrigger = true;
            trigger.center = new Vector3(0f, 1.5f, 0f);
            trigger.radius = 1.75f;

            var lightGo = new GameObject("TreeLight");
            lightGo.transform.SetParent(holder.transform, false);
            lightGo.transform.localPosition = new Vector3(0f, 3.1f, 0f);
            var point = lightGo.AddComponent<Light>();
            point.type = LightType.Point;
            point.color = new Color(1f, 0.72f, 0.35f);
            point.intensity = 6.5f;
            point.range = 14f;
            point.shadows = LightShadows.Soft;

            fx = TreeFx.Add(holder.transform);
            WorldLabel(holder.transform, "PROTECT THIS TREE", new Vector3(0f, 6.4f, 0f), 0.08f, new Color(1f, 0.85f, 0.35f));
            return holder.transform;
        }

        static void WorldLabel(Transform parent, string text, Vector3 localPos, float size, Color color)
        {
            var go = new GameObject("WorldLabel");
            go.transform.SetParent(parent, false);
            go.transform.localPosition = localPos;
            var tm = go.AddComponent<TextMesh>();
            tm.text = text;
            tm.fontSize = 48;
            tm.characterSize = size;
            tm.anchor = TextAnchor.MiddleCenter;
            tm.alignment = TextAlignment.Center;
            tm.color = color;
            tm.fontStyle = FontStyle.Bold;
            go.AddComponent<Billboard>();
        }

        static void BuildSky(Transform root)
        {
            var sky = GnomeAssets.SpawnFbx("FBX/SM_Env_Skybox_01.fbx", GnomeAssets.SkyMaterial(), root);
            if (sky == null) return;
            sky.name = "SkyDome";
            sky.transform.position = Vector3.zero;
            GnomeAssets.FitFootprint(sky, 90f);
            foreach (var renderer in sky.GetComponentsInChildren<Renderer>())
            {
                renderer.shadowCastingMode = UnityEngine.Rendering.ShadowCastingMode.Off;
                renderer.receiveShadows = false;
            }
        }

        static void BuildSnow(Transform root, float radius)
        {
            var go = new GameObject("FallingSnow");
            go.transform.SetParent(root, false);
            go.transform.position = new Vector3(0f, 11f, 0f);

            var ps = go.AddComponent<ParticleSystem>();
            var main = ps.main;
            main.loop = true;
            main.duration = 5f;
            main.startLifetime = 7f;
            main.startSpeed = 0.4f;
            main.startSize = 0.07f;
            main.startColor = new Color(1f, 1f, 1f, 0.85f);
            main.maxParticles = 1800;
            main.simulationSpace = ParticleSystemSimulationSpace.World;
            main.gravityModifier = 0.12f;

            var emission = ps.emission;
            emission.rateOverTime = 140f;

            var shape = ps.shape;
            shape.enabled = true;
            shape.shapeType = ParticleSystemShapeType.Box;
            shape.scale = new Vector3(radius * 2.2f, 1f, radius * 2.2f);

            var vel = ps.velocityOverLifetime;
            vel.enabled = true;
            vel.x = new ParticleSystem.MinMaxCurve(-0.35f, 0.35f);
            vel.z = new ParticleSystem.MinMaxCurve(-0.35f, 0.35f);

            var renderer = go.GetComponent<ParticleSystemRenderer>();
            renderer.material = GnomeAssets.ParticleMaterial();
            renderer.renderMode = ParticleSystemRenderMode.Billboard;
        }

        static void BuildYardLights(Transform root, Transform tree)
        {
            Vector3[] spots =
            {
                new Vector3(4.5f, 0f, 3.2f),
                new Vector3(-4.2f, 0f, 3.8f),
                new Vector3(3.8f, 0f, -4.4f),
                new Vector3(-5.1f, 0f, -3.4f),
                new Vector3(6.2f, 0f, 0.4f),
                new Vector3(-6.0f, 0f, 1.6f),
                new Vector3(0.8f, 0f, 5.6f)
            };

            GnomeKind[] decor =
            {
                GnomeKind.Santa, GnomeKind.Dwarf, GnomeKind.Wizard, GnomeKind.Soldier,
                GnomeKind.Beach, GnomeKind.Theorist, GnomeKind.Basalt
            };
            for (int i = 0; i < spots.Length; i++)
            {
                var holder = new GameObject("YardGnome_" + decor[i]);
                holder.transform.SetParent(root, false);
                holder.transform.position = spots[i];
                Vector3 look = tree.position - spots[i];
                look.y = 0f;
                if (look.sqrMagnitude > 0.001f)
                    holder.transform.rotation = Quaternion.LookRotation(look.normalized);

                var visual = GnomeAssets.SpawnGnomeVisual(decor[i], holder.transform);
                if (visual == null) continue;
                visual.transform.localPosition = Vector3.zero;
                GnomeAssets.NormalizeHeight(visual, 0.85f);
                GnomeAssets.SnapFeetTo(visual, 0f);
                foreach (var col in visual.GetComponentsInChildren<Collider>())
                    Object.Destroy(col);
            }
        }

        static FirstPersonPlayer BuildPlayer(Transform root, WorldContext world)
        {
            var sceneCam = Camera.main;
            if (sceneCam != null)
            {
                world.SceneCamera = sceneCam;
                sceneCam.enabled = false;
                var listener = sceneCam.GetComponent<AudioListener>();
                if (listener != null) listener.enabled = false;
            }

            var playerGo = new GameObject("Player");
            playerGo.transform.SetParent(root, false);
            playerGo.transform.position = new Vector3(0f, 0.08f, -7.5f);
            playerGo.transform.rotation = Quaternion.identity;

            var controller = playerGo.AddComponent<CharacterController>();
            controller.height = 1.8f;
            controller.radius = 0.38f;
            controller.center = new Vector3(0f, 0.9f, 0f);
            controller.minMoveDistance = 0f;
            controller.slopeLimit = 50f;

            var head = new GameObject("Head");
            head.transform.SetParent(playerGo.transform, false);
            head.transform.localPosition = new Vector3(0f, 1.62f, 0f);

            var camGo = new GameObject("GameCamera");
            camGo.tag = "MainCamera";
            camGo.transform.SetParent(head.transform, false);

            var cam = camGo.AddComponent<Camera>();
            cam.fieldOfView = 70f;
            cam.nearClipPlane = 0.07f;
            cam.farClipPlane = 180f;
            cam.clearFlags = CameraClearFlags.Skybox;
            cam.backgroundColor = new Color(0.42f, 0.55f, 0.72f);
            camGo.AddComponent<AudioListener>();
            var urp = camGo.AddComponent<UnityEngine.Rendering.Universal.UniversalAdditionalCameraData>();
            urp.renderPostProcessing = true;
            world.GameCamera = cam;
            world.Shake = camGo.AddComponent<CameraShake>();

            var player = playerGo.AddComponent<FirstPersonPlayer>();
            player.Controller = controller;
            player.Head = head.transform;
            player.Camera = cam;
            return player;
        }
    }
}
