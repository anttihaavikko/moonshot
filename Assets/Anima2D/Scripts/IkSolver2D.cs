using System;
using System.Collections.Generic;
using UnityEngine;

namespace Anima2D
{
    [Serializable]
    public abstract class IkSolver2D
    {
        [SerializeField] private Transform m_RootBoneTransform;

        [SerializeField] private List<SolverPose> m_SolverPoses = new List<SolverPose>();
        [SerializeField] private float m_Weight = 1f;
        [SerializeField] private bool m_RestoreDefaultPose = true;

        public Vector3 targetPosition;

        private Bone2D m_CachedRootBone;

        public Bone2D rootBone
        {
            get
            {
                if (m_CachedRootBone && m_RootBoneTransform != m_CachedRootBone.transform) m_CachedRootBone = null;

                if (!m_CachedRootBone && m_RootBoneTransform)
                    m_CachedRootBone = m_RootBoneTransform.GetComponent<Bone2D>();

                return m_CachedRootBone;
            }
            private set
            {
                m_CachedRootBone = value;
                m_RootBoneTransform = null;

                if (value) m_RootBoneTransform = value.transform;
            }
        }

        public List<SolverPose> solverPoses => m_SolverPoses;

        public float weight
        {
            get => m_Weight;
            set => m_Weight = Mathf.Clamp01(value);
        }

        public bool restoreDefaultPose
        {
            get => m_RestoreDefaultPose;
            set => m_RestoreDefaultPose = value;
        }

        public void Initialize(Bone2D _rootBone, int numChilds)
        {
            rootBone = _rootBone;

            var bone = rootBone;
            solverPoses.Clear();

            for (var i = 0; i < numChilds; ++i)
                if (bone)
                {
                    var solverPose = new SolverPose();
                    solverPose.bone = bone;
                    solverPoses.Add(solverPose);
                    bone = bone.child;
                }

            StoreDefaultPoses();
        }

        public void Update()
        {
            if (weight > 0f)
            {
                if (restoreDefaultPose) RestoreDefaultPoses();

                DoSolverUpdate();
                UpdateBones();
            }
        }

        public void StoreDefaultPoses()
        {
            for (var i = 0; i < solverPoses.Count; i++)
            {
                var pose = solverPoses[i];

                if (pose != null) pose.StoreDefaultPose();
            }
        }

        public void RestoreDefaultPoses()
        {
            for (var i = 0; i < solverPoses.Count; i++)
            {
                var pose = solverPoses[i];

                if (pose != null) pose.RestoreDefaultPose();
            }
        }

        private void UpdateBones()
        {
            for (var i = 0; i < solverPoses.Count; ++i)
            {
                var solverPose = solverPoses[i];

                if (solverPose != null && solverPose.bone)
                {
                    if (weight == 1f)
                        solverPose.bone.transform.localRotation = solverPose.solverRotation;
                    else
                        solverPose.bone.transform.localRotation = Quaternion.Slerp(
                            solverPose.bone.transform.localRotation,
                            solverPose.solverRotation,
                            weight);
                }
            }
        }

        protected abstract void DoSolverUpdate();

        [Serializable]
        public class SolverPose
        {
            [SerializeField] private Transform m_BoneTransform;

            public Vector3 solverPosition = Vector3.zero;
            public Quaternion solverRotation = Quaternion.identity;
            public Quaternion defaultLocalRotation = Quaternion.identity;

            private Bone2D m_CachedBone;

            public Bone2D bone
            {
                get
                {
                    if (m_CachedBone && m_BoneTransform != m_CachedBone.transform) m_CachedBone = null;

                    if (!m_CachedBone && m_BoneTransform) m_CachedBone = m_BoneTransform.GetComponent<Bone2D>();

                    return m_CachedBone;
                }

                set
                {
                    m_CachedBone = value;
                    m_BoneTransform = null;

                    if (value) m_BoneTransform = m_CachedBone.transform;
                }
            }

            public void StoreDefaultPose()
            {
                defaultLocalRotation = bone.transform.localRotation;
            }

            public void RestoreDefaultPose()
            {
                if (bone) bone.transform.localRotation = defaultLocalRotation;
            }
        }
    }
}