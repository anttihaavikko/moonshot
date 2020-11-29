using System.Collections.Generic;
using UnityEditor;
using UnityEditorInternal;
using UnityEngine;

namespace Anima2D
{
    [CustomEditor(typeof(PoseManager))]
    public class PoseManagerEditor : Editor
    {
        private List<string> m_DuplicatedPaths;
        private ReorderableList mList;

        private void OnEnable()
        {
            m_DuplicatedPaths = GetDuplicatedPaths((target as PoseManager).transform);

            SetupList();
        }

        private void SetupList()
        {
            var poseListProperty = serializedObject.FindProperty("m_Poses");

            if (poseListProperty != null)
            {
                mList = new ReorderableList(serializedObject, poseListProperty, true, true, true, true);

                mList.drawElementCallback = (rect, index, isActive, isFocused) =>
                {
                    var poseProperty = mList.serializedProperty.GetArrayElementAtIndex(index);

                    rect.y += 1.5f;

                    EditorGUI.PropertyField(
                        new Rect(rect.x, rect.y, rect.width - 120, EditorGUIUtility.singleLineHeight), poseProperty,
                        GUIContent.none);

                    EditorGUI.BeginDisabledGroup(!poseProperty.objectReferenceValue);

                    if (GUI.Button(new Rect(rect.x + rect.width - 115, rect.y, 55, EditorGUIUtility.singleLineHeight),
                        "Save"))
                        if (EditorUtility.DisplayDialog("Overwrite Pose",
                            "Overwrite '" + poseProperty.objectReferenceValue.name + "'?", "Apply", "Cancel"))
                        {
                            PoseUtils.SavePose(poseProperty.objectReferenceValue as Pose,
                                (target as PoseManager).transform);
                            mList.index = index;
                        }

                    if (GUI.Button(new Rect(rect.x + rect.width - 55, rect.y, 55, EditorGUIUtility.singleLineHeight),
                        "Load"))
                    {
                        PoseUtils.LoadPose(poseProperty.objectReferenceValue as Pose,
                            (target as PoseManager).transform);
                        mList.index = index;
                    }

                    EditorGUI.EndDisabledGroup();
                };

                mList.drawHeaderCallback = rect => { EditorGUI.LabelField(rect, "Poses"); };

                mList.onSelectCallback = list => { };
            }
        }

        public override void OnInspectorGUI()
        {
            DrawDefaultInspector();

            serializedObject.Update();

            if (mList != null) mList.DoLayoutList();

            EditorGUILayout.Space();

            EditorGUILayout.BeginHorizontal();

            GUILayout.FlexibleSpace();

            if (GUILayout.Button("Create new pose", GUILayout.Width(150))) EditorApplication.delayCall += CreateNewPose;

            GUILayout.FlexibleSpace();

            EditorGUILayout.EndHorizontal();

            EditorGUILayout.Space();

            if (m_DuplicatedPaths.Count > 0)
            {
                var helpString = "Warning: duplicated bone paths found.\nPlease use unique bone paths:\n\n";

                foreach (var path in m_DuplicatedPaths) helpString += path + "\n";

                EditorGUILayout.HelpBox(helpString, MessageType.Warning, true);
            }

            serializedObject.ApplyModifiedProperties();
        }

        private void CreateNewPose()
        {
            serializedObject.Update();

            var newPose = ScriptableObjectUtility.CreateAssetWithSavePanel<Pose>("Create a pose asset", "pose.asset",
                "asset", "Create a new pose");

            mList.serializedProperty.arraySize += 1;

            var newElement = mList.serializedProperty.GetArrayElementAtIndex(mList.serializedProperty.arraySize - 1);

            newElement.objectReferenceValue = newPose;

            serializedObject.ApplyModifiedProperties();

            PoseUtils.SavePose(newPose, (target as PoseManager).transform);
        }

        private List<string> GetDuplicatedPaths(Transform root)
        {
            var paths = new List<string>(50);
            var duplicates = new List<string>(50);
            var bones = new List<Bone2D>(50);

            root.GetComponentsInChildren(true, bones);

            for (var i = 0; i < bones.Count; i++)
            {
                var bone = bones[i];

                if (bone)
                {
                    var bonePath = BoneUtils.GetBonePath(root, bone);

                    if (paths.Contains(bonePath))
                        duplicates.Add(bonePath);
                    else
                        paths.Add(bonePath);
                }
            }

            return duplicates;
        }
    }
}