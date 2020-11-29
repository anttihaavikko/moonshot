using System;
using UnityEditor;
using UnityEngine;

namespace Anima2D
{
    [Serializable]
    internal class TimeArea : ZoomableArea
    {
        public enum TimeRulerDragMode
        {
            None,
            Start,
            End,
            Dragging,
            Cancel
        }

        internal const int kTickRulerDistMin = 3;
        internal const int kTickRulerDistFull = 80;
        internal const int kTickRulerDistLabel = 40;
        internal const float kTickRulerHeightMax = 0.7f;
        internal const float kTickRulerFatThreshold = 0.5f;
        private static Styles2 styles;
        private static float s_OriginalTime;
        private static float s_PickOffset;
        private TickHandler m_HTicks;
        private TickHandler m_VTicks;

        public TimeArea(bool minimalGUI) : base(minimalGUI)
        {
            float[] tickModulos =
            {
                1E-07f,
                5E-07f,
                1E-06f,
                5E-06f,
                1E-05f,
                5E-05f,
                0.0001f,
                0.0005f,
                0.001f,
                0.005f,
                0.01f,
                0.05f,
                0.1f,
                0.5f,
                1f,
                5f,
                10f,
                50f,
                100f,
                500f,
                1000f,
                5000f,
                10000f,
                50000f,
                100000f,
                500000f,
                1000000f,
                5000000f,
                1E+07f
            };
            hTicks = new TickHandler();
            hTicks.SetTickModulos(tickModulos);
            vTicks = new TickHandler();
            vTicks.SetTickModulos(tickModulos);
        }

        public TickHandler hTicks
        {
            get => m_HTicks;
            set => m_HTicks = value;
        }

        public TickHandler vTicks
        {
            get => m_VTicks;
            set => m_VTicks = value;
        }

        private static void InitStyles()
        {
            if (styles == null) styles = new Styles2();
        }

        private void SetTickMarkerRanges()
        {
            hTicks.SetRanges(shownArea.xMin, shownArea.xMax, drawRect.xMin, drawRect.xMax);
            vTicks.SetRanges(shownArea.yMin, shownArea.yMax, drawRect.yMin, drawRect.yMax);
        }

        public void DrawMajorTicks(Rect position, float frameRate)
        {
            var color = Handles.color;
            GUI.BeginGroup(position);
            if (Event.current.type != EventType.Repaint)
            {
                GUI.EndGroup();
                return;
            }

            InitStyles();
            SetTickMarkerRanges();
            hTicks.SetTickStrengths(3f, 80f, true);
            var textColor = styles.TimelineTick.normal.textColor;
            textColor.a = 0.1f;
            Handles.color = textColor;
            for (var i = 0; i < hTicks.tickLevels; i++)
            {
                var num = hTicks.GetStrengthOfLevel(i) * 0.9f;
                if (num > 0.5f)
                {
                    var ticksAtLevel = hTicks.GetTicksAtLevel(i, true);
                    for (var j = 0; j < ticksAtLevel.Length; j++)
                        if (ticksAtLevel[j] >= 0f)
                        {
                            var num2 = Mathf.RoundToInt(ticksAtLevel[j] * frameRate);
                            var x = FrameToPixel(num2, frameRate, position);
                            Handles.DrawLine(new Vector3(x, 0f, 0f), new Vector3(x, position.height, 0f));
                        }
                }
            }

            GUI.EndGroup();
            Handles.color = color;
        }

        public void TimeRuler(Rect position, float frameRate)
        {
            var color = GUI.color;
            GUI.BeginGroup(position);
            if (Event.current.type != EventType.Repaint)
            {
                GUI.EndGroup();
                return;
            }

            InitStyles();
            HandlesExtra.ApplyWireMaterial();
            GL.Begin(1);
            var backgroundColor = GUI.backgroundColor;
            SetTickMarkerRanges();
            hTicks.SetTickStrengths(3f, 80f, true);
            var textColor = styles.TimelineTick.normal.textColor;
            textColor.a = 0.75f;
            for (var i = 0; i < hTicks.tickLevels; i++)
            {
                var num = hTicks.GetStrengthOfLevel(i) * 0.9f;
                var ticksAtLevel = hTicks.GetTicksAtLevel(i, true);
                for (var j = 0; j < ticksAtLevel.Length; j++)
                    if (ticksAtLevel[j] >= hRangeMin && ticksAtLevel[j] <= hRangeMax)
                    {
                        var num2 = Mathf.RoundToInt(ticksAtLevel[j] * frameRate);
                        var num3 = position.height * Mathf.Min(1f, num) * 0.7f;
                        var num4 = FrameToPixel(num2, frameRate, position);
                        GL.Color(new Color(1f, 1f, 1f, num / 0.5f) * textColor);
                        GL.Vertex(new Vector3(num4, position.height - num3 + 0.5f, 0f));
                        GL.Vertex(new Vector3(num4, position.height - 0.5f, 0f));
                        if (num > 0.5f)
                        {
                            GL.Color(new Color(1f, 1f, 1f, num / 0.5f - 1f) * textColor);
                            GL.Vertex(new Vector3(num4 + 1f, position.height - num3 + 0.5f, 0f));
                            GL.Vertex(new Vector3(num4 + 1f, position.height - 0.5f, 0f));
                        }
                    }
            }

            GL.End();
            var levelWithMinSeparation = hTicks.GetLevelWithMinSeparation(40f);
            var ticksAtLevel2 = hTicks.GetTicksAtLevel(levelWithMinSeparation, false);
            for (var k = 0; k < ticksAtLevel2.Length; k++)
                if (ticksAtLevel2[k] >= hRangeMin && ticksAtLevel2[k] <= hRangeMax)
                {
                    var num5 = Mathf.RoundToInt(ticksAtLevel2[k] * frameRate);
                    var num6 = Mathf.Floor(FrameToPixel(num5, frameRate, rect));
                    var text = FormatFrame(num5, frameRate);
                    GUI.Label(new Rect(num6 + 3f, -3f, 40f, 20f), text, styles.TimelineTick);
                }

            GUI.EndGroup();
            GUI.backgroundColor = backgroundColor;
            GUI.color = color;
        }

        public TimeRulerDragMode BrowseRuler(Rect position, ref float time, float frameRate, bool pickAnywhere,
            GUIStyle thumbStyle)
        {
            var controlID = GUIUtility.GetControlID(3126789, FocusType.Passive);
            return BrowseRuler(position, controlID, ref time, frameRate, pickAnywhere, thumbStyle);
        }

        public TimeRulerDragMode BrowseRuler(Rect position, int id, ref float time, float frameRate, bool pickAnywhere,
            GUIStyle thumbStyle)
        {
            var current = Event.current;
            var position2 = position;
            if (time != -1f)
            {
                position2.x = Mathf.Round(TimeToPixel(time, position)) - thumbStyle.overflow.left;
                position2.width = thumbStyle.fixedWidth + thumbStyle.overflow.horizontal;
            }

            switch (current.GetTypeForControl(id))
            {
                case EventType.MouseDown:
                    if (position2.Contains(current.mousePosition))
                    {
                        GUIUtility.hotControl = id;
                        s_PickOffset = current.mousePosition.x - TimeToPixel(time, position);
                        current.Use();
                        return TimeRulerDragMode.Start;
                    }

                    if (pickAnywhere && position.Contains(current.mousePosition))
                    {
                        GUIUtility.hotControl = id;
                        var num = SnapTimeToWholeFPS(PixelToTime(current.mousePosition.x, position), frameRate);
                        s_OriginalTime = time;
                        if (num != time) GUI.changed = true;
                        time = num;
                        s_PickOffset = 0f;
                        current.Use();
                        return TimeRulerDragMode.Start;
                    }

                    break;
                case EventType.MouseUp:
                    if (GUIUtility.hotControl == id)
                    {
                        GUIUtility.hotControl = 0;
                        current.Use();
                        return TimeRulerDragMode.End;
                    }

                    break;
                case EventType.MouseDrag:
                    if (GUIUtility.hotControl == id)
                    {
                        var num2 = SnapTimeToWholeFPS(PixelToTime(current.mousePosition.x - s_PickOffset, position),
                            frameRate);
                        if (num2 != time) GUI.changed = true;
                        time = num2;
                        current.Use();
                        return TimeRulerDragMode.Dragging;
                    }

                    break;
                case EventType.KeyDown:
                    if (GUIUtility.hotControl == id && current.keyCode == KeyCode.Escape)
                    {
                        if (time != s_OriginalTime) GUI.changed = true;
                        time = s_OriginalTime;
                        GUIUtility.hotControl = 0;
                        current.Use();
                        return TimeRulerDragMode.Cancel;
                    }

                    break;
                case EventType.Repaint:
                    if (time != -1f)
                    {
                        var flag = position.Contains(current.mousePosition);
                        position2.x += thumbStyle.overflow.left;
                        thumbStyle.Draw(position2, id == GUIUtility.hotControl, flag || id == GUIUtility.hotControl,
                            false, false);
                    }

                    break;
            }

            return TimeRulerDragMode.None;
        }

        private void DrawLine(Vector2 lhs, Vector2 rhs)
        {
            GL.Vertex(DrawingToViewTransformPoint(new Vector3(lhs.x, lhs.y, 0f)));
            GL.Vertex(DrawingToViewTransformPoint(new Vector3(rhs.x, rhs.y, 0f)));
        }

        public float FrameToPixel(float i, float frameRate, Rect rect)
        {
            return (i - shownArea.xMin * frameRate) * rect.width / (shownArea.width * frameRate);
        }

        public string FormatFrame(int frame, float frameRate)
        {
            var length = ((int) frameRate).ToString().Length;
            var str = string.Empty;
            if (frame < 0)
            {
                str = "-";
                frame = -frame;
            }

            return str + frame / (int) frameRate + ":" + (frame % frameRate).ToString().PadLeft(length, '0');
        }

        public static float SnapTimeToWholeFPS(float time, float frameRate)
        {
            if (frameRate == 0f) return time;
            return Mathf.Round(time * frameRate) / frameRate;
        }

        private class Styles2
        {
            public GUIStyle labelTickMarks = "CurveEditorLabelTickMarks";
            public readonly GUIStyle TimelineTick = "AnimationTimelineTick";
        }
    }
}