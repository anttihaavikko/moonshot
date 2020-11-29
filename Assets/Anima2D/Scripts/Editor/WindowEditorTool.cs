using UnityEditor;
using UnityEngine;

namespace Anima2D
{
    public abstract class WindowEditorTool
    {
        public delegate bool BoolCallback();

        public delegate void Callback();

        private static int s_WindowID;

        public BoolCallback canShow;

        private int m_WindowID = -1;
        public Callback onGUIChanged;
        public Callback onHide;
        public Callback onShow;

        public Rect windowRect = new Rect(0f, 0f, 100f, 100f);
        public string header => GetHeader();

        public int windowID
        {
            get
            {
                if (m_WindowID < 0) m_WindowID = ++s_WindowID;
                return m_WindowID;
            }
        }

        public bool isShown { get; private set; }

        public bool isHovered => isShown && windowRect.Contains(Event.current.mousePosition);

        protected virtual bool CanShow()
        {
            if (canShow != null) return canShow();

            return true;
        }

        protected virtual void DoShow()
        {
            if (onShow != null) onShow();
        }

        protected virtual void DoGUIChanged()
        {
            if (onGUIChanged != null) onGUIChanged();
        }

        protected virtual void DoHide()
        {
            if (onHide != null) onHide();
        }

        public virtual void OnWindowGUI(Rect viewRect)
        {
            if (!isShown && CanShow())
            {
                isShown = true;
                DoShow();
            }

            if (isShown && !CanShow())
            {
                isShown = false;
                DoHide();
            }

            if (CanShow())
            {
                windowRect = GUILayout.Window(windowID, windowRect, DoWindow, header);

                DoGUI();

                if (isHovered)
                {
                    var controlID = GUIUtility.GetControlID("WindowHovered".GetHashCode(), FocusType.Passive);

                    if (Event.current.GetTypeForControl(controlID) == EventType.Layout)
                        HandleUtility.AddControl(controlID, 0f);
                }
            }
        }

        protected abstract string GetHeader();
        protected abstract void DoWindow(int windowId);

        protected virtual void DoGUI()
        {
        }
    }
}