using System;
using UnityEngine;

namespace Anima2D
{
    [Serializable]
    public class SpriteMeshData : ScriptableObject
    {
        [SerializeField] private Vector2 m_PivotPoint;

        [SerializeField] private Vector2[] m_Vertices = new Vector2[0];

        [SerializeField] private BoneWeight[] m_BoneWeights = new BoneWeight[0];

        [SerializeField] private IndexedEdge[] m_Edges = new IndexedEdge[0];

        [SerializeField] private Vector2[] m_Holes = new Vector2[0];

        [SerializeField] private int[] m_Indices = new int[0];

        [SerializeField] private BindInfo[] m_BindPoses = new BindInfo[0];

        [SerializeField] private BlendShape[] m_Blendshapes = new BlendShape[0];

        public Vector2 pivotPoint
        {
            get => m_PivotPoint;
            set => m_PivotPoint = value;
        }

        public Vector2[] vertices
        {
            get => m_Vertices;
            set => m_Vertices = value;
        }

        public BoneWeight[] boneWeights
        {
            get => m_BoneWeights;
            set => m_BoneWeights = value;
        }

        public IndexedEdge[] edges
        {
            get => m_Edges;
            set => m_Edges = value;
        }

        public int[] indices
        {
            get => m_Indices;
            set => m_Indices = value;
        }

        public Vector2[] holes
        {
            get => m_Holes;
            set => m_Holes = value;
        }

        public BindInfo[] bindPoses
        {
            get => m_BindPoses;
            set => m_BindPoses = value;
        }

        public BlendShape[] blendshapes
        {
            get => m_Blendshapes;
            set => m_Blendshapes = value;
        }
    }
}