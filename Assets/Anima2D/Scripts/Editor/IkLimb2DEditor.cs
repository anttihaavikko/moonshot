using UnityEditor;

namespace Anima2D
{
    [CustomEditor(typeof(IkLimb2D))]
    public class IkLimb2DEditor : Ik2DEditor
    {
        public override void OnInspectorGUI()
        {
            base.OnInspectorGUI();

            serializedObject.Update();

            var flipProp = serializedObject.FindProperty("flip");

            EditorGUI.BeginChangeCheck();

            EditorGUILayout.PropertyField(flipProp);

            if (EditorGUI.EndChangeCheck()) EditorUpdater.SetDirty("Flip");

            serializedObject.ApplyModifiedProperties();
        }
    }
}