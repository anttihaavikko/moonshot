using System;
using UnityEngine;

namespace Anima2D
{
    [Serializable]
    public class ZoomableArea
    {
        private static Vector2 m_MouseDownPosition = new Vector2(-1000000f, -1000000f);
        private static int zoomableAreaHash = "ZoomableArea".GetHashCode();
        public bool m_UniformScale;
        private int horizontalScrollbarID;
        private Rect m_DrawArea = new Rect(0f, 0f, 100f, 100f);
        private bool m_HAllowExceedBaseRangeMax = true;
        private bool m_HAllowExceedBaseRangeMin = true;
        private float m_HBaseRangeMax = 1f;
        private float m_HBaseRangeMin;
        private bool m_HRangeLocked;
        private float m_HScaleMax = 100000f;
        private float m_HScaleMin = 0.001f;
        private bool m_HSlider = true;
        private bool m_IgnoreScrollWheelUntilClicked;
        private Rect m_LastShownAreaInsideMargins = new Rect(0f, 0f, 100f, 100f);
        private float m_MarginBottom;
        private float m_MarginLeft;
        private float m_MarginRight;
        private float m_MarginTop;
        private bool m_MinimalGUI;
        internal Vector2 m_Scale = new Vector2(1f, -1f);
        private bool m_ScaleWithWindow;
        internal Vector2 m_Translation = new Vector2(0f, 0f);
        private bool m_VAllowExceedBaseRangeMax = true;
        private bool m_VAllowExceedBaseRangeMin = true;
        private float m_VBaseRangeMax = 1f;
        private float m_VBaseRangeMin;
        private bool m_VRangeLocked;
        private float m_VScaleMax = 100000f;
        private float m_VScaleMin = 0.001f;
        private bool m_VSlider = true;
        private Styles styles;
        private int verticalScrollbarID;

        public ZoomableArea()
        {
            m_MinimalGUI = false;
            styles = new Styles(false);
        }

        public ZoomableArea(bool minimalGUI)
        {
            m_MinimalGUI = minimalGUI;
            styles = new Styles(minimalGUI);
        }

        public bool hRangeLocked
        {
            get => m_HRangeLocked;
            set => m_HRangeLocked = value;
        }

        public bool vRangeLocked
        {
            get => m_VRangeLocked;
            set => m_VRangeLocked = value;
        }

        public float hBaseRangeMin
        {
            get => m_HBaseRangeMin;
            set => m_HBaseRangeMin = value;
        }

        public float hBaseRangeMax
        {
            get => m_HBaseRangeMax;
            set => m_HBaseRangeMax = value;
        }

        public float vBaseRangeMin
        {
            get => m_VBaseRangeMin;
            set => m_VBaseRangeMin = value;
        }

        public float vBaseRangeMax
        {
            get => m_VBaseRangeMax;
            set => m_VBaseRangeMax = value;
        }

        public bool hAllowExceedBaseRangeMin
        {
            get => m_HAllowExceedBaseRangeMin;
            set => m_HAllowExceedBaseRangeMin = value;
        }

        public bool hAllowExceedBaseRangeMax
        {
            get => m_HAllowExceedBaseRangeMax;
            set => m_HAllowExceedBaseRangeMax = value;
        }

        public bool vAllowExceedBaseRangeMin
        {
            get => m_VAllowExceedBaseRangeMin;
            set => m_VAllowExceedBaseRangeMin = value;
        }

        public bool vAllowExceedBaseRangeMax
        {
            get => m_VAllowExceedBaseRangeMax;
            set => m_VAllowExceedBaseRangeMax = value;
        }

        public float hRangeMin
        {
            get => !hAllowExceedBaseRangeMin ? hBaseRangeMin : float.NegativeInfinity;
            set => SetAllowExceed(ref m_HBaseRangeMin, ref m_HAllowExceedBaseRangeMin, value);
        }

        public float hRangeMax
        {
            get => !hAllowExceedBaseRangeMax ? hBaseRangeMax : float.PositiveInfinity;
            set => SetAllowExceed(ref m_HBaseRangeMax, ref m_HAllowExceedBaseRangeMax, value);
        }

        public float vRangeMin
        {
            get => !vAllowExceedBaseRangeMin ? vBaseRangeMin : float.NegativeInfinity;
            set => SetAllowExceed(ref m_VBaseRangeMin, ref m_VAllowExceedBaseRangeMin, value);
        }

        public float vRangeMax
        {
            get => !vAllowExceedBaseRangeMax ? vBaseRangeMax : float.PositiveInfinity;
            set => SetAllowExceed(ref m_VBaseRangeMax, ref m_VAllowExceedBaseRangeMax, value);
        }

        public bool scaleWithWindow
        {
            get => m_ScaleWithWindow;
            set => m_ScaleWithWindow = value;
        }

        public bool hSlider
        {
            get => m_HSlider;
            set
            {
                var rect = this.rect;
                m_HSlider = value;
                this.rect = rect;
            }
        }

        public bool vSlider
        {
            get => m_VSlider;
            set
            {
                var rect = this.rect;
                m_VSlider = value;
                this.rect = rect;
            }
        }

        public bool uniformScale
        {
            get => m_UniformScale;
            set => m_UniformScale = value;
        }

        public bool ignoreScrollWheelUntilClicked
        {
            get => m_IgnoreScrollWheelUntilClicked;
            set => m_IgnoreScrollWheelUntilClicked = value;
        }

        public Vector2 scale => m_Scale;

        public float margin
        {
            set
            {
                m_MarginBottom = value;
                m_MarginTop = value;
                m_MarginRight = value;
                m_MarginLeft = value;
            }
        }

        public float leftmargin
        {
            get => m_MarginLeft;
            set => m_MarginLeft = value;
        }

        public float rightmargin
        {
            get => m_MarginRight;
            set => m_MarginRight = value;
        }

        public float topmargin
        {
            get => m_MarginTop;
            set => m_MarginTop = value;
        }

        public float bottommargin
        {
            get => m_MarginBottom;
            set => m_MarginBottom = value;
        }

        public Rect rect
        {
            get => new Rect(drawRect.x, drawRect.y, drawRect.width + (!m_VSlider ? 0f : styles.visualSliderWidth),
                drawRect.height + (!m_HSlider ? 0f : styles.visualSliderWidth));
            set
            {
                var rect = new Rect(value.x, value.y, value.width - (!m_VSlider ? 0f : styles.visualSliderWidth),
                    value.height - (!m_HSlider ? 0f : styles.visualSliderWidth));
                if (rect != m_DrawArea)
                {
                    if (m_ScaleWithWindow)
                    {
                        m_DrawArea = rect;
                        shownAreaInsideMargins = m_LastShownAreaInsideMargins;
                    }
                    else
                    {
                        m_Translation += new Vector2((rect.width - m_DrawArea.width) / 2f,
                            (rect.height - m_DrawArea.height) / 2f);
                        m_DrawArea = rect;
                    }
                }

                EnforceScaleAndRange();
            }
        }

        public Rect drawRect => m_DrawArea;

        public Rect shownArea
        {
            get => new Rect(-m_Translation.x / m_Scale.x, -(m_Translation.y - drawRect.height) / m_Scale.y,
                drawRect.width / m_Scale.x, drawRect.height / -m_Scale.y);
            set
            {
                m_Scale.x = drawRect.width / value.width;
                m_Scale.y = -drawRect.height / value.height;
                m_Translation.x = -value.x * m_Scale.x;
                m_Translation.y = drawRect.height - value.y * m_Scale.y;
                EnforceScaleAndRange();
            }
        }

        public Rect shownAreaInsideMargins
        {
            get => shownAreaInsideMarginsInternal;
            set
            {
                shownAreaInsideMarginsInternal = value;
                EnforceScaleAndRange();
            }
        }

        private Rect shownAreaInsideMarginsInternal
        {
            get
            {
                var num = leftmargin / m_Scale.x;
                var num2 = rightmargin / m_Scale.x;
                var num3 = topmargin / m_Scale.y;
                var num4 = bottommargin / m_Scale.y;
                var shownArea = this.shownArea;
                shownArea.x += num;
                shownArea.y -= num3;
                shownArea.width -= num + num2;
                shownArea.height += num3 + num4;
                return shownArea;
            }
            set
            {
                m_Scale.x = (drawRect.width - leftmargin - rightmargin) / value.width;
                m_Scale.y = -(drawRect.height - topmargin - bottommargin) / value.height;
                m_Translation.x = -value.x * m_Scale.x + leftmargin;
                m_Translation.y = drawRect.height - value.y * m_Scale.y - topmargin;
            }
        }

        public virtual Bounds drawingBounds =>
            new Bounds(new Vector3((hBaseRangeMin + hBaseRangeMax) * 0.5f, (vBaseRangeMin + vBaseRangeMax) * 0.5f, 0f),
                new Vector3(hBaseRangeMax - hBaseRangeMin, vBaseRangeMax - vBaseRangeMin, 1f));

        public Matrix4x4 drawingToViewMatrix =>
            Matrix4x4.TRS(m_Translation, Quaternion.identity, new Vector3(m_Scale.x, m_Scale.y, 1f));

        public Vector2 mousePositionInDrawing => ViewToDrawingTransformPoint(Event.current.mousePosition);

        private void SetAllowExceed(ref float rangeEnd, ref bool allowExceed, float value)
        {
            if (value == float.NegativeInfinity || value == float.PositiveInfinity)
            {
                rangeEnd = value != float.NegativeInfinity ? 1 : 0;
                allowExceed = true;
            }
            else
            {
                rangeEnd = value;
                allowExceed = false;
            }
        }

        internal void SetDrawRectHack(Rect r, bool scrollbars)
        {
            m_DrawArea = r;
            m_VSlider = scrollbars;
            m_HSlider = scrollbars;
        }

        public void OnEnable()
        {
            styles = new Styles(m_MinimalGUI);
        }

        public void SetShownHRangeInsideMargins(float min, float max)
        {
            m_Scale.x = (drawRect.width - leftmargin - rightmargin) / (max - min);
            m_Translation.x = -min * m_Scale.x + leftmargin;
            EnforceScaleAndRange();
        }

        public void SetShownHRange(float min, float max)
        {
            m_Scale.x = drawRect.width / (max - min);
            m_Translation.x = -min * m_Scale.x;
            EnforceScaleAndRange();
        }

        public void SetShownVRangeInsideMargins(float min, float max)
        {
            m_Scale.y = -(drawRect.height - topmargin - bottommargin) / (max - min);
            m_Translation.y = drawRect.height - min * m_Scale.y - topmargin;
            EnforceScaleAndRange();
        }

        public void SetShownVRange(float min, float max)
        {
            m_Scale.y = -drawRect.height / (max - min);
            m_Translation.y = drawRect.height - min * m_Scale.y;
            EnforceScaleAndRange();
        }

        public Vector2 DrawingToViewTransformPoint(Vector2 lhs)
        {
            return new Vector2(lhs.x * m_Scale.x + m_Translation.x, lhs.y * m_Scale.y + m_Translation.y);
        }

        public Vector3 DrawingToViewTransformPoint(Vector3 lhs)
        {
            return new Vector3(lhs.x * m_Scale.x + m_Translation.x, lhs.y * m_Scale.y + m_Translation.y, 0f);
        }

        public Vector2 ViewToDrawingTransformPoint(Vector2 lhs)
        {
            return new Vector2((lhs.x - m_Translation.x) / m_Scale.x, (lhs.y - m_Translation.y) / m_Scale.y);
        }

        public Vector3 ViewToDrawingTransformPoint(Vector3 lhs)
        {
            return new Vector3((lhs.x - m_Translation.x) / m_Scale.x, (lhs.y - m_Translation.y) / m_Scale.y, 0f);
        }

        public Vector2 DrawingToViewTransformVector(Vector2 lhs)
        {
            return new Vector2(lhs.x * m_Scale.x, lhs.y * m_Scale.y);
        }

        public Vector3 DrawingToViewTransformVector(Vector3 lhs)
        {
            return new Vector3(lhs.x * m_Scale.x, lhs.y * m_Scale.y, 0f);
        }

        public Vector2 ViewToDrawingTransformVector(Vector2 lhs)
        {
            return new Vector2(lhs.x / m_Scale.x, lhs.y / m_Scale.y);
        }

        public Vector3 ViewToDrawingTransformVector(Vector3 lhs)
        {
            return new Vector3(lhs.x / m_Scale.x, lhs.y / m_Scale.y, 0f);
        }

        public Vector2 NormalizeInViewSpace(Vector2 vec)
        {
            vec = Vector2.Scale(vec, m_Scale);
            vec /= vec.magnitude;
            return Vector2.Scale(vec, new Vector2(1f / m_Scale.x, 1f / m_Scale.y));
        }

        private bool IsZoomEvent()
        {
            return Event.current.button == 1 && Event.current.alt;
        }

        private bool IsPanEvent()
        {
            return Event.current.button == 0 && Event.current.alt ||
                   Event.current.button == 2 && !Event.current.command;
        }

        public void BeginViewGUI()
        {
            if (styles.horizontalScrollbar == null) styles.InitGUIStyles(m_MinimalGUI);
            HandleZoomAndPanEvents(m_DrawArea);
            horizontalScrollbarID = GUIUtility.GetControlID(EditorGUIExtra.s_MinMaxSliderHash, FocusType.Passive);
            verticalScrollbarID = GUIUtility.GetControlID(EditorGUIExtra.s_MinMaxSliderHash, FocusType.Passive);
            if (!m_MinimalGUI || Event.current.type != EventType.Repaint) SliderGUI();
        }

        public void HandleZoomAndPanEvents(Rect area)
        {
            GUILayout.BeginArea(area);
            area.x = 0f;
            area.y = 0f;
            var controlID = GUIUtility.GetControlID(zoomableAreaHash, FocusType.Passive, area);
            switch (Event.current.GetTypeForControl(controlID))
            {
                case EventType.MouseDown:
                    if (area.Contains(Event.current.mousePosition))
                    {
                        GUIUtility.keyboardControl = controlID;
                        if (IsZoomEvent() || IsPanEvent())
                        {
                            GUIUtility.hotControl = controlID;
                            m_MouseDownPosition = mousePositionInDrawing;
                            Event.current.Use();
                        }
                    }

                    break;
                case EventType.MouseUp:
                    if (GUIUtility.hotControl == controlID)
                    {
                        GUIUtility.hotControl = 0;
                        m_MouseDownPosition = new Vector2(-1000000f, -1000000f);
                    }

                    break;
                case EventType.MouseDrag:
                    if (GUIUtility.hotControl == controlID)
                    {
                        if (IsZoomEvent())
                        {
                            Zoom(m_MouseDownPosition, false);
                            Event.current.Use();
                        }
                        else
                        {
                            if (IsPanEvent())
                            {
                                Pan();
                                Event.current.Use();
                            }
                        }
                    }

                    break;
                case EventType.ScrollWheel:
                    if (area.Contains(Event.current.mousePosition))
                        if (!m_IgnoreScrollWheelUntilClicked || GUIUtility.keyboardControl == controlID)
                        {
                            Zoom(mousePositionInDrawing, true);
                            Event.current.Use();
                        }

                    break;
            }

            GUILayout.EndArea();
        }

        public void EndViewGUI()
        {
            if (m_MinimalGUI && Event.current.type == EventType.Repaint) SliderGUI();
        }

        private void SliderGUI()
        {
            if (!m_HSlider && !m_VSlider) return;
            var drawingBounds = this.drawingBounds;
            var shownAreaInsideMargins = this.shownAreaInsideMargins;
            var num = styles.sliderWidth - styles.visualSliderWidth;
            var num2 = !vSlider || !hSlider ? 0f : num;
            var a = m_Scale;
            if (m_HSlider)
            {
                var position = new Rect(drawRect.x + 1f, drawRect.yMax - num, drawRect.width - num2,
                    styles.sliderWidth);
                var width = shownAreaInsideMargins.width;
                var xMin = shownAreaInsideMargins.xMin;
                EditorGUIExtra.MinMaxScroller(position, horizontalScrollbarID, ref xMin, ref width, drawingBounds.min.x,
                    drawingBounds.max.x, float.NegativeInfinity, float.PositiveInfinity, styles.horizontalScrollbar,
                    styles.horizontalMinMaxScrollbarThumb, styles.horizontalScrollbarLeftButton,
                    styles.horizontalScrollbarRightButton, true);
                var num3 = xMin;
                var num4 = xMin + width;
                if (num3 > shownAreaInsideMargins.xMin) num3 = Mathf.Min(num3, num4 - m_HScaleMin);
                if (num4 < shownAreaInsideMargins.xMax) num4 = Mathf.Max(num4, num3 + m_HScaleMin);
                SetShownHRangeInsideMargins(num3, num4);
            }

            if (m_VSlider)
            {
                var position2 = new Rect(drawRect.xMax - num, drawRect.y, styles.sliderWidth, drawRect.height - num2);
                var height = shownAreaInsideMargins.height;
                var num5 = -shownAreaInsideMargins.yMax;
                EditorGUIExtra.MinMaxScroller(position2, verticalScrollbarID, ref num5, ref height,
                    -drawingBounds.max.y, -drawingBounds.min.y, float.NegativeInfinity, float.PositiveInfinity,
                    styles.verticalScrollbar, styles.verticalMinMaxScrollbarThumb, styles.verticalScrollbarUpButton,
                    styles.verticalScrollbarDownButton, false);
                var num3 = -(num5 + height);
                var num4 = -num5;
                if (num3 > shownAreaInsideMargins.yMin) num3 = Mathf.Min(num3, num4 - m_VScaleMin);
                if (num4 < shownAreaInsideMargins.yMax) num4 = Mathf.Max(num4, num3 + m_VScaleMin);
                SetShownVRangeInsideMargins(num3, num4);
            }

            if (uniformScale)
            {
                var num6 = drawRect.width / drawRect.height;
                a -= m_Scale;
                var b = new Vector2(-a.y * num6, -a.x / num6);
                m_Scale -= b;
                m_Translation.x = m_Translation.x - a.y / 2f;
                m_Translation.y = m_Translation.y - a.x / 2f;
                EnforceScaleAndRange();
            }
        }

        private void Pan()
        {
            if (!m_HRangeLocked) m_Translation.x = m_Translation.x + Event.current.delta.x;
            if (!m_VRangeLocked) m_Translation.y = m_Translation.y + Event.current.delta.y;
            EnforceScaleAndRange();
        }

        private void Zoom(Vector2 zoomAround, bool scrollwhell)
        {
            var num = Event.current.delta.x + Event.current.delta.y;
            if (scrollwhell) num = -num;
            var num2 = Mathf.Max(0.01f, 1f + num * 0.01f);
            if (!m_HRangeLocked)
            {
                m_Translation.x = m_Translation.x - zoomAround.x * (num2 - 1f) * m_Scale.x;
                m_Scale.x = m_Scale.x * num2;
            }

            if (!m_VRangeLocked)
            {
                m_Translation.y = m_Translation.y - zoomAround.y * (num2 - 1f) * m_Scale.y;
                m_Scale.y = m_Scale.y * num2;
            }

            EnforceScaleAndRange();
        }

        private void EnforceScaleAndRange()
        {
            var hScaleMin = m_HScaleMin;
            var vScaleMin = m_VScaleMin;
            var value = m_HScaleMax;
            var value2 = m_VScaleMax;
            if (hRangeMax != float.PositiveInfinity && hRangeMin != float.NegativeInfinity)
                value = Mathf.Min(m_HScaleMax, hRangeMax - hRangeMin);
            if (vRangeMax != float.PositiveInfinity && vRangeMin != float.NegativeInfinity)
                value2 = Mathf.Min(m_VScaleMax, vRangeMax - vRangeMin);
            var lastShownAreaInsideMargins = m_LastShownAreaInsideMargins;
            var shownAreaInsideMargins = this.shownAreaInsideMargins;
            if (shownAreaInsideMargins == lastShownAreaInsideMargins) return;
            var num = 1E-05f;
            if (shownAreaInsideMargins.width < lastShownAreaInsideMargins.width - num)
            {
                var t = Mathf.InverseLerp(lastShownAreaInsideMargins.width, shownAreaInsideMargins.width, hScaleMin);
                shownAreaInsideMargins = new Rect(Mathf.Lerp(lastShownAreaInsideMargins.x, shownAreaInsideMargins.x, t),
                    shownAreaInsideMargins.y,
                    Mathf.Lerp(lastShownAreaInsideMargins.width, shownAreaInsideMargins.width, t),
                    shownAreaInsideMargins.height);
            }

            if (shownAreaInsideMargins.height < lastShownAreaInsideMargins.height - num)
            {
                var t2 = Mathf.InverseLerp(lastShownAreaInsideMargins.height, shownAreaInsideMargins.height, vScaleMin);
                shownAreaInsideMargins = new Rect(shownAreaInsideMargins.x,
                    Mathf.Lerp(lastShownAreaInsideMargins.y, shownAreaInsideMargins.y, t2),
                    shownAreaInsideMargins.width,
                    Mathf.Lerp(lastShownAreaInsideMargins.height, shownAreaInsideMargins.height, t2));
            }

            if (shownAreaInsideMargins.width > lastShownAreaInsideMargins.width + num)
            {
                var t3 = Mathf.InverseLerp(lastShownAreaInsideMargins.width, shownAreaInsideMargins.width, value);
                shownAreaInsideMargins = new Rect(
                    Mathf.Lerp(lastShownAreaInsideMargins.x, shownAreaInsideMargins.x, t3), shownAreaInsideMargins.y,
                    Mathf.Lerp(lastShownAreaInsideMargins.width, shownAreaInsideMargins.width, t3),
                    shownAreaInsideMargins.height);
            }

            if (shownAreaInsideMargins.height > lastShownAreaInsideMargins.height + num)
            {
                var t4 = Mathf.InverseLerp(lastShownAreaInsideMargins.height, shownAreaInsideMargins.height, value2);
                shownAreaInsideMargins = new Rect(shownAreaInsideMargins.x,
                    Mathf.Lerp(lastShownAreaInsideMargins.y, shownAreaInsideMargins.y, t4),
                    shownAreaInsideMargins.width,
                    Mathf.Lerp(lastShownAreaInsideMargins.height, shownAreaInsideMargins.height, t4));
            }

            if (shownAreaInsideMargins.xMin < hRangeMin) shownAreaInsideMargins.x = hRangeMin;
            if (shownAreaInsideMargins.xMax > hRangeMax)
                shownAreaInsideMargins.x = hRangeMax - shownAreaInsideMargins.width;
            if (shownAreaInsideMargins.yMin < vRangeMin) shownAreaInsideMargins.y = vRangeMin;
            if (shownAreaInsideMargins.yMax > vRangeMax)
                shownAreaInsideMargins.y = vRangeMax - shownAreaInsideMargins.height;
            shownAreaInsideMarginsInternal = shownAreaInsideMargins;
            m_LastShownAreaInsideMargins = shownAreaInsideMargins;
        }

        public float PixelToTime(float pixelX, Rect rect)
        {
            return (pixelX - rect.x) * shownArea.width / rect.width + shownArea.x;
        }

        public float TimeToPixel(float time, Rect rect)
        {
            return (time - shownArea.x) / shownArea.width * rect.width + rect.x;
        }

        public float PixelDeltaToTime(Rect rect)
        {
            return shownArea.width / rect.width;
        }

        [Serializable]
        public class Styles
        {
            public GUIStyle horizontalScrollbar;
            public GUIStyle horizontalMinMaxScrollbarThumb;
            public GUIStyle horizontalScrollbarLeftButton;
            public GUIStyle horizontalScrollbarRightButton;
            public GUIStyle verticalScrollbar;
            public GUIStyle verticalMinMaxScrollbarThumb;
            public GUIStyle verticalScrollbarUpButton;
            public GUIStyle verticalScrollbarDownButton;
            public float sliderWidth;
            public float visualSliderWidth;

            public Styles(bool minimalGUI)
            {
                if (minimalGUI)
                {
                    visualSliderWidth = 0f;
                    sliderWidth = 15f;
                }
                else
                {
                    visualSliderWidth = 15f;
                    sliderWidth = 15f;
                }
            }

            public void InitGUIStyles(bool minimalGUI)
            {
                if (minimalGUI)
                {
                    horizontalMinMaxScrollbarThumb = "MiniMinMaxSliderHorizontal";
                    horizontalScrollbarLeftButton = GUIStyle.none;
                    horizontalScrollbarRightButton = GUIStyle.none;
                    horizontalScrollbar = GUIStyle.none;
                    verticalMinMaxScrollbarThumb = "MiniMinMaxSlidervertical";
                    verticalScrollbarUpButton = GUIStyle.none;
                    verticalScrollbarDownButton = GUIStyle.none;
                    verticalScrollbar = GUIStyle.none;
                }
                else
                {
                    horizontalMinMaxScrollbarThumb = "horizontalMinMaxScrollbarThumb";
                    horizontalScrollbarLeftButton = "horizontalScrollbarLeftbutton";
                    horizontalScrollbarRightButton = "horizontalScrollbarRightbutton";
                    horizontalScrollbar = GUI.skin.horizontalScrollbar;
                    verticalMinMaxScrollbarThumb = "verticalMinMaxScrollbarThumb";
                    verticalScrollbarUpButton = "verticalScrollbarUpbutton";
                    verticalScrollbarDownButton = "verticalScrollbarDownbutton";
                    verticalScrollbar = GUI.skin.verticalScrollbar;
                }
            }
        }
    }
}