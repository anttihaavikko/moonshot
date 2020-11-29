using System;
using UnityEngine;

namespace Anima2D
{
#if UNITY_EDITOR
    [ExecuteInEditMode]
    public class EditorUpdaterProxy : MonoBehaviour
    {
        private static EditorUpdaterProxy m_Instance;
        public Action onLateUpdate;

        public static EditorUpdaterProxy Instance
        {
            get
            {
                if (!m_Instance)
                {
                    m_Instance = FindObjectOfType<EditorUpdaterProxy>();

                    if (!m_Instance)
                    {
                        var l_instanceGO = new GameObject("EditorUpdaterProxy");

                        m_Instance = l_instanceGO.AddComponent<EditorUpdaterProxy>();

                        l_instanceGO.hideFlags = HideFlags.HideAndDontSave;
                        m_Instance.hideFlags = HideFlags.HideAndDontSave;
                    }
                }

                return m_Instance;
            }
        }

        public static bool isActive => m_Instance != null;

        private void Awake()
        {
            if (Instance == this)
            {
            }
            else
            {
                Destroy(gameObject);
            }
        }

        private void LateUpdate()
        {
            if (onLateUpdate != null) onLateUpdate.Invoke();
        }
    }
#endif
}