using UnityEditor;
using UnityEditorInternal;
using UnityEngine;

namespace Anima2D
{
    [CustomEditor(typeof(IkGroup))]
    public class IkGroupEditor : Editor
    {
        private ReorderableList mList;

        private void OnEnable()
        {
            SetupList();
        }

        private void SetupList()
        {
            var ikListProperty = serializedObject.FindProperty("m_IkComponents");

            if (ikListProperty != null)
            {
                mList = new ReorderableList(serializedObject, ikListProperty, true, true, true, true);

                mList.drawElementCallback = (rect, index, isActive, isFocused) =>
                {
                    var boneProperty = mList.serializedProperty.GetArrayElementAtIndex(index);

                    rect.y += 1.5f;

                    EditorGUI.PropertyField(new Rect(rect.x, rect.y, rect.width, EditorGUIUtility.singleLineHeight),
                        boneProperty, GUIContent.none);
                };

                mList.drawHeaderCallback = rect => { EditorGUI.LabelField(rect, "IK Components"); };

                mList.onSelectCallback = list => { };
            }
        }

        public override void OnInspectorGUI()
        {
            DrawDefaultInspector();

            serializedObject.Update();

            if (mList != null) mList.DoLayoutList();

            EditorGUILayout.Space();

            serializedObject.ApplyModifiedProperties();
        }
    }
}