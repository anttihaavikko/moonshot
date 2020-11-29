using System;
using UnityEngine;

namespace Anima2D
{
    [Serializable]
    public class IkSolver2DLimb : IkSolver2D
    {
        public bool flip;

        protected override void DoSolverUpdate()
        {
            if (!rootBone || solverPoses.Count != 2) return;

            var pose0 = solverPoses[0];
            var pose1 = solverPoses[1];

            var localTargetPosition = targetPosition - rootBone.transform.position;
            localTargetPosition.z = 0f;

            var distanceMagnitude = localTargetPosition.magnitude;

            var angle0 = 0f;
            var angle1 = 0f;

            var sqrDistance = localTargetPosition.sqrMagnitude;

            var sqrParentLength = pose0.bone.length * pose0.bone.length;
            var sqrTargetLength = pose1.bone.length * pose1.bone.length;

            var angle0Cos = (sqrDistance + sqrParentLength - sqrTargetLength) /
                            (2f * pose0.bone.length * distanceMagnitude);
            var angle1Cos = (sqrDistance - sqrParentLength - sqrTargetLength) /
                            (2f * pose0.bone.length * pose1.bone.length);

            if (angle0Cos >= -1f && angle0Cos <= 1f && angle1Cos >= -1f && angle1Cos <= 1f)
            {
                angle0 = Mathf.Acos(angle0Cos) * Mathf.Rad2Deg;
                angle1 = Mathf.Acos(angle1Cos) * Mathf.Rad2Deg;
            }

            var flipSign = flip ? -1f : 1f;

            var rootBoneToTarget =
                Vector3.ProjectOnPlane(targetPosition - rootBone.transform.position, rootBone.transform.forward);

            if (rootBone.transform.parent)
                rootBoneToTarget = rootBone.transform.parent.InverseTransformDirection(rootBoneToTarget);

            var baseAngle = Mathf.Atan2(rootBoneToTarget.y, rootBoneToTarget.x) * Mathf.Rad2Deg;

            pose0.solverRotation = Quaternion.Euler(0f, 0f, baseAngle - flipSign * angle0);
            pose1.solverRotation = Quaternion.Euler(0f, 0f, flipSign * angle1);
        }
    }
}