using System;
using UnityEditor;
using UnityEngine;

namespace Anima2D
{
    public class TextureEditorWindow : EditorWindow
    {
        protected const float k_BorderMargin = 10f;
        protected const float k_ScrollbarMargin = 16f;
        protected const float k_InspectorWindowMargin = 8f;
        protected const float k_InspectorWidth = 330f;
        protected const float k_InspectorHeight = 148f;
        protected const float k_MinZoomPercentage = 0.9f;
        protected const float k_MaxZoom = 10f;
        protected const float k_WheelZoomSpeed = 0.03f;
        protected const float k_MouseZoomSpeed = 0.005f;

        public static string s_NoSelectionWarning = "No sprite selected";
        protected static Styles s_Styles;

        private static Material s_HandleWireMaterial;
        private static Material s_HandleWireMaterial2D;
        public Color textureColor = Color.white;
        protected float m_MipLevel;
        protected Vector2 m_ScrollPosition;
        protected bool m_ShowAlpha;
        protected Texture2D m_Texture;
        protected Rect m_TextureRect;
        protected Rect m_TextureViewRect;
        protected float m_Zoom = -1f;

        private static Material handleWireMaterial
        {
            get
            {
                if (!s_HandleWireMaterial)
                {
                    s_HandleWireMaterial = (Material) EditorGUIUtility.LoadRequired("SceneView/HandleLines.mat");
                    s_HandleWireMaterial2D = (Material) EditorGUIUtility.LoadRequired("SceneView/2DHandleLines.mat");
                }

                return !Camera.current ? s_HandleWireMaterial2D : s_HandleWireMaterial;
            }
        }

        private static Texture2D transparentCheckerTexture
        {
            get
            {
                if (EditorGUIUtility.isProSkin)
                    return EditorGUIUtility.LoadRequired("Previews/Textures/textureCheckerDark.png") as Texture2D;
                return EditorGUIUtility.LoadRequired("Previews/Textures/textureChecker.png") as Texture2D;
            }
        }

        protected Rect maxScrollRect
        {
            get
            {
                var num = m_Texture.width * 0.5f * m_Zoom;
                var num2 = m_Texture.height * 0.5f * m_Zoom;
                return new Rect(-num, -num2, m_TextureViewRect.width + num * 2f, m_TextureViewRect.height + num2 * 2f);
            }
        }

        protected Rect maxRect
        {
            get
            {
                var num = m_TextureViewRect.width * 0.5f / GetMinZoom();
                var num2 = m_TextureViewRect.height * 0.5f / GetMinZoom();
                var left = -num;
                var top = -num2;
                var width = m_Texture.width + num * 2f;
                var height = m_Texture.height + num2 * 2f;
                return new Rect(left, top, width, height);
            }
        }

        protected virtual void OnGUI()
        {
            if (m_Texture)
            {
                InitStyles();

                EditorGUILayout.BeginHorizontal("Toolbar");
                DoToolbarGUI();
                EditorGUILayout.EndHorizontal();

                m_TextureViewRect = new Rect(0f, 16f, position.width - 16f, position.height - 16f - 16f);

                EditorGUILayout.BeginHorizontal();
                GUILayout.FlexibleSpace();
                DoTextureGUI();
                EditorGUILayout.EndHorizontal();
            }
        }

        protected void InitStyles()
        {
            if (s_Styles == null) s_Styles = new Styles();
        }

        protected float GetMinZoom()
        {
            if (m_Texture == null) return 1f;
            return Mathf.Min(m_TextureViewRect.width / m_Texture.width, m_TextureViewRect.height / m_Texture.height) *
                   0.9f;
        }

        protected virtual void HandleZoom()
        {
            var flag = Event.current.alt && Event.current.button == 1;
            if (flag) EditorGUIUtility.AddCursorRect(m_TextureViewRect, MouseCursor.Zoom);
            if ((Event.current.type == EventType.MouseUp || Event.current.type == EventType.MouseDown) && flag ||
                (Event.current.type == EventType.KeyUp || Event.current.type == EventType.KeyDown) &&
                Event.current.keyCode == KeyCode.LeftAlt) Repaint();
            if (Event.current.type == EventType.ScrollWheel || Event.current.type == EventType.MouseDrag &&
                Event.current.alt && Event.current.button == 1)
            {
                var zoomMultiplier = 1f - Event.current.delta.y *
                    (Event.current.type != EventType.ScrollWheel ? -0.005f : 0.03f);
                var wantedZoom = m_Zoom * zoomMultiplier;
                var currentZoom = Mathf.Clamp(wantedZoom, GetMinZoom(), 10f);
                if (currentZoom != m_Zoom)
                {
                    m_Zoom = currentZoom;
                    if (wantedZoom != currentZoom)
                        zoomMultiplier /= wantedZoom / currentZoom;

                    Vector3 textureHalfSize = new Vector2(m_Texture.width, m_Texture.height) * 0.5f;
                    var mousePositionWorld = Handles.inverseMatrix.MultiplyPoint3x4(Event.current.mousePosition);
                    var delta = (mousePositionWorld - textureHalfSize) * (zoomMultiplier - 1f);

                    m_ScrollPosition += (Vector2) Handles.matrix.MultiplyVector(delta);

                    Event.current.Use();
                }
            }
        }

        protected void HandlePanning()
        {
            var flag = !Event.current.alt && Event.current.button > 0 || Event.current.alt && Event.current.button <= 0;
            if (flag && GUIUtility.hotControl == 0)
            {
                EditorGUIUtility.AddCursorRect(m_TextureViewRect, MouseCursor.Pan);
                if (Event.current.type == EventType.MouseDrag)
                {
                    m_ScrollPosition -= Event.current.delta;
                    Event.current.Use();
                }
            }

            if ((Event.current.type == EventType.MouseUp || Event.current.type == EventType.MouseDown) && flag ||
                (Event.current.type == EventType.KeyUp || Event.current.type == EventType.KeyDown) &&
                Event.current.keyCode == KeyCode.LeftAlt) Repaint();
        }

        public void DrawLine(Vector3 p1, Vector3 p2)
        {
            GL.Vertex(p1);
            GL.Vertex(p2);
        }

        public void BeginLines(Color color)
        {
            handleWireMaterial.SetPass(0);
            GL.PushMatrix();
            GL.MultMatrix(Handles.matrix);
            GL.Begin(1);
            GL.Color(color);
        }

        public void EndLines()
        {
            GL.End();
            GL.PopMatrix();
        }

        protected void DrawTexturespaceBackground()
        {
            var num = Mathf.Max(maxRect.width, maxRect.height);
            var b = new Vector2(maxRect.xMin, maxRect.yMin);
            var num2 = num * 0.5f;
            var a = !EditorGUIUtility.isProSkin ? 0.08f : 0.15f;
            var num3 = 8f;
            BeginLines(new Color(0f, 0f, 0f, a));
            for (var num4 = 0f; num4 <= num; num4 += num3)
            {
                var x = -num2 + num4 + b.x;
                var y = num2 + num4 + b.y;
                var p1 = new Vector2(x, y);

                x = num2 + num4 + b.x;
                y = -num2 + num4 + b.y;
                ;
                var p2 = new Vector2(x, y);
                DrawLine(p1, p2);
            }

            EndLines();
        }

        private float Log2(float x)
        {
            return (float) (Math.Log(x) / Math.Log(2.0));
        }

        protected void DrawTexture()
        {
            var num = Mathf.Max(m_Texture.width, 1);
            var num2 = Mathf.Min(m_MipLevel, m_Texture.mipmapCount - 1);
            //float mipMapBias = this.m_Texture.mipMapBias;
            m_Texture.mipMapBias = num2 - Log2(num / m_TextureRect.width);
            //FilterMode filterMode = this.m_Texture.filterMode;
            //m_Texture.filterMode = FilterMode.Point;
            var r = m_TextureRect;
            r.position -= m_ScrollPosition;

            if (m_ShowAlpha)
            {
                EditorGUI.DrawTextureAlpha(r, m_Texture);
            }
            else
            {
                GUI.DrawTextureWithTexCoords(r, transparentCheckerTexture,
                    new Rect(r.width * -0.5f / transparentCheckerTexture.width,
                        r.height * -0.5f / transparentCheckerTexture.height,
                        r.width / transparentCheckerTexture.width,
                        r.height / transparentCheckerTexture.height), false);

                GUI.color = textureColor;
                GUI.DrawTexture(r, m_Texture);
            }

            //m_Texture.filterMode = filterMode;
            //m_Texture.mipMapBias = mipMapBias;
        }

        protected void DrawScreenspaceBackground()
        {
            if (Event.current.type == EventType.Repaint)
                s_Styles.preBackground.Draw(m_TextureViewRect, false, false, false, false);
        }

        protected void HandleScrollbars()
        {
            var position = new Rect(m_TextureViewRect.xMin, m_TextureViewRect.yMax, m_TextureViewRect.width, 16f);
            m_ScrollPosition.x = GUI.HorizontalScrollbar(position, m_ScrollPosition.x, m_TextureViewRect.width,
                maxScrollRect.xMin, maxScrollRect.xMax);
            var position2 = new Rect(m_TextureViewRect.xMax, m_TextureViewRect.yMin, 16f, m_TextureViewRect.height);
            m_ScrollPosition.y = GUI.VerticalScrollbar(position2, m_ScrollPosition.y, m_TextureViewRect.height,
                maxScrollRect.yMin, maxScrollRect.yMax);
        }

        protected void SetupHandlesMatrix()
        {
            var pos = new Vector3(m_TextureRect.x - m_ScrollPosition.x, m_TextureRect.yMax - m_ScrollPosition.y, 0f);
            var s = new Vector3(m_Zoom, -m_Zoom, 1f);
            Handles.matrix = Matrix4x4.TRS(pos, Quaternion.identity, s);
        }

        protected void DoAlphaZoomToolbarGUI()
        {
            m_ShowAlpha = GUILayout.Toggle(m_ShowAlpha, !m_ShowAlpha ? s_Styles.RGBIcon : s_Styles.alphaIcon,
                "toolbarButton");
            m_Zoom = GUILayout.HorizontalSlider(m_Zoom, GetMinZoom(), 10f, s_Styles.preSlider, s_Styles.preSliderThumb,
                GUILayout.MaxWidth(64f));
            var num = 1;

            if (m_Texture != null) num = Mathf.Max(num, m_Texture.mipmapCount);

            EditorGUI.BeginDisabledGroup(num == 1);
            GUILayout.Box(s_Styles.smallMip, s_Styles.preLabel);
            m_MipLevel = Mathf.Round(GUILayout.HorizontalSlider(m_MipLevel, num - 1, 0f, s_Styles.preSlider,
                s_Styles.preSliderThumb, GUILayout.MaxWidth(64f)));
            GUILayout.Box(s_Styles.largeMip, s_Styles.preLabel);
            EditorGUI.EndDisabledGroup();
        }

        protected void DoTextureGUI()
        {
            if (m_Zoom < 0f)
                m_Zoom = GetMinZoom();

            m_TextureRect = new Rect(m_TextureViewRect.width / 2f - m_Texture.width * m_Zoom / 2f,
                m_TextureViewRect.height / 2f - m_Texture.height * m_Zoom / 2f,
                m_Texture.width * m_Zoom,
                m_Texture.height * m_Zoom);

            HandleScrollbars();
            SetupHandlesMatrix();

            DrawScreenspaceBackground();

            GUI.BeginGroup(m_TextureViewRect);

            HandleEvents();

            if (Event.current.type == EventType.Repaint)
            {
                DrawTexturespaceBackground();
                DrawTexture();
                DrawGizmos();
            }

            DoTextureGUIExtras();

            GUI.EndGroup();
        }

        protected virtual void HandleEvents()
        {
        }

        protected virtual void DoTextureGUIExtras()
        {
        }

        protected virtual void DrawGizmos()
        {
        }

        protected void SetNewTexture(Texture2D texture)
        {
            if (texture != m_Texture)
            {
                m_Texture = texture;
                m_Zoom = -1f;
            }
        }

        protected virtual void DoToolbarGUI()
        {
        }

        protected class Styles
        {
            public readonly GUIContent alphaIcon;
            public readonly GUIStyle createRect = "U2D.createRect";
            public readonly GUIStyle dragBorderdot = new GUIStyle();
            public readonly GUIStyle dragBorderDotActive = new GUIStyle();
            public readonly GUIStyle dragdot = "U2D.dragDot";
            public readonly GUIStyle dragdotactive = "U2D.dragDotActive";
            public readonly GUIStyle dragdotDimmed = "U2D.dragDotDimmed";
            public readonly GUIContent largeMip;
            public readonly GUIStyle notice;
            public readonly GUIStyle pivotdot = "U2D.pivotDot";
            public readonly GUIStyle pivotdotactive = "U2D.pivotDotActive";
            public readonly GUIStyle preBackground = "preBackground";
            public readonly GUIStyle preButton = "preButton";
            public readonly GUIStyle preLabel = "preLabel";
            public readonly GUIStyle preSlider = "preSlider";
            public readonly GUIStyle preSliderThumb = "preSliderThumb";
            public readonly GUIStyle preToolbar = "preToolbar";
            public readonly GUIContent RGBIcon;
            public readonly GUIContent showBonesIcon;
            public readonly GUIContent smallMip;
            public readonly GUIContent spriteIcon;
            public readonly GUIStyle toolbar;

            private Texture2D mShowBonesImage;

            public Styles()
            {
                toolbar = new GUIStyle(EditorStyles.inspectorDefaultMargins);
                toolbar.margin.top = 0;
                toolbar.margin.bottom = 0;
                alphaIcon = EditorGUIUtility.IconContent("PreTextureAlpha");
                RGBIcon = EditorGUIUtility.IconContent("PreTextureRGB");
                preToolbar.border.top = 0;
                createRect.border = new RectOffset(3, 3, 3, 3);
                notice = new GUIStyle(GUI.skin.label);
                notice.alignment = TextAnchor.MiddleCenter;
                notice.normal.textColor = Color.yellow;
                dragBorderdot.fixedHeight = 5f;
                dragBorderdot.fixedWidth = 5f;
                dragBorderdot.normal.background = EditorGUIUtility.whiteTexture;
                dragBorderDotActive.fixedHeight = dragBorderdot.fixedHeight;
                dragBorderDotActive.fixedWidth = dragBorderdot.fixedWidth;
                dragBorderDotActive.normal.background = EditorGUIUtility.whiteTexture;
                smallMip = EditorGUIUtility.IconContent("PreTextureMipMapLow");
                largeMip = EditorGUIUtility.IconContent("PreTextureMipMapHigh");
                spriteIcon = EditorGUIUtility.IconContent("Sprite Icon");
                spriteIcon.tooltip = "Reset Sprite";
                showBonesIcon = new GUIContent(showBonesImage);
                showBonesIcon.tooltip = "Show Bones";
            }

            private Texture2D showBonesImage
            {
                get
                {
                    if (!mShowBonesImage)
                    {
                        mShowBonesImage = Resources.Load<Texture2D>("showBonesIcon");
                        mShowBonesImage.hideFlags = HideFlags.DontSave;
                    }

                    return mShowBonesImage;
                }
            }
        }
    }
}