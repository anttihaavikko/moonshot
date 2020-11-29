using System.Collections.Generic;
using System.Linq;
using UnityEditor;
using UnityEditor.Callbacks;
using UnityEngine;

namespace Anima2D
{
    [InitializeOnLoad]
    public class EditorEventHandler
    {
        private static List<SpriteMeshInstance> s_SpriteMeshInstances = new List<SpriteMeshInstance>();

        private static SpriteMesh spriteMesh;
        private static SpriteMeshInstance instance;
        private static SpriteMeshInstance currentDestination;
        private static List<Bone2D> s_InstanceBones = new List<Bone2D>();
        private static bool init;
        private static Vector3 instancePosition = Vector3.zero;
        private static Transform parentTransform;

        static EditorEventHandler()
        {
            EditorCallbacks.onSceneGUIDelegate += OnSceneGUI;
            EditorCallbacks.hierarchyWindowItemOnGUI += HierarchyWindowItemCallback;
            EditorCallbacks.hierarchyChanged += HierarchyChanged;
        }

        private static SpriteMesh GetSpriteMesh()
        {
            SpriteMesh l_spriteMesh = null;

            if (DragAndDrop.objectReferences.Length > 0)
            {
                var obj = DragAndDrop.objectReferences[0];

                l_spriteMesh = obj as SpriteMesh;
            }

            return l_spriteMesh;
        }

        private static void Cleanup()
        {
            init = false;
            spriteMesh = null;
            instance = null;
            currentDestination = null;
            parentTransform = null;
            s_InstanceBones.Clear();
        }

        private static Vector3 GetMouseWorldPosition()
        {
            var mouseRay = HandleUtility.GUIPointToWorldRay(Event.current.mousePosition);
            var rootPlane = new Plane(Vector3.forward, Vector3.zero);

            var distance = 0f;
            var mouseWorldPos = Vector3.zero;

            if (rootPlane.Raycast(mouseRay, out distance)) mouseWorldPos = mouseRay.GetPoint(distance);

            return mouseWorldPos;
        }

        private static void CreateInstance()
        {
            instance = SpriteMeshUtils.CreateSpriteMeshInstance(spriteMesh, false);

            if (instance)
            {
                s_InstanceBones = instance.bones;

                instance.transform.parent = parentTransform;

                if (parentTransform) instance.transform.localPosition = Vector3.zero;
            }
        }

        [DidReloadScripts]
        private static void HierarchyChanged()
        {
            s_SpriteMeshInstances = EditorExtra.FindComponentsOfType<SpriteMeshInstance>().ToList();
        }

        private static void HierarchyWindowItemCallback(int pID, Rect pRect)
        {
            instancePosition = Vector3.zero;
            GameObject parent = null;

            if (pRect.Contains(Event.current.mousePosition))
            {
                parent = EditorUtility.InstanceIDToObject(pID) as GameObject;

                if (parent) parentTransform = parent.transform;
            }

            HandleDragAndDrop(false, parentTransform);
        }

        private static void OnSceneGUI(SceneView sceneview)
        {
            instancePosition = GetMouseWorldPosition();
            HandleDragAndDrop(true, null);
        }

        private static SpriteMeshInstance GetClosestBindeableIntersectingSpriteMeshInstance()
        {
            var minDistance = float.MaxValue;
            SpriteMeshInstance closestSpriteMeshInstance = null;

            foreach (var spriteMeshInstance in s_SpriteMeshInstances)
                if (spriteMeshInstance && spriteMeshInstance != instance && spriteMeshInstance.spriteMesh &&
                    spriteMeshInstance.cachedRenderer)
                {
                    var guiRay = HandleUtility.GUIPointToWorldRay(Event.current.mousePosition);

                    if (spriteMeshInstance.cachedRenderer.bounds.IntersectRay(guiRay))
                        if (Bindeable(instance, spriteMeshInstance))
                        {
                            var guiCenter =
                                HandleUtility.WorldToGUIPoint(spriteMeshInstance.cachedRenderer.bounds.center);
                            var distance = (Event.current.mousePosition - guiCenter).sqrMagnitude;
                            if (distance < minDistance) closestSpriteMeshInstance = spriteMeshInstance;
                        }
                }

            return closestSpriteMeshInstance;
        }

        private static int FindBindInfo(BindInfo bindInfo, SpriteMeshInstance spriteMeshInstance)
        {
            if (spriteMeshInstance)
                return FindBindInfo(bindInfo, SpriteMeshUtils.LoadSpriteMeshData(spriteMeshInstance.spriteMesh));

            return -1;
        }

        private static int FindBindInfo(BindInfo bindInfo, SpriteMeshData spriteMeshData)
        {
            if (bindInfo && spriteMeshData)
                for (var i = 0; i < spriteMeshData.bindPoses.Length; ++i)
                {
                    var l_bindInfo = spriteMeshData.bindPoses[i];

                    if (bindInfo.name ==
                        l_bindInfo.name /*&& Mathf.Approximately(bindInfo.boneLength,l_bindInfo.boneLength)*/) return i;
                }

            return -1;
        }

        private static bool Bindeable(SpriteMeshInstance targetSpriteMeshInstance,
            SpriteMeshInstance destinationSpriteMeshInstance)
        {
            var bindeable = false;

            if (targetSpriteMeshInstance &&
                destinationSpriteMeshInstance &&
                targetSpriteMeshInstance.spriteMesh &&
                destinationSpriteMeshInstance.spriteMesh &&
                targetSpriteMeshInstance.spriteMesh != destinationSpriteMeshInstance.spriteMesh &&
                destinationSpriteMeshInstance.cachedSkinnedRenderer)
            {
                var targetData = SpriteMeshUtils.LoadSpriteMeshData(targetSpriteMeshInstance.spriteMesh);
                var destinationData = SpriteMeshUtils.LoadSpriteMeshData(destinationSpriteMeshInstance.spriteMesh);

                bindeable = true;

                if (destinationData.bindPoses.Length >= targetData.bindPoses.Length)
                {
                    for (var i = 0; i < targetData.bindPoses.Length; ++i)
                        if (bindeable)
                        {
                            var bindInfo = targetData.bindPoses[i];

                            if (FindBindInfo(bindInfo, destinationData) < 0) bindeable = false;
                        }
                }
                else
                {
                    bindeable = false;
                }
            }

            return bindeable;
        }

        private static void HandleDragAndDrop(bool createOnEnter, Transform parent)
        {
            switch (Event.current.type)
            {
                case EventType.DragUpdated:

                    if (!init)
                    {
                        spriteMesh = GetSpriteMesh();

                        if (createOnEnter)
                        {
                            parentTransform = null;
                            CreateInstance();
                        }

                        Event.current.Use();

                        init = true;
                    }

                    if (instance)
                    {
                        instance.transform.position = instancePosition;

                        var l_currentDestination = GetClosestBindeableIntersectingSpriteMeshInstance();

                        if (currentDestination != l_currentDestination)
                        {
                            currentDestination = l_currentDestination;

                            if (currentDestination)
                            {
                                var destinationBones = currentDestination.bones;
                                var newBones = new List<Bone2D>();

                                var data = SpriteMeshUtils.LoadSpriteMeshData(instance.spriteMesh);

                                for (var i = 0; i < data.bindPoses.Length; ++i)
                                {
                                    var bindInfo = data.bindPoses[i];
                                    var index = FindBindInfo(bindInfo, currentDestination);
                                    if (index >= 0 && index < destinationBones.Count)
                                        newBones.Add(destinationBones[index]);
                                }

                                instance.transform.parent = currentDestination.transform.parent;
                                instance.bones = newBones;
                                SpriteMeshUtils.UpdateRenderer(instance, false);

                                foreach (var bone in s_InstanceBones)
                                {
                                    bone.hideFlags = HideFlags.HideAndDontSave;
                                    bone.gameObject.SetActive(false);
                                }
                            }
                            else
                            {
                                foreach (var bone in s_InstanceBones)
                                {
                                    bone.hideFlags = HideFlags.None;
                                    bone.gameObject.SetActive(true);
                                }

                                instance.transform.parent = null;
                                instance.bones = s_InstanceBones;
                                SpriteMeshUtils.UpdateRenderer(instance, false);
                            }

                            SceneView.RepaintAll();
                        }
                    }

                    break;

                case EventType.DragExited:

                    if (instance)
                    {
                        Object.DestroyImmediate(instance.gameObject);
                        Event.current.Use();
                    }

                    Cleanup();
                    break;

                case EventType.DragPerform:

                    if (!createOnEnter) CreateInstance();

                    if (instance)
                    {
                        if (currentDestination)
                            foreach (var bone in s_InstanceBones)
                                if (bone)
                                    Object.DestroyImmediate(bone.gameObject);

                        Undo.RegisterCreatedObjectUndo(instance.gameObject, "create SpriteMeshInstance");
                    }

                    Cleanup();
                    break;
            }

            if (instance) DragAndDrop.visualMode = DragAndDropVisualMode.Copy;
        }
    }
}