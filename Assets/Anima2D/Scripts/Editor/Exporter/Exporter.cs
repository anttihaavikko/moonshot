using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

namespace Anima2D
{
    public class Exporter
    {
        [MenuItem("Window/Anima2D/Export Prefab", true)]
        private static bool ExportValidate()
        {
            SpriteMeshInstance[] spriteMeshInstances = null;

            if (Selection.activeGameObject)
                spriteMeshInstances = Selection.activeGameObject.GetComponentsInChildren<SpriteMeshInstance>(true);

            return !Application.isPlaying && spriteMeshInstances != null && spriteMeshInstances.Length > 0;
        }

        [MenuItem("Window/Anima2D/Export Prefab", false, 40)]
        private static void Export()
        {
            var path = EditorUtility.SaveFilePanelInProject("Export", Selection.activeGameObject.name + ".prefab",
                "prefab", "Export to prefab");

            if (path.Length <= 0) return;

            var instance = Object.Instantiate(Selection.activeGameObject);

#if UNITY_2018_3_OR_NEWER
            var prefab = PrefabUtility.SaveAsPrefabAsset(instance, path);
#else
			GameObject prefab = PrefabUtility.CreatePrefab(path,instance);
#endif

            Object.DestroyImmediate(instance);

            var spriteMeshInstances = new List<SpriteMeshInstance>();

            prefab.GetComponentsInChildren(true, spriteMeshInstances);

            foreach (var spriteMeshInstance in spriteMeshInstances)
                if (spriteMeshInstance.spriteMesh &&
                    spriteMeshInstance.spriteMesh.sprite)
                {
                    if (spriteMeshInstance.spriteMesh.sharedMesh)
                    {
                        var mesh = Object.Instantiate(spriteMeshInstance.spriteMesh.sharedMesh);

                        mesh.name = spriteMeshInstance.spriteMesh.sharedMesh.name;

                        AssetDatabase.AddObjectToAsset(mesh, prefab);

                        if (spriteMeshInstance.cachedMeshFilter)
                            spriteMeshInstance.cachedMeshFilter.sharedMesh = mesh;
                        else if (spriteMeshInstance.cachedSkinnedRenderer)
                            spriteMeshInstance.cachedSkinnedRenderer.sharedMesh = mesh;
                    }

                    if (spriteMeshInstance.sharedMaterial)
                    {
                        var material = Object.Instantiate(spriteMeshInstance.sharedMaterial);

                        material.name = spriteMeshInstance.name;
                        material.mainTexture = spriteMeshInstance.spriteMesh.sprite.texture;
                        material.color = spriteMeshInstance.color;

                        AssetDatabase.AddObjectToAsset(material, prefab);

                        if (spriteMeshInstance.cachedRenderer)
                            spriteMeshInstance.cachedRenderer.sharedMaterial = material;
                    }
                }

            DestroyComponents<SpriteMeshInstance>(prefab);
            DestroyComponents<SpriteMeshAnimation>(prefab);
            DestroyComponents<Ik2D>(prefab);
            DestroyComponents<IkGroup>(prefab);
            DestroyComponents<Control>(prefab);
            DestroyComponents<Bone2D>(prefab);
            DestroyComponents<PoseManager>(prefab);

            EditorUtility.SetDirty(prefab);

            AssetDatabase.SaveAssets();
        }

        private static void DestroyComponents<T>(GameObject gameObject) where T : MonoBehaviour
        {
            var components = new List<T>();

            gameObject.GetComponentsInChildren(true, components);

            foreach (var component in components) Object.DestroyImmediate(component, true);
        }
    }
}