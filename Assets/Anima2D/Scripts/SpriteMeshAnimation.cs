using UnityEngine;

namespace Anima2D
{
    [ExecuteInEditMode]
    [RequireComponent(typeof(SpriteMeshInstance))]
    public class SpriteMeshAnimation : MonoBehaviour
    {
        [SerializeField] private float m_Frame;

        [SerializeField] private SpriteMesh[] m_Frames;

        private int m_OldFrame;

        private SpriteMeshInstance m_SpriteMeshInstance;

        public SpriteMesh[] frames
        {
            get => m_Frames;
            set => m_Frames = value;
        }

        public SpriteMeshInstance cachedSpriteMeshInstance
        {
            get
            {
                if (!m_SpriteMeshInstance) m_SpriteMeshInstance = GetComponent<SpriteMeshInstance>();

                return m_SpriteMeshInstance;
            }
        }

        public int frame
        {
            get => (int) m_Frame;
            set => m_Frame = value;
        }

        private void LateUpdate()
        {
            if (m_OldFrame != frame &&
                m_Frames != null &&
                m_Frames.Length > 0 && m_Frames.Length > frame &&
                cachedSpriteMeshInstance)
            {
                m_OldFrame = frame;
                cachedSpriteMeshInstance.spriteMesh = m_Frames[frame];
            }
        }
    }
}