using UnityEditor;
using UnityEditorInternal;
using UnityEngine;

namespace Anima2D
{
    [CustomEditor(typeof(SpriteMeshAnimation))]
    public class SpriteMeshAnimationEditor : Editor
    {
        private SerializedProperty m_FrameListProperty;
        private SerializedProperty m_FrameProperty;
        private ReorderableList m_List;

        private void OnEnable()
        {
            m_FrameListProperty = serializedObject.FindProperty("m_Frames");
            m_FrameProperty = serializedObject.FindProperty("m_Frame");

            SetupList();
        }

        private void SetupList()
        {
            if (m_FrameListProperty != null)
            {
                m_List = new ReorderableList(serializedObject, m_FrameListProperty, true, true, true, true);

                m_List.drawElementCallback = (rect, index, isActive, isFocused) =>
                {
                    var poseProperty = m_List.serializedProperty.GetArrayElementAtIndex(index);

                    rect.y += 1.5f;

                    EditorGUI.PropertyField(new Rect(rect.x, rect.y, rect.width, EditorGUIUtility.singleLineHeight),
                        poseProperty, GUIContent.none);
                };

                m_List.drawHeaderCallback = rect => { EditorGUI.LabelField(rect, "Frames"); };

                m_List.onSelectCallback = list => { };
            }
        }

        public override void OnInspectorGUI()
        {
            serializedObject.Update();

            var spriteMeshAnimation = target as SpriteMeshAnimation;

            EditorGUI.BeginDisabledGroup(m_FrameListProperty.arraySize == 0);

            EditorGUI.BeginChangeCheck();

            var frame = EditorGUILayout.IntSlider("Frame", spriteMeshAnimation.frame, 0,
                m_FrameListProperty.arraySize - 1);

            if (EditorGUI.EndChangeCheck())
            {
                Undo.RecordObject(spriteMeshAnimation, "Set frame");

                m_FrameProperty.floatValue = frame;
                spriteMeshAnimation.frame = frame;
            }

            EditorGUI.EndDisabledGroup();

            m_List.DoLayoutList();

            serializedObject.ApplyModifiedProperties();

            EditorUtility.SetDirty(spriteMeshAnimation);
            EditorUtility.SetDirty(spriteMeshAnimation.cachedSpriteMeshInstance);
        }
    }
}