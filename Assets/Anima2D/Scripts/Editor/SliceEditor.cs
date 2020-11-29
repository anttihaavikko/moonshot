using UnityEditor;
using UnityEngine;

namespace Anima2D
{
    public class SliceEditor : WindowEditorTool
    {
        public SpriteMeshCache spriteMeshCache;

        public SliceEditor()
        {
            windowRect = new Rect(0f, 0f, 225f, 35);

            alpha = 0.05f;
            detail = 0.25f;
            holes = true;
        }

        public float alpha { get; set; }
        public float detail { get; set; }
        public float tessellation { get; set; }
        public bool holes { get; set; }

        protected override string GetHeader()
        {
            return "Slice tool";
        }

        public override void OnWindowGUI(Rect viewRect)
        {
            windowRect.position = new Vector2(5f, 30f);

            base.OnWindowGUI(viewRect);
        }

        protected override void DoWindow(int windowId)
        {
            EditorGUIUtility.labelWidth = 85f;
            EditorGUIUtility.fieldWidth = 32f;

            detail = EditorGUILayout.Slider("Outline detail", detail, 0f, 1f);
            alpha = EditorGUILayout.Slider("Alpha cutout", alpha, 0f, 1f);
            tessellation = EditorGUILayout.Slider("Tessellation", tessellation, 0f, 1f);
            holes = EditorGUILayout.Toggle("Detect holes", holes);

            EditorGUILayout.BeginHorizontal();

            GUILayout.FlexibleSpace();

            if (GUILayout.Button("Apply", GUILayout.Width(70f)))
                if (spriteMeshCache)
                    spriteMeshCache.InitFromOutline(detail, alpha, holes, tessellation, "set outline");

            GUILayout.FlexibleSpace();

            EditorGUILayout.EndHorizontal();
        }

        protected override void DoGUI()
        {
            if (canShow())
            {
                var pos = Vector3.zero;
                var rot = Quaternion.identity;
                var rect = spriteMeshCache.rect;

                EditorGUI.BeginChangeCheck();

                RectHandles.Do(ref rect, ref pos, ref rot, false);

                if (EditorGUI.EndChangeCheck())
                {
                    spriteMeshCache.RegisterUndo("set rect");

                    spriteMeshCache.rect = rect;
                }
            }
        }
    }
}