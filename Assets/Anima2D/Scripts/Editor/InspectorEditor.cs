using System.Linq;
using UnityEditor;
using UnityEngine;

namespace Anima2D
{
    public class InspectorEditor : WindowEditorTool
    {
        public SpriteMeshCache spriteMeshCache;

        public InspectorEditor()
        {
            windowRect = new Rect(0f, 0f, 250f, 75);
        }

        protected override string GetHeader()
        {
            return "Inspector";
        }

        private Vector2 GetWindowSize()
        {
            var size = Vector2.one;

            if (spriteMeshCache.mode == SpriteMeshEditorWindow.Mode.Mesh)
            {
                if (spriteMeshCache.isBound && spriteMeshCache.selection.Count > 0)
                    size = new Vector2(250f, 95f);
                else if (spriteMeshCache.selectedBindPose)
                    size = new Vector2(175f, 75f);
                else
                    size = new Vector2(175f, 75f);
            }
            else if (spriteMeshCache.mode == SpriteMeshEditorWindow.Mode.Blendshapes)
            {
                if (spriteMeshCache.selectedBlendshape) size = new Vector2(200f, 45f);
            }

            return size;
        }

        public override void OnWindowGUI(Rect viewRect)
        {
            windowRect.size = GetWindowSize();
            windowRect.position = new Vector2(5f, viewRect.height - windowRect.height - 5f);

            base.OnWindowGUI(viewRect);
        }

        protected override void DoWindow(int windowId)
        {
            EditorGUILayout.BeginVertical();

            if (spriteMeshCache.mode == SpriteMeshEditorWindow.Mode.Mesh)
            {
                if (spriteMeshCache.isBound && spriteMeshCache.selection.Count > 0)
                    DoVerticesInspector();
                else if (spriteMeshCache.selectedBindPose)
                    DoBindPoseInspector();
                else
                    DoSpriteMeshInspector();
            }
            else if (spriteMeshCache.mode == SpriteMeshEditorWindow.Mode.Blendshapes)
            {
                if (spriteMeshCache.selectedBlendshape) DoBlendshapeInspector();
            }

            EditorGUILayout.EndVertical();

            EditorGUIUtility.labelWidth = -1;
        }

        private void DoSpriteMeshInspector()
        {
            if (spriteMeshCache.spriteMesh)
            {
                EditorGUI.BeginDisabledGroup(true);

                EditorGUIUtility.labelWidth = 55f;

                EditorGUILayout.ObjectField("Sprite", spriteMeshCache.spriteMesh.sprite, typeof(Object), false);

                EditorGUI.EndDisabledGroup();

                EditorGUIUtility.labelWidth = 15f;

                EditorGUI.BeginChangeCheck();

                var pivotPoint = EditorGUILayout.Vector2Field("Pivot", spriteMeshCache.pivotPoint);

                if (EditorGUI.EndChangeCheck())
                {
                    spriteMeshCache.RegisterUndo("set pivot");

                    spriteMeshCache.SetPivotPoint(pivotPoint);
                }
            }
        }

        private void DoBindPoseInspector()
        {
            EditorGUIUtility.labelWidth = 55f;
            EditorGUIUtility.fieldWidth = 55f;

            EditorGUI.BeginChangeCheck();

            var name = EditorGUILayout.TextField("Name", spriteMeshCache.selectedBindPose.name);

            if (EditorGUI.EndChangeCheck())
            {
                if (string.IsNullOrEmpty(name)) name = "New bone";

                spriteMeshCache.selectedBindPose.name = name;

                spriteMeshCache.RegisterUndo("set name");

                if (!string.IsNullOrEmpty(spriteMeshCache.selectedBindPose.path))
                {
                    var index = spriteMeshCache.selectedBindPose.path.LastIndexOf("/");

                    if (index < 0)
                        index = 0;
                    else
                        index++;

                    foreach (var bindInfo in spriteMeshCache.bindPoses)
                        if (!string.IsNullOrEmpty(bindInfo.path) && index < bindInfo.path.Length)
                        {
                            var pathPrefix = bindInfo.path;
                            var pathSuffix = "";

                            if (bindInfo.path.Contains('/'))
                            {
                                pathPrefix = bindInfo.path.Substring(0, index);

                                var tail = bindInfo.path.Substring(index);

                                var index2 = tail.IndexOf("/");

                                if (index2 > 0) pathSuffix = tail.Substring(index2);
                                bindInfo.path = pathPrefix + name + pathSuffix;
                            }
                            else
                            {
                                bindInfo.path = bindInfo.name;
                            }
                        }
                }


                spriteMeshCache.isDirty = true;
            }

            EditorGUI.BeginChangeCheck();

            var zOrder = EditorGUILayout.IntField("Z-Order", spriteMeshCache.selectedBindPose.zOrder);

            if (EditorGUI.EndChangeCheck())
            {
                spriteMeshCache.RegisterUndo("set z-order");
                spriteMeshCache.selectedBindPose.zOrder = zOrder;
                spriteMeshCache.isDirty = true;
            }

            EditorGUI.BeginChangeCheck();

            var color = EditorGUILayout.ColorField("Color", spriteMeshCache.selectedBindPose.color);

            if (EditorGUI.EndChangeCheck())
            {
                spriteMeshCache.RegisterUndo("set color");
                spriteMeshCache.selectedBindPose.color = color;
                spriteMeshCache.isDirty = true;
            }
        }

        private bool IsMixedBoneIndex(int weightIndex, out int boneIndex)
        {
            boneIndex = -1;
            var weight = 0f;

            spriteMeshCache.GetBoneWeight(spriteMeshCache.nodes[spriteMeshCache.selection.First()])
                .GetWeight(weightIndex, out boneIndex, out weight);

            var selectedNodes = spriteMeshCache.selectedNodes;

            foreach (var node in selectedNodes)
            {
                var l_boneIndex = -1;
                spriteMeshCache.GetBoneWeight(node).GetWeight(weightIndex, out l_boneIndex, out weight);

                if (l_boneIndex != boneIndex) return true;
            }

            return false;
        }

        private void DoVerticesInspector()
        {
            if (spriteMeshCache.selection.Count > 0)
            {
                var names = spriteMeshCache.GetBoneNames("Unassigned");

                var boneWeight = BoneWeight.Create();

                EditorGUI.BeginChangeCheck();

                var mixedBoneIndex0 = false;
                var mixedBoneIndex1 = false;
                var mixedBoneIndex2 = false;
                var mixedBoneIndex3 = false;
                var changedIndex0 = false;
                var changedIndex1 = false;
                var changedIndex2 = false;
                var changedIndex3 = false;
                var mixedWeight = false;

                if (spriteMeshCache.multiselection)
                {
                    mixedWeight = true;

                    var boneIndex = -1;
                    mixedBoneIndex0 = IsMixedBoneIndex(0, out boneIndex);
                    if (!mixedBoneIndex0) boneWeight.boneIndex0 = boneIndex;
                    mixedBoneIndex1 = IsMixedBoneIndex(1, out boneIndex);
                    if (!mixedBoneIndex1) boneWeight.boneIndex1 = boneIndex;
                    mixedBoneIndex2 = IsMixedBoneIndex(2, out boneIndex);
                    if (!mixedBoneIndex2) boneWeight.boneIndex2 = boneIndex;
                    mixedBoneIndex3 = IsMixedBoneIndex(3, out boneIndex);
                    if (!mixedBoneIndex3) boneWeight.boneIndex3 = boneIndex;
                }
                else
                {
                    boneWeight = spriteMeshCache.GetBoneWeight(spriteMeshCache.selectedNode);
                }

                EditorGUI.BeginChangeCheck();

                EditorGUI.BeginChangeCheck();
                boneWeight = EditorGUIExtra.Weight(boneWeight, 0, names, mixedBoneIndex0, mixedWeight);
                changedIndex0 = EditorGUI.EndChangeCheck();

                EditorGUI.BeginChangeCheck();
                boneWeight = EditorGUIExtra.Weight(boneWeight, 1, names, mixedBoneIndex1, mixedWeight);
                changedIndex1 = EditorGUI.EndChangeCheck();

                EditorGUI.BeginChangeCheck();
                boneWeight = EditorGUIExtra.Weight(boneWeight, 2, names, mixedBoneIndex2, mixedWeight);
                changedIndex2 = EditorGUI.EndChangeCheck();

                EditorGUI.BeginChangeCheck();
                boneWeight = EditorGUIExtra.Weight(boneWeight, 3, names, mixedBoneIndex3, mixedWeight);
                changedIndex3 = EditorGUI.EndChangeCheck();

                if (EditorGUI.EndChangeCheck())
                {
                    spriteMeshCache.RegisterUndo("modify weights");

                    if (spriteMeshCache.multiselection)
                    {
                        var selectedNodes = spriteMeshCache.selectedNodes;

                        foreach (var node in selectedNodes)
                        {
                            var l_boneWeight = spriteMeshCache.GetBoneWeight(node);

                            if (!mixedBoneIndex0 || changedIndex0)
                                l_boneWeight.SetWeight(0, boneWeight.boneIndex0, l_boneWeight.weight0);
                            if (!mixedBoneIndex1 || changedIndex1)
                                l_boneWeight.SetWeight(1, boneWeight.boneIndex1, l_boneWeight.weight1);
                            if (!mixedBoneIndex2 || changedIndex2)
                                l_boneWeight.SetWeight(2, boneWeight.boneIndex2, l_boneWeight.weight2);
                            if (!mixedBoneIndex3 || changedIndex3)
                                l_boneWeight.SetWeight(3, boneWeight.boneIndex3, l_boneWeight.weight3);

                            spriteMeshCache.SetBoneWeight(node, l_boneWeight);
                        }
                    }
                    else
                    {
                        spriteMeshCache.SetBoneWeight(spriteMeshCache.selectedNode, boneWeight);
                    }
                }

                EditorGUI.showMixedValue = false;
            }
        }

        private void DoBlendshapeInspector()
        {
            EditorGUIUtility.labelWidth = 65f;
            EditorGUIUtility.fieldWidth = 55f;

            var name = spriteMeshCache.selectedBlendshape.name;

            EditorGUI.BeginChangeCheck();

            name = EditorGUILayout.TextField("Name", name);

            if (EditorGUI.EndChangeCheck())
            {
                spriteMeshCache.RegisterUndo("change name");
                spriteMeshCache.selectedBlendshape.name = name;
            }
        }
    }
}