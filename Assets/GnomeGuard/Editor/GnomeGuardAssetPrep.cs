using UnityEditor;
using UnityEngine;

namespace GnomeGuard.EditorTools
{
    [InitializeOnLoad]
    static class GnomeGuardAssetPrep
    {
        const string Key = "GnomeGuard.AssetPrep.v3";

        static GnomeGuardAssetPrep()
        {
            EditorApplication.delayCall += Prepare;
        }

        static void Prepare()
        {
            if (EditorApplication.isPlayingOrWillChangePlaymode) return;
            if (SessionState.GetBool(Key, false)) return;

            string root = "Assets/POLYGON_Holiday_Gnomes_SourceFiles_v2";
            if (!AssetDatabase.IsValidFolder(root)) return;

            string[] fbxGuids = AssetDatabase.FindAssets("t:Model", new[] { root + "/FBX" });
            if (fbxGuids.Length == 0) return;

            foreach (string guid in fbxGuids)
            {
                string path = AssetDatabase.GUIDToAssetPath(guid);
                var importer = AssetImporter.GetAtPath(path) as ModelImporter;
                if (importer == null) continue;
                bool dirty = false;
                if (importer.animationType != ModelImporterAnimationType.None)
                {
                    importer.animationType = ModelImporterAnimationType.None;
                    dirty = true;
                }
                if (importer.materialImportMode != ModelImporterMaterialImportMode.None)
                {
                    importer.materialImportMode = ModelImporterMaterialImportMode.None;
                    dirty = true;
                }
                if (dirty) importer.SaveAndReimport();
            }

            EnsureBuildScene();
            SessionState.SetBool(Key, true);
        }

        static void EnsureBuildScene()
        {
            const string scene = "Assets/Scenes/SampleScene.unity";
            var scenes = EditorBuildSettings.scenes;
            foreach (var s in scenes)
            {
                if (s.path == scene) return;
            }

            var next = new EditorBuildSettingsScene[scenes.Length + 1];
            scenes.CopyTo(next, 0);
            next[scenes.Length] = new EditorBuildSettingsScene(scene, true);
            EditorBuildSettings.scenes = next;
        }

        [MenuItem("Gnome Guard/Prepare Assets")]
        static void ForcePrepare()
        {
            SessionState.SetBool(Key, false);
            Prepare();
            EditorUtility.DisplayDialog("Gnome Guard", "Assets prepared. Press Play in SampleScene.", "OK");
        }
    }
}
