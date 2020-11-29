using System.Collections.Generic;

namespace Anima2D
{
    public class SpriteMeshInstanceTracker
    {
        private readonly Dictionary<int, float> m_BlendShapeWeightTracker = new Dictionary<int, float>();

        private SpriteMesh m_SpriteMesh;

        private SpriteMeshInstance m_SpriteMeshInstance;
        private readonly List<TransformTracker> m_TransformTrackers = new List<TransformTracker>();

        public SpriteMeshInstance spriteMeshInstance
        {
            get => m_SpriteMeshInstance;
            set
            {
                m_SpriteMeshInstance = value;
                Update();
            }
        }

        public bool spriteMeshChanged
        {
            get
            {
                if (m_SpriteMeshInstance) return m_SpriteMesh != m_SpriteMeshInstance.spriteMesh;

                return false;
            }
        }

        public bool changed
        {
            get
            {
                if (spriteMeshChanged) return true;

                if (m_SpriteMeshInstance)
                    if (m_SpriteMesh && m_SpriteMeshInstance.cachedSkinnedRenderer)
                    {
                        var blendShapeCount = m_SpriteMeshInstance.sharedMesh.blendShapeCount;

                        if (blendShapeCount != m_BlendShapeWeightTracker.Count) return true;

                        for (var i = 0; i < blendShapeCount; ++i)
                        {
                            var weight = 0f;

                            if (m_BlendShapeWeightTracker.TryGetValue(i, out weight))
                                if (m_SpriteMeshInstance.cachedSkinnedRenderer.GetBlendShapeWeight(i) != weight)
                                    return true;
                        }

                        foreach (var tracker in m_TransformTrackers)
                            if (tracker.changed)
                                return true;
                    }

                return false;
            }
        }

        public void Update()
        {
            m_TransformTrackers.Clear();
            m_BlendShapeWeightTracker.Clear();
            m_SpriteMesh = null;

            if (m_SpriteMeshInstance && m_SpriteMeshInstance.spriteMesh)
            {
                m_SpriteMesh = m_SpriteMeshInstance.spriteMesh;

                m_TransformTrackers.Add(new TransformTracker(m_SpriteMeshInstance.transform));

                foreach (var bone in m_SpriteMeshInstance.bones)
                    m_TransformTrackers.Add(new TransformTracker(bone.transform));

                if (m_SpriteMeshInstance.cachedSkinnedRenderer)
                {
                    var blendShapeCount = m_SpriteMeshInstance.sharedMesh.blendShapeCount;

                    for (var i = 0; i < blendShapeCount; ++i)
                        m_BlendShapeWeightTracker.Add(i,
                            m_SpriteMeshInstance.cachedSkinnedRenderer.GetBlendShapeWeight(i));
                }
            }
        }
    }
}