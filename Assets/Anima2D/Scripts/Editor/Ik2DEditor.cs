using UnityEditor;
using UnityEngine;

namespace Anima2D
{
    [CanEditMultipleObjects]
    [CustomEditor(typeof(Ik2D), true)]
    public class Ik2DEditor : Editor
    {
        private Ik2D m_Ik2D;
        private SerializedProperty m_OrientChildProperty;
        private SerializedProperty m_RecordProperty;
        private SerializedProperty m_RestorePoseProperty;
        private SerializedProperty m_TargetTransformProperty;
        private SerializedProperty m_WeightProperty;

        protected virtual void OnEnable()
        {
            m_Ik2D = target as Ik2D;

            m_RecordProperty = serializedObject.FindProperty("m_Record");
            m_TargetTransformProperty = serializedObject.FindProperty("m_TargetTransform");
            m_WeightProperty = serializedObject.FindProperty("m_Weight");
            m_RestorePoseProperty = serializedObject.FindProperty("m_RestoreDefaultPose");
            m_OrientChildProperty = serializedObject.FindProperty("m_OrientChild");
        }

        public override void OnInspectorGUI()
        {
            serializedObject.Update();

            EditorGUILayout.PropertyField(m_RecordProperty);

            Transform targetTransform = null;

            if (m_Ik2D.target) targetTransform = m_Ik2D.target.transform;

            EditorGUI.BeginChangeCheck();

            var newTargetTransform =
                EditorGUILayout.ObjectField(new GUIContent("Target"), targetTransform, typeof(Transform), true) as
                    Transform;

            if (EditorGUI.EndChangeCheck())
            {
                Undo.RegisterCompleteObjectUndo(m_Ik2D, "set target");

                if (newTargetTransform && !newTargetTransform.GetComponent<Bone2D>()) newTargetTransform = null;

                if (newTargetTransform != targetTransform)
                {
                    m_TargetTransformProperty.objectReferenceValue = newTargetTransform;
                    IkUtils.InitializeIk2D(serializedObject);
                    EditorUpdater.SetDirty("set target");
                }
            }

            /*
            EditorGUI.BeginChangeCheck();

            EditorGUILayout.PropertyField(m_TargetTransformProperty);

            if(EditorGUI.EndChangeCheck())
            {
                IkUtils.InitializeIk2D(serializedObject);

                DoUpdateIK();
            }
            */

            EditorGUI.BeginChangeCheck();

            EditorGUILayout.Slider(m_WeightProperty, 0f, 1f);
            EditorGUILayout.PropertyField(m_RestorePoseProperty);
            EditorGUILayout.PropertyField(m_OrientChildProperty);

            if (EditorGUI.EndChangeCheck()) EditorUpdater.SetDirty(Undo.GetCurrentGroupName());

            serializedObject.ApplyModifiedProperties();
        }
    }
}