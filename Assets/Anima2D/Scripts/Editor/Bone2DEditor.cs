using UnityEditor;
using UnityEngine;

namespace Anima2D
{
    [CanEditMultipleObjects]
    [CustomEditor(typeof(Bone2D))]
    public class Bone2DEditor : Editor
    {
        private SerializedProperty m_AlphaProperty;
        private Bone2D m_Bone;
        private SerializedProperty m_ChildTransformProperty;
        private SerializedProperty m_ColorProperty;
        private SerializedProperty m_LengthProperty;

        private void OnEnable()
        {
            Tools.hidden = Tools.current == Tool.Move;

            m_Bone = target as Bone2D;

            m_ColorProperty = serializedObject.FindProperty("m_Color");
            m_AlphaProperty = m_ColorProperty.FindPropertyRelative("a");

            m_ChildTransformProperty = serializedObject.FindProperty("m_ChildTransform");
            m_LengthProperty = serializedObject.FindProperty("m_Length");
        }

        private void OnDisable()
        {
            Tools.hidden = false;
        }

        private void OnSceneGUI()
        {
            if (Tools.current == Tool.Move)
            {
                Tools.hidden = true;

                var size = HandleUtility.GetHandleSize(m_Bone.transform.position) / 5f;

                var rotation = m_Bone.transform.rotation;

                EditorGUI.BeginChangeCheck();

                var cameraRotation = Camera.current.transform.rotation;

                if (Event.current.type == EventType.Repaint)
                    Camera.current.transform.rotation = m_Bone.transform.rotation;

#if UNITY_5_6_OR_NEWER
                var newPosition = Handles.FreeMoveHandle(m_Bone.transform.position, rotation, size, Vector3.zero,
                    Handles.RectangleHandleCap);
#else
				Vector3 newPosition =
 Handles.FreeMoveHandle(m_Bone.transform.position, rotation, size, Vector3.zero, Handles.RectangleCap);
#endif

                if (Event.current.type == EventType.Repaint) Camera.current.transform.rotation = cameraRotation;

                if (EditorGUI.EndChangeCheck())
                {
                    Undo.RecordObject(m_Bone.transform, "Move");

                    m_Bone.transform.position = newPosition;

                    BoneUtils.OrientToChild(m_Bone.parentBone, Event.current.shift, Undo.GetCurrentGroupName(), true);

                    EditorUtility.SetDirty(m_Bone.transform);

                    EditorUpdater.SetDirty("Move");
                }
            }
            else
            {
                Tools.hidden = false;
            }
        }

        public override void OnInspectorGUI()
        {
            var childChanged = false;

            serializedObject.Update();

            EditorGUILayout.PropertyField(m_ColorProperty);
            EditorGUILayout.Slider(m_AlphaProperty, 0f, 1f, new GUIContent("Alpha"));

            Transform childTransform = null;

            if (m_Bone.child) childTransform = m_Bone.child.transform;

            EditorGUI.BeginDisabledGroup(targets.Length > 1);

            EditorGUI.showMixedValue = targets.Length > 1;

            EditorGUI.BeginChangeCheck();

            var newChildTransform =
                EditorGUILayout.ObjectField(new GUIContent("Child"), childTransform, typeof(Transform), true) as
                    Transform;

            if (EditorGUI.EndChangeCheck())
            {
                if (newChildTransform && (newChildTransform.parent != m_Bone.transform ||
                                          !newChildTransform.GetComponent<Bone2D>())) newChildTransform = null;

                m_ChildTransformProperty.objectReferenceValue = newChildTransform;

                childChanged = true;
            }

            EditorGUI.EndDisabledGroup();

            EditorGUILayout.PropertyField(m_LengthProperty);

            serializedObject.ApplyModifiedProperties();

            if (childChanged) BoneUtils.OrientToChild(m_Bone, true, "set child", false);
        }
    }
}