using UnityEngine;

namespace Anima2D
{
    public abstract class Ik2D : MonoBehaviour
    {
        [SerializeField] private bool m_Record;

        [SerializeField] private Transform m_TargetTransform;

        [SerializeField] private int m_NumBones;
        [SerializeField] private float m_Weight = 1f;
        [SerializeField] private bool m_RestoreDefaultPose = true;
        [SerializeField] private bool m_OrientChild = true;

        private Bone2D m_CachedTarget;

        public IkSolver2D solver => GetSolver();

        public bool record => m_Record;

        public Bone2D target
        {
            get
            {
                if (m_CachedTarget && m_TargetTransform != m_CachedTarget.transform)
                {
                    m_CachedTarget = null;
                }

                if (!m_CachedTarget && m_TargetTransform)
                {
                    m_CachedTarget = m_TargetTransform.GetComponent<Bone2D>();
                }

                return m_CachedTarget;
            }
            set
            {
                m_CachedTarget = value;
                m_TargetTransform = value.transform;
                InitializeSolver();
            }
        }

        public int numBones
        {
            get => ValidateNumBones(m_NumBones);
            set
            {
                var l_numBones = ValidateNumBones(value);

                if (l_numBones != m_NumBones)
                {
                    m_NumBones = l_numBones;
                    InitializeSolver();
                }
            }
        }

        public float weight
        {
            get => m_Weight;
            set => m_Weight = value;
        }

        public bool restoreDefaultPose
        {
            get => m_RestoreDefaultPose;
            set => m_RestoreDefaultPose = value;
        }

        public bool orientChild
        {
            get => m_OrientChild;
            set => m_OrientChild = value;
        }

        private void Start()
        {
            OnStart();
        }

        private void Update()
        {
            SetAttachedIK(this);

            OnUpdate();
        }

        private void LateUpdate()
        {
            OnLateUpdate();

            UpdateIK();
        }

        private void OnDrawGizmos()
        {
            Gizmos.matrix = transform.localToWorldMatrix;

            if (enabled && target && numBones > 0)
            {
                Gizmos.DrawIcon(transform.position, "ikGoal");
            }
            else
            {
                Gizmos.DrawIcon(transform.position, "ikGoalDisabled");
            }
        }

        private void OnValidate()
        {
            Validate();
        }

        private void SetAttachedIK(Ik2D ik2D)
        {
            for (var i = 0; i < solver.solverPoses.Count; i++)
            {
                var pose = solver.solverPoses[i];

                if (pose.bone)
                {
                    pose.bone.attachedIK = ik2D;
                }
            }
        }

        public void UpdateIK()
        {
            OnIkUpdate();

            solver.Update();

            if (orientChild && target.child)
            {
                target.child.transform.rotation = transform.rotation;
            }
        }

        protected virtual void OnIkUpdate()
        {
            solver.weight = weight;
            solver.targetPosition = transform.position;
            solver.restoreDefaultPose = restoreDefaultPose;
        }

        private void InitializeSolver()
        {
            var rootBone = Bone2D.GetChainBoneByIndex(target, numBones - 1);

            SetAttachedIK(null);

            solver.Initialize(rootBone, numBones);
        }

        protected virtual int ValidateNumBones(int numBones)
        {
            return numBones;
        }

        protected virtual void Validate()
        {
        }

        protected virtual void OnStart()
        {
        }

        protected virtual void OnUpdate()
        {
        }

        protected virtual void OnLateUpdate()
        {
        }

        protected abstract IkSolver2D GetSolver();
    }
}