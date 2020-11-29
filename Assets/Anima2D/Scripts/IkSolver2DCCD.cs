using System;
using UnityEngine;

namespace Anima2D
{
    [Serializable]
    public class IkSolver2DCCD : IkSolver2D
    {
        public int iterations = 10;
        public float damping = 0.8f;

        protected override void DoSolverUpdate()
        {
            if (!rootBone) return;

            for (var i = 0; i < solverPoses.Count; ++i)
            {
                var solverPose = solverPoses[i];

                if (solverPose != null && solverPose.bone)
                {
                    solverPose.solverRotation = solverPose.bone.transform.localRotation;
                    solverPose.solverPosition =
                        rootBone.transform.InverseTransformPoint(solverPose.bone.transform.position);
                }
            }

            var localEndPosition =
                rootBone.transform.InverseTransformPoint(solverPoses[solverPoses.Count - 1].bone.endPosition);
            var localTargetPosition = rootBone.transform.InverseTransformPoint(targetPosition);

            damping = Mathf.Clamp01(damping);

            var l_damping = 1f - Mathf.Lerp(0f, 0.99f, damping);

            for (var i = 0; i < iterations; ++i)
            for (var j = solverPoses.Count - 1; j >= 0; --j)
            {
                var solverPose = solverPoses[j];

                var toTarget = localTargetPosition - solverPose.solverPosition;
                var toEnd = localEndPosition - solverPose.solverPosition;
                toTarget.z = 0f;
                toEnd.z = 0f;

                var localAngleDelta = MathUtils.SignedAngle(toEnd, toTarget, Vector3.forward);

                localAngleDelta *= l_damping;

                var localRotation = Quaternion.AngleAxis(localAngleDelta, Vector3.forward);

                solverPose.solverRotation = solverPose.solverRotation * localRotation;

                var pivotPosition = solverPose.solverPosition;

                localEndPosition = RotatePositionFrom(localEndPosition, pivotPosition, localRotation);

                for (var k = solverPoses.Count - 1; k > j; --k)
                {
                    var sp = solverPoses[k];
                    sp.solverPosition = RotatePositionFrom(sp.solverPosition, pivotPosition, localRotation);
                }
            }
        }

        private Vector3 RotatePositionFrom(Vector3 position, Vector3 pivot, Quaternion rotation)
        {
            var v = position - pivot;
            v = rotation * v;
            return pivot + v;
        }
    }
}