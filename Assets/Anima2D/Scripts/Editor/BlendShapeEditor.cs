using UnityEditor;
using UnityEngine;

namespace Anima2D
{
    public class BlendShapeEditor : WindowEditorTool
    {
        public SpriteMeshCache spriteMeshCache;

        public BlendShapeEditor()
        {
            windowRect = new Rect(5f, 5f, 250, 45);
        }

        private float windowHeight => 80f;

        protected override string GetHeader()
        {
            return "Blendshapes";
        }


        public override void OnWindowGUI(Rect viewRect)
        {
            windowRect.position = new Vector2(0f, -15f);

            base.OnWindowGUI(viewRect);
        }

        protected override void DoWindow(int windowId)
        {
            if (!spriteMeshCache)
                //Debug.Log("No SpriteMeshCache");
                return;

            if (!spriteMeshCache)
                //Debug.Log("No SpriteMeshCache");
                return;

            if (spriteMeshCache.blendshapes == null)
                //Debug.Log("spriteMeshCache.blendshapes == null");
                return;

            EditorGUIUtility.labelWidth = 50f;

            EditorGUILayout.BeginVertical();

            EditorGUILayout.BeginHorizontal();

            EditorGUI.BeginChangeCheck();

            var blendshapeIndex = spriteMeshCache.blendshapes.IndexOf(spriteMeshCache.selectedBlendshape);

            blendshapeIndex = EditorGUILayout.Popup(blendshapeIndex, GetBlendshapeNames(), GUILayout.Width(100f));

            if (EditorGUI.EndChangeCheck())
            {
                spriteMeshCache.RegisterUndo("select blendshape");
                spriteMeshCache.selectedBlendshape = spriteMeshCache.blendshapes[blendshapeIndex];
            }

            EditorGUILayout.Space();

            if (GUILayout.Button(new GUIContent("New", "Create a blend shape"), EditorStyles.miniButtonLeft,
                GUILayout.Width(50f))) CreateBlendshape();

            EditorGUI.BeginDisabledGroup(spriteMeshCache.selectedBlendshape == null);

            if (GUILayout.Button(new GUIContent("Delete", "Delete blend shape"), EditorStyles.miniButtonRight,
                GUILayout.Width(50f))) DeleteBlendshape();

            EditorGUI.EndDisabledGroup();

            EditorGUILayout.Space();

            EditorGUIUtility.fieldWidth = 35f;
            EditorGUIUtility.labelWidth = 1f;

            EditorGUI.BeginDisabledGroup(spriteMeshCache.selectedBlendshapeFrame == null);

            EditorGUILayout.LabelField("Frame:");

            if (GUILayout.Button(new GUIContent("Delete", "Delete frame"), EditorStyles.miniButtonLeft,
                GUILayout.Width(50f))) DeleteFrame();

            if (GUILayout.Button(new GUIContent("Reset", "Reset vertices"), EditorStyles.miniButtonRight,
                GUILayout.Width(50f))) ResetVertices();

            EditorGUI.EndDisabledGroup();

            GUILayout.Space(1f);

            EditorGUILayout.EndHorizontal();

            EditorGUILayout.EndVertical();
        }

        private void CreateBlendshape()
        {
            if (spriteMeshCache)
            {
                var blendShape = spriteMeshCache.CreateBlendshape("New BlendShape", "Create BlendShape");

                spriteMeshCache.selectedBlendshape = blendShape;

                spriteMeshCache.CreateBlendShapeFrame(blendShape, 100f, "Create BlendShape");

                spriteMeshCache.blendShapeWeight = 100f;
            }
        }

        private void DeleteBlendshape()
        {
            if (spriteMeshCache)
                spriteMeshCache.DeleteBlendShape(spriteMeshCache.selectedBlendshape, "Delete BlendShape");
        }

        private void ResetVertices()
        {
            if (spriteMeshCache)
            {
                if (spriteMeshCache.selection.Count > 0)
                    spriteMeshCache.ResetVertices(spriteMeshCache.selectedNodes, "Reset vertices");
                else
                    spriteMeshCache.ResetVertices(spriteMeshCache.nodes, "Reset vertices");
            }
        }

        private void DeleteFrame()
        {
            if (spriteMeshCache && spriteMeshCache.selectedBlendshapeFrame)
                spriteMeshCache.DeleteBlendShapeFrame(spriteMeshCache.selectedBlendshape,
                    spriteMeshCache.selectedBlendshapeFrame,
                    "Delete frame");
        }

        private string[] GetBlendshapeNames()
        {
            if (spriteMeshCache && spriteMeshCache.blendshapes != null)
            {
                var i = 0;

                return spriteMeshCache.blendshapes.ConvertAll(b => i++ + "  " + b.name).ToArray();
            }

            return new string[0];
        }
    }
}