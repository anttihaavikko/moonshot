using UnityEngine;

namespace Anima2D
{
    public class Bone2D : MonoBehaviour
    {
        [SerializeField] private Color m_Color = Color.white;

        [SerializeField] private float m_Length = 1f;

        [SerializeField] [HideInInspector] private Transform m_ChildTransform;

        [SerializeField] private Ik2D m_AttachedIK;

        private Bone2D m_CachedChild;
        private Bone2D m_ParentBone;

        public Ik2D attachedIK
        {
            get => m_AttachedIK;
            set => m_AttachedIK = value;
        }

        public Color color
        {
            get => m_Color;
            set => m_Color = value;
        }

        public Bone2D child
        {
            get
            {
                if (m_CachedChild && m_ChildTransform != m_CachedChild.transform) m_CachedChild = null;

                if (m_ChildTransform && m_ChildTransform.parent != transform) m_CachedChild = null;

                if (!m_CachedChild && m_ChildTransform && m_ChildTransform.parent == transform)
                    m_CachedChild = m_ChildTransform.GetComponent<Bone2D>();

                return m_CachedChild;
            }

            set
            {
                m_CachedChild = value;
                m_ChildTransform = m_CachedChild.transform;
            }
        }

        public Vector3 localEndPosition => Vector3.right * localLength;

        public Vector3 endPosition => transform.TransformPoint(localEndPosition);

        public float localLength
        {
            get
            {
                if (child)
                {
                    var childPosition = transform.InverseTransformPoint(child.transform.position);
                    m_Length = Mathf.Clamp(childPosition.x, 0f, childPosition.x);
                }

                return m_Length;
            }

            set
            {
                if (!child) m_Length = value;
            }
        }

        public float length => transform.TransformVector(localEndPosition).magnitude;

        public Bone2D parentBone
        {
            get
            {
                var parentTransform = transform.parent;

                if (!m_ParentBone)
                {
                    if (parentTransform)
                        m_ParentBone = parentTransform.GetComponent<Bone2D>();
                }
                else if (parentTransform != m_ParentBone.transform)
                {
                    if (parentTransform)
                        m_ParentBone = parentTransform.GetComponent<Bone2D>();
                    else
                        m_ParentBone = null;
                }

                return m_ParentBone;
            }
        }

        public Bone2D linkedParentBone
        {
            get
            {
                if (parentBone && parentBone.child == this) return parentBone;

                return null;
            }
        }

        public Bone2D root
        {
            get
            {
                var rootBone = this;

                while (rootBone.parentBone) rootBone = rootBone.parentBone;

                return rootBone;
            }
        }

        public Bone2D chainRoot
        {
            get
            {
                var chainRoot = this;

                while (chainRoot.parentBone && chainRoot.parentBone.child == chainRoot)
                    chainRoot = chainRoot.parentBone;

                return chainRoot;
            }
        }

        public int chainLength
        {
            get
            {
                var chainRoot = this;

                var length = 1;

                while (chainRoot.parentBone && chainRoot.parentBone.child == chainRoot)
                {
                    ++length;
                    chainRoot = chainRoot.parentBone;
                }

                return length;
            }
        }

        private void OnDrawGizmos()
        {
        }

        public static Bone2D GetChainBoneByIndex(Bone2D chainTip, int index)
        {
            if (!chainTip)
                return null;

            var bone = chainTip;

            var chainLength = bone.chainLength;

            for (var i = 0; i < chainLength && bone; ++i)
            {
                if (i == index) return bone;

                if (bone.linkedParentBone)
                    bone = bone.parentBone;
                else
                    return null;
            }

            return null;
        }
    }
}