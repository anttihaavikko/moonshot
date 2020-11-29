using UnityEngine;

namespace Anima2D
{
    public class BakedMesh
    {
        private Mesh m_ProxyMesh;

        private SkinnedMeshRenderer m_SkinnedMeshRenderer;

        private Mesh proxyMesh
        {
            get
            {
                if (!m_ProxyMesh)
                {
                    m_ProxyMesh = new Mesh();
                    m_ProxyMesh.hideFlags = HideFlags.DontSave;
                    m_ProxyMesh.MarkDynamic();
                }

                return m_ProxyMesh;
            }
        }

        public Vector3[] vertices
        {
            get => proxyMesh.vertices;
            set => proxyMesh.vertices = value;
        }

        public SkinnedMeshRenderer skinnedMeshRenderer
        {
            get => m_SkinnedMeshRenderer;
            set
            {
                if (m_SkinnedMeshRenderer != value)
                {
                    m_SkinnedMeshRenderer = value;
                    Bake();
                }
            }
        }

        public void Bake()
        {
            if (skinnedMeshRenderer) skinnedMeshRenderer.BakeMesh(proxyMesh);
        }

        public void Destroy()
        {
            if (m_ProxyMesh) Object.DestroyImmediate(m_ProxyMesh);
        }
    }
}