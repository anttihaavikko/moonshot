using System.Linq;
using UnityEditor;
using UnityEngine;

namespace Anima2D
{
    public class BlendShapeFrameEditor : WindowEditorTool
    {
        private Timeline m_TimeLine;
        public SpriteMeshCache spriteMeshCache;

        public BlendShapeFrameEditor()
        {
            windowRect.size = new Vector2(200f, EditorGUIUtility.singleLineHeight * 2f);

            Undo.undoRedoPerformed += UndoRedoPerformed;
            BlendShapeFrameDopeElement.onFrameChanged += OnFrameChanged;
        }

        protected override string GetHeader()
        {
            return "Frames";
        }

        public override void OnWindowGUI(Rect viewRect)
        {
            var xPos = Mathf.Max(200f + 5f + 5f, viewRect.width - 400f);

            windowRect.position = new Vector2(xPos, viewRect.height - windowRect.height - 5f);
            windowRect.size = new Vector2(viewRect.width - xPos - 5f, windowRect.size.y);

            base.OnWindowGUI(viewRect);
        }

        private void UndoRedoPerformed()
        {
            if (spriteMeshCache && m_TimeLine != null) m_TimeLine.Time = spriteMeshCache.blendShapeWeight;
        }

        protected override void DoWindow(int windowId)
        {
            if (m_TimeLine == null) m_TimeLine = new Timeline();

            EditorGUILayout.BeginVertical();

            var rect = GUILayoutUtility.GetRect(10f, 32f);

            EditorGUILayout.EndVertical();

            EditorGUI.BeginChangeCheck();

            if (windowRect.width > 32f)
            {
                var l_DopeElements = spriteMeshCache.selectedBlendshape.frames.ToList()
                    .ConvertAll(f => (IDopeElement) BlendShapeFrameDopeElement.Create(f));

                m_TimeLine.dopeElements = l_DopeElements;
                m_TimeLine.FrameRate = 1f;
                m_TimeLine.Time = spriteMeshCache.blendShapeWeight;
                m_TimeLine.DoTimeline(rect);
            }

            if (EditorGUI.EndChangeCheck()) spriteMeshCache.blendShapeWeight = Mathf.Clamp(m_TimeLine.Time, 0f, 100f);
        }

        private void OnFrameChanged(BlendShapeFrame blendShapeFrame, float weight)
        {
            spriteMeshCache.SetBlendShapeFrameWeight(blendShapeFrame, weight, "Set weight");
        }
    }
}