using System.Collections.Generic;
using UnityEditor;
using UnityEditorInternal;
using UnityEngine;

namespace Anima2D
{
    [DisallowMultipleComponent]
    [CanEditMultipleObjects]
    [CustomEditor(typeof(SpriteMeshInstance))]
    public class SpriteMeshInstanceEditor : Editor
    {
        private SerializedProperty m_BoneTransformsProperty;
        private SerializedProperty m_ColorProperty;
        private SerializedProperty m_MaterialsProperty;
        private SerializedProperty m_SortingLayerID;

        private SerializedProperty m_SortingOrder;
        private SpriteMeshData m_SpriteMeshData;
        private SpriteMeshInstance m_SpriteMeshInstance;
        private SerializedProperty m_SpriteMeshProperty;

        private int m_UndoGroup = -1;

        private ReorderableList mBoneList;

        private void OnEnable()
        {
            m_SpriteMeshInstance = target as SpriteMeshInstance;
            m_SortingOrder = serializedObject.FindProperty("m_SortingOrder");
            m_SortingLayerID = serializedObject.FindProperty("m_SortingLayerID");
            m_SpriteMeshProperty = serializedObject.FindProperty("m_SpriteMesh");
            m_ColorProperty = serializedObject.FindProperty("m_Color");
            m_MaterialsProperty = serializedObject.FindProperty("m_Materials.Array");
            m_BoneTransformsProperty = serializedObject.FindProperty("m_BoneTransforms.Array");

            UpgradeToMaterials();

            UpdateSpriteMeshData();
            SetupBoneList();

#if UNITY_5_5_OR_NEWER

#else
			EditorUtility.SetSelectedWireframeHidden(m_SpriteMeshInstance.cachedRenderer, !m_SpriteMeshInstance.cachedSkinnedRenderer);
#endif
        }

        public void OnDisable()
        {
            if (target)
            {
#if UNITY_5_5_OR_NEWER

#else
				EditorUtility.SetSelectedWireframeHidden(m_SpriteMeshInstance.cachedRenderer, false);
#endif
            }
        }

        private void UpgradeToMaterials()
        {
            if (Selection.transforms.Length == 1 && m_MaterialsProperty.arraySize == 0)
            {
                serializedObject.Update();
                m_MaterialsProperty.InsertArrayElementAtIndex(0);
                m_MaterialsProperty.GetArrayElementAtIndex(0).objectReferenceValue = SpriteMeshUtils.defaultMaterial;
                serializedObject.ApplyModifiedProperties();
            }
        }

        private bool HasBindPoses()
        {
            var hasBindPoses = false;

            if (m_SpriteMeshData && m_SpriteMeshData.bindPoses != null && m_SpriteMeshData.bindPoses.Length > 0)
                hasBindPoses = true;

            return hasBindPoses;
        }

        private void SetupBoneList()
        {
            if (HasBindPoses() && m_BoneTransformsProperty.arraySize != m_SpriteMeshData.bindPoses.Length)
            {
                var oldSize = m_BoneTransformsProperty.arraySize;

                serializedObject.Update();

                m_BoneTransformsProperty.arraySize = m_SpriteMeshData.bindPoses.Length;

                for (var i = oldSize; i < m_BoneTransformsProperty.arraySize; ++i)
                {
                    var element = m_BoneTransformsProperty.GetArrayElementAtIndex(i);
                    element.objectReferenceValue = null;
                }

                serializedObject.ApplyModifiedProperties();
            }

            mBoneList = new ReorderableList(serializedObject, m_BoneTransformsProperty, !HasBindPoses(), true,
                !HasBindPoses(), !HasBindPoses());

            mBoneList.drawElementCallback = (rect, index, isActive, isFocused) =>
            {
                var boneProperty = mBoneList.serializedProperty.GetArrayElementAtIndex(index);

                rect.y += 1.5f;

                var labelWidth = 0f;

                if (HasBindPoses() && index < m_SpriteMeshData.bindPoses.Length)
                {
                    labelWidth = EditorGUIUtility.labelWidth;
                    EditorGUI.LabelField(new Rect(rect.x, rect.y, labelWidth, EditorGUIUtility.singleLineHeight),
                        new GUIContent(m_SpriteMeshData.bindPoses[index].name));
                }

                EditorGUI.BeginChangeCheck();

                EditorGUI.PropertyField(
                    new Rect(rect.x + labelWidth, rect.y, rect.width - labelWidth, EditorGUIUtility.singleLineHeight),
                    boneProperty, GUIContent.none);

                if (EditorGUI.EndChangeCheck())
                {
                    var l_NewTransform = boneProperty.objectReferenceValue as Transform;
                    if (l_NewTransform && !l_NewTransform.GetComponent<Bone2D>())
                        boneProperty.objectReferenceValue = null;
                }
            };

            mBoneList.drawHeaderCallback = rect => { EditorGUI.LabelField(rect, "Bones"); };

            mBoneList.onSelectCallback = list => { };
        }


        public override void OnInspectorGUI()
        {
#if UNITY_5_5_OR_NEWER

#else
			EditorUtility.SetSelectedWireframeHidden(m_SpriteMeshInstance.cachedRenderer, !m_SpriteMeshInstance.cachedSkinnedRenderer);
#endif

            EditorGUI.BeginChangeCheck();

            serializedObject.Update();

            EditorGUILayout.PropertyField(m_SpriteMeshProperty);

            serializedObject.ApplyModifiedProperties();

            if (EditorGUI.EndChangeCheck())
            {
                UpdateSpriteMeshData();
                UpdateRenderers();
                SetupBoneList();
            }

            serializedObject.Update();

            EditorGUILayout.PropertyField(m_ColorProperty);

            if (m_MaterialsProperty.arraySize == 0) m_MaterialsProperty.InsertArrayElementAtIndex(0);
            EditorGUILayout.PropertyField(m_MaterialsProperty.GetArrayElementAtIndex(0), new GUIContent("Material"),
                true);

            EditorGUILayout.Space();

            EditorGUIExtra.SortingLayerField(new GUIContent("Sorting Layer"), m_SortingLayerID, EditorStyles.popup,
                EditorStyles.label);
            EditorGUILayout.PropertyField(m_SortingOrder, new GUIContent("Order in Layer"));

            EditorGUILayout.Space();

            if (!HasBindPoses())
            {
                var bones = new List<Bone2D>();

                EditorGUI.BeginChangeCheck();

                var root = EditorGUILayout.ObjectField("Set bones", null, typeof(Transform), true) as Transform;

                if (EditorGUI.EndChangeCheck())
                {
                    if (root) root.GetComponentsInChildren(bones);

                    Undo.RegisterCompleteObjectUndo(m_SpriteMeshInstance, "set bones");

                    m_BoneTransformsProperty.arraySize = bones.Count;

                    for (var i = 0; i < bones.Count; ++i)
                        m_BoneTransformsProperty.GetArrayElementAtIndex(i).objectReferenceValue = bones[i].transform;

                    UpdateRenderers();
                }
            }

            EditorGUI.BeginChangeCheck();

            if (mBoneList != null) mBoneList.DoLayoutList();

            serializedObject.ApplyModifiedProperties();

            if (EditorGUI.EndChangeCheck()) UpdateRenderers();

            if (m_SpriteMeshInstance.spriteMesh)
            {
                if (SpriteMeshUtils.HasNullBones(m_SpriteMeshInstance))
                    EditorGUILayout.HelpBox("Warning:\nBone list contains null references.", MessageType.Warning);

                if (m_SpriteMeshInstance.spriteMesh.sharedMesh.bindposes.Length != m_SpriteMeshInstance.bones.Count)
                    EditorGUILayout.HelpBox(
                        "Warning:\nNumber of SpriteMesh Bind Poses and number of Bones does not match.",
                        MessageType.Warning);
            }
        }

        private void UpdateSpriteMeshData()
        {
            m_SpriteMeshData = null;

            if (m_SpriteMeshProperty != null && m_SpriteMeshProperty.objectReferenceValue)
                m_SpriteMeshData =
                    SpriteMeshUtils.LoadSpriteMeshData(m_SpriteMeshProperty.objectReferenceValue as SpriteMesh);
        }

        private void UpdateRenderers()
        {
            m_UndoGroup = Undo.GetCurrentGroup();

            EditorApplication.delayCall += DoUpdateRenderer;
        }

        private void DoUpdateRenderer()
        {
            SpriteMeshUtils.UpdateRenderer(m_SpriteMeshInstance);

#if UNITY_5_5_OR_NEWER

#else
			EditorUtility.SetSelectedWireframeHidden(m_SpriteMeshInstance.cachedRenderer, !m_SpriteMeshInstance.cachedSkinnedRenderer);
#endif

            Undo.CollapseUndoOperations(m_UndoGroup);
            SceneView.RepaintAll();
        }
    }
}