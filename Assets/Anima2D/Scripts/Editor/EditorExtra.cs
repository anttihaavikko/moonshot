using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEditorInternal;
using UnityEngine;
#if UNITY_5_6_OR_NEWER
using UnityEngine.Rendering;

#endif

namespace Anima2D
{
    public class EditorExtra
    {
        public static GameObject InstantiateForAnimatorPreview(Object original)
        {
            var result = Object.Instantiate(original) as GameObject;

            var behaviours = new List<Behaviour>();
            result.GetComponentsInChildren(false, behaviours);

            foreach (var behaviour in behaviours)
            {
                var spriteMeshInstance = behaviour as SpriteMeshInstance;

                if (spriteMeshInstance && spriteMeshInstance.spriteMesh && spriteMeshInstance.spriteMesh.sprite)
                {
                    var material = spriteMeshInstance.sharedMaterial;

                    if (material)
                    {
                        var materialClone = Object.Instantiate(material);
                        materialClone.hideFlags = HideFlags.HideAndDontSave;
                        materialClone.mainTexture = spriteMeshInstance.spriteMesh.sprite.texture;

                        spriteMeshInstance.sharedMaterial = materialClone;
                        spriteMeshInstance.cachedRenderer.sharedMaterial = materialClone;
                    }
                }

                if (behaviour == null ||
                    behaviour is Ik2D ||
                    behaviour is SpriteMeshAnimation
#if UNITY_5_6_OR_NEWER
                    || behaviour is SortingGroup
#endif
                )
                    continue;
                behaviour.enabled = false;
            }

            return result;
        }

        public static void DestroyAnimatorPreviewInstance(GameObject instance)
        {
            var spriteMeshInstances = new List<SpriteMeshInstance>();
            instance.GetComponentsInChildren(false, spriteMeshInstances);

            foreach (var spriteMeshInstance in spriteMeshInstances)
                if (spriteMeshInstance && spriteMeshInstance.spriteMesh && spriteMeshInstance.spriteMesh.sprite)
                {
                    var materialClone = spriteMeshInstance.sharedMaterial;

                    if (materialClone != null)
                        Object.DestroyImmediate(materialClone);
                }

            Object.DestroyImmediate(instance);
        }

        public static void InitInstantiatedPreviewRecursive(GameObject go)
        {
            go.hideFlags = HideFlags.HideAndDontSave;

            foreach (Transform transform in go.transform) InitInstantiatedPreviewRecursive(transform.gameObject);
        }


        public static List<string> GetSortingLayerNames()
        {
            var names = new List<string>();

            var sortingLayersProperty =
                typeof(InternalEditorUtility).GetProperty("sortingLayerNames",
                    BindingFlags.Static | BindingFlags.NonPublic);
            if (sortingLayersProperty != null)
            {
                var sortingLayers = (string[]) sortingLayersProperty.GetValue(null, new object[0]);
                names.AddRange(sortingLayers);
            }

            return names;
        }

        public static bool IsProSkin()
        {
            var isProSkin = false;

            var prop = typeof(EditorGUIUtility).GetProperty("isProSkin", BindingFlags.Static | BindingFlags.NonPublic);

            if (prop != null) isProSkin = (bool) prop.GetValue(null, new object[0]);

            return isProSkin;
        }

        public static GameObject PickGameObject(Vector2 mousePosition)
        {
            var methodInfo = typeof(EditorWindow).Assembly.GetType("UnityEditor.SceneViewPicking")
                .GetMethod("PickGameObject", BindingFlags.Static | BindingFlags.Public);

            if (methodInfo != null) return (GameObject) methodInfo.Invoke(null, new object[] {mousePosition});

            return null;
        }

        public static T[] FindComponentsOfType<T>() where T : Component
        {
#if UNITY_2018_3_OR_NEWER
            var currentStage = StageUtility.GetCurrentStageHandle();
            return currentStage.FindComponentsOfType<T>()
                .Where(x => x.gameObject.scene.isLoaded && x.gameObject.activeInHierarchy).ToArray();
#else
			return GameObject.FindObjectsOfType<T>();
#endif
        }
    }
}