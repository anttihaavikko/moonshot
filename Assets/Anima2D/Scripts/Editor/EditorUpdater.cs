using System.Collections.Generic;
using System.Linq;
using UnityEditor;
using UnityEditor.Callbacks;
using UnityEngine;

namespace Anima2D
{
    [InitializeOnLoad]
    internal class EditorCallbacks
    {
        public delegate void CallbackFunction();

        public delegate void HierarchyWindowItemCallback(int instanceID, Rect selectionRect);

        public delegate void OnSceneFunc(SceneView sceneView);

        public static CallbackFunction update;
        public static OnSceneFunc onSceneGUIDelegate;
        public static CallbackFunction hierarchyChanged;
        public static CallbackFunction undoRedoPerformed;
        public static HierarchyWindowItemCallback hierarchyWindowItemOnGUI;

        static EditorCallbacks()
        {
            EditorApplication.update += () => { update(); };
#if UNITY_2019_1_OR_NEWER
            SceneView.duringSceneGui += sv => { onSceneGUIDelegate(sv); };
#else
			SceneView.onSceneGUIDelegate += (sv) => { onSceneGUIDelegate(sv); };
#endif
#if UNITY_2018_1_OR_NEWER
            EditorApplication.hierarchyChanged += () => { hierarchyChanged(); };
#else
			EditorApplication.hierarchyWindowChanged += () => { hierarchyChanged(); };
#endif
            EditorApplication.hierarchyWindowItemOnGUI = (i, r) => { hierarchyWindowItemOnGUI(i, r); };
            Undo.undoRedoPerformed += () => { undoRedoPerformed(); };
        }
    }

    [InitializeOnLoad]
    [ExecuteInEditMode]
    public class EditorUpdater
    {
        private static bool s_Dirty = true;
        private static string s_UndoName = "";
        private static bool s_DraggingATool;
        private static List<Ik2D> s_Ik2Ds = new List<Ik2D>();
        private static List<Bone2D> s_Bones = new List<Bone2D>();
        private static List<Control> s_Controls = new List<Control>();
        private static bool s_InAnimationMode;
        private static float s_OldAnimationTime;
        private static float s_LastUpdate;
        private static int s_LastNearestControl = -1;

        static EditorUpdater()
        {
            EditorCallbacks.update += Update;
            EditorCallbacks.onSceneGUIDelegate += OnSceneGUI;
            EditorCallbacks.hierarchyChanged += HierarchyChanged;
            EditorCallbacks.undoRedoPerformed += UndoRedoPerformed;
        }

        [DidReloadScripts]
        private static void HierarchyChanged()
        {
            s_Ik2Ds = EditorExtra.FindComponentsOfType<Ik2D>().ToList();
            s_Bones = EditorExtra.FindComponentsOfType<Bone2D>().ToList();
            s_Controls = EditorExtra.FindComponentsOfType<Control>().ToList();
        }

        private static void UndoRedoPerformed()
        {
            foreach (var bone in s_Bones)
                if (bone)
                    bone.attachedIK = null;

            SetDirty();

            EditorApplication.delayCall += () => { SceneView.RepaintAll(); };
        }

        public static void SetDirty()
        {
            SetDirty("");
        }

        public static void SetDirty(string undoName)
        {
            s_UndoName = undoName;
            s_Dirty = true;
        }

        public static void Update(string undoName, bool record)
        {
            var updatedIKs = new List<Ik2D>();

            for (var i = 0; i < s_Ik2Ds.Count; i++)
            {
                var ik2D = s_Ik2Ds[i];

                if (ik2D && !updatedIKs.Contains(ik2D))
                {
                    var ikList = IkUtils.UpdateIK(ik2D, undoName, record);

                    if (ikList != null)
                    {
                        updatedIKs.AddRange(ikList);
                        updatedIKs = updatedIKs.Distinct().ToList();
                    }
                }
            }

            foreach (var control in s_Controls)
                if (control && control.isActiveAndEnabled && control.bone)
                {
                    control.transform.position = control.bone.transform.position;
                    control.transform.rotation = control.bone.transform.rotation;
                }
        }

        private static void AnimationModeCheck()
        {
            if (s_InAnimationMode != AnimationMode.InAnimationMode())
            {
                SetDirty();
                s_InAnimationMode = AnimationMode.InAnimationMode();
            }
        }

        private static void AnimationWindowTimeCheck()
        {
            var currentAnimationTime = AnimationWindowExtra.currentTime;

            if (s_OldAnimationTime != currentAnimationTime) SetDirty();

            s_OldAnimationTime = currentAnimationTime;
        }

        private static void OnSceneGUI(SceneView sceneview)
        {
            if (!s_DraggingATool &&
                GUIUtility.hotControl != 0 &&
                !ToolsExtra.viewToolActive)
                s_DraggingATool = Event.current.type == EventType.MouseDrag;

            Gizmos.OnSceneGUI(sceneview);

            if (s_LastNearestControl != HandleUtility.nearestControl)
            {
                s_LastNearestControl = HandleUtility.nearestControl;
                SceneView.RepaintAll();
            }
        }

        private static void OnLateUpdate()
        {
            if (AnimationMode.InAnimationMode())
            {
                SetDirty();

                UpdateIKs();
            }
        }

        private static void Update()
        {
            EditorUpdaterProxy.Instance.onLateUpdate -= OnLateUpdate;
            EditorUpdaterProxy.Instance.onLateUpdate += OnLateUpdate;

            if (s_DraggingATool)
            {
                s_DraggingATool = false;

                var undoName = "Move";

                if (Tools.current == Tool.Rotate) undoName = "Rotate";
                if (Tools.current == Tool.Scale) undoName = "Scale";

                for (var i = 0; i < Selection.transforms.Length; i++)
                {
                    var transform = Selection.transforms[i];
                    var control = transform.GetComponent<Control>();
                    if (control && control.isActiveAndEnabled && control.bone)
                    {
                        Undo.RecordObject(control.bone.transform, undoName);

                        control.bone.transform.position = control.transform.position;
                        control.bone.transform.rotation = control.transform.rotation;

                        BoneUtils.OrientToChild(control.bone.parentBone, false, undoName, true);
                    }

                    var ik2D = transform.GetComponent<Ik2D>();
                    if (ik2D && ik2D.record) IkUtils.UpdateIK(ik2D, undoName, true);
                }

                SetDirty();
            }

            AnimationModeCheck();
            AnimationWindowTimeCheck();

            IkUtils.UpdateAttachedIKs(s_Ik2Ds);

            UpdateIKs();
        }

        private static void UpdateIKs()
        {
            if (!s_Dirty) return;

            if (s_LastUpdate == Time.realtimeSinceStartup) return;

            Update(s_UndoName, false);

            s_Dirty = false;
            s_UndoName = "";
            s_LastUpdate = Time.realtimeSinceStartup;
        }
    }
}