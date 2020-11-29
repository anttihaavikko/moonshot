using UnityEditor;
using UnityEngine;

namespace Anima2D
{
    public class MeshToolEditor : WindowEditorTool
    {
        public enum MeshTool
        {
            None,
            Hole
        }

        public SpriteMeshCache spriteMeshCache;

        public MeshToolEditor()
        {
            windowRect = new Rect(0f, 0f, 200f, 45f);

            sliceToggle = false;
        }

        public MeshTool tool { get; set; }

        public bool sliceToggle { get; set; }

        protected override string GetHeader()
        {
            return "";
        }

        public override void OnWindowGUI(Rect viewRect)
        {
            windowRect.position = new Vector2(0f, -15f);

            base.OnWindowGUI(viewRect);
        }

        protected override void DoWindow(int windowId)
        {
            GUI.color = Color.white;

            EditorGUILayout.BeginHorizontal();

            sliceToggle = GUILayout.Toggle(sliceToggle, new GUIContent("Slice", "Slice the sprite"),
                EditorStyles.miniButton, GUILayout.Width(50f));

            EditorGUILayout.Space();

            var holeToggle = GUILayout.Toggle(tool == MeshTool.Hole, new GUIContent("Hole", "Create holes (H)"),
                EditorStyles.miniButton, GUILayout.Width(50f));

            if (holeToggle)
                tool = MeshTool.Hole;
            else
                tool = MeshTool.None;

            EditorGUILayout.Space();

            EditorGUI.BeginDisabledGroup(!spriteMeshCache.spriteMeshInstance);

            if (GUILayout.Toggle(spriteMeshCache.isBound, new GUIContent("Bind", "Bind bones"),
                EditorStyles.miniButtonLeft, GUILayout.Width(50f)))
                if (!spriteMeshCache.isBound && spriteMeshCache.spriteMeshInstance)
                {
                    spriteMeshCache.RegisterUndo("Bind bones");
                    spriteMeshCache.BindBones();
                    spriteMeshCache.CalculateAutomaticWeights();
                }

            EditorGUI.EndDisabledGroup();

            if (GUILayout.Toggle(!spriteMeshCache.isBound, new GUIContent("Unbind", "Clear binding data"),
                EditorStyles.miniButtonRight, GUILayout.Width(50f)))
                if (spriteMeshCache.isBound)
                {
                    spriteMeshCache.RegisterUndo("Clear weights");
                    spriteMeshCache.ClearWeights();
                }

            GUILayout.Space(1f);

            EditorGUILayout.EndHorizontal();
        }
    }
}