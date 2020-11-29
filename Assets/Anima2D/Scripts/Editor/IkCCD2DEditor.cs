using UnityEditor;

namespace Anima2D
{
    [CanEditMultipleObjects]
    [CustomEditor(typeof(IkCCD2D))]
    public class IkCCD2DEditor : Ik2DEditor
    {
        public override void OnInspectorGUI()
        {
            var ikCCD2D = target as IkCCD2D;

            base.OnInspectorGUI();

            var numBonesProp = serializedObject.FindProperty("m_NumBones");
            var iterationsProp = serializedObject.FindProperty("iterations");
            var dampingProp = serializedObject.FindProperty("damping");

            var targetBone = ikCCD2D.target;

            serializedObject.Update();

            EditorGUI.BeginDisabledGroup(!targetBone);

            EditorGUI.BeginChangeCheck();

            var chainLength = 0;

            if (targetBone) chainLength = targetBone.chainLength;

            EditorGUILayout.IntSlider(numBonesProp, 0, chainLength);

            if (EditorGUI.EndChangeCheck())
            {
                Undo.RegisterCompleteObjectUndo(ikCCD2D, "Set num bones");

                IkUtils.InitializeIk2D(serializedObject);
                EditorUpdater.SetDirty("Set num bones");
            }

            EditorGUI.EndDisabledGroup();

            EditorGUI.BeginChangeCheck();

            EditorGUILayout.PropertyField(iterationsProp);
            EditorGUILayout.PropertyField(dampingProp);

            if (EditorGUI.EndChangeCheck()) EditorUpdater.SetDirty(Undo.GetCurrentGroupName());

            serializedObject.ApplyModifiedProperties();
        }
    }
}