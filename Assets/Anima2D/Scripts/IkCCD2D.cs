using UnityEngine;

namespace Anima2D
{
    public class IkCCD2D : Ik2D
    {
        public int iterations = 10;

        [Range(0, 1)] public float damping = 0.8f;

        [SerializeField] private IkSolver2DCCD m_Solver = new IkSolver2DCCD();

        protected override IkSolver2D GetSolver()
        {
            return m_Solver;
        }

        protected override void OnIkUpdate()
        {
            base.OnIkUpdate();

            m_Solver.iterations = iterations;
            m_Solver.damping = damping;
        }
    }
}