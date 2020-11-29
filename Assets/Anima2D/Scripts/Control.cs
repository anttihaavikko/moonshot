using UnityEngine;

namespace Anima2D
{
    public class Control : MonoBehaviour
    {
        [SerializeField] private Transform m_BoneTransform;

        private Bone2D m_CachedBone;

        public Color color
        {
            get
            {
                if (m_CachedBone)
                {
                    var color = m_CachedBone.color;
                    color.a = 1f;
                    return color;
                }

                return Color.white;
            }
        }

        public Bone2D bone
        {
            get
            {
                if (m_CachedBone && m_BoneTransform != m_CachedBone.transform) m_CachedBone = null;

                if (!m_CachedBone && m_BoneTransform) m_CachedBone = m_BoneTransform.GetComponent<Bone2D>();

                return m_CachedBone;
            }
            set => m_BoneTransform = value.transform;
        }

        private void Start()
        {
        }

        private void LateUpdate()
        {
            if (bone)
            {
                transform.position = bone.transform.position;
                transform.rotation = bone.transform.rotation;
            }
        }
    }
}