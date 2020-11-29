using UnityEngine;

namespace Anima2D
{
    public class TransformTracker
    {
        private readonly Vector3 m_LocalScale;
        private readonly Vector3 m_Position;
        private readonly Quaternion m_Rotation;
        private readonly Transform m_Transform;

        public TransformTracker(Transform transform)
        {
            m_Transform = transform;
            m_Position = m_Transform.position;
            m_Rotation = m_Transform.rotation;
            m_LocalScale = m_Transform.localScale;
        }

        public bool changed =>
            !m_Transform ||
            m_Transform.position != m_Position ||
            m_Transform.rotation != m_Rotation ||
            m_Transform.localScale != m_LocalScale;
    }
}