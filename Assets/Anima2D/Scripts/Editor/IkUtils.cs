using System.Collections.Generic;
using System.Linq;
using UnityEditor;
using UnityEngine;

namespace Anima2D
{
    public static class IkUtils
    {
        public static void InitializeIk2D(SerializedObject ikSO)
        {
            var targetTransformProp = ikSO.FindProperty("m_TargetTransform");
            var numBonesProp = ikSO.FindProperty("m_NumBones");
            var solverProp = ikSO.FindProperty("m_Solver");
            var solverPosesProp = solverProp.FindPropertyRelative("m_SolverPoses");
            var rootBoneTransformProp = solverProp.FindPropertyRelative("m_RootBoneTransform");

            var targetTransform = targetTransformProp.objectReferenceValue as Transform;
            Bone2D targetBone = null;

            if (targetTransform) targetBone = targetTransform.GetComponent<Bone2D>();

            Bone2D rootBone = null;
            Transform rootBoneTransform = null;

            if (targetBone) rootBone = Bone2D.GetChainBoneByIndex(targetBone, numBonesProp.intValue - 1);

            if (rootBone) rootBoneTransform = rootBone.transform;

            for (var i = 0; i < solverPosesProp.arraySize; ++i)
            {
                var poseProp = solverPosesProp.GetArrayElementAtIndex(i);
                var poseBoneProp = poseProp.FindPropertyRelative("m_BoneTransform");

                var boneTransform = poseBoneProp.objectReferenceValue as Transform;
                var bone = boneTransform.GetComponent<Bone2D>();

                if (bone) bone.attachedIK = null;
            }

            rootBoneTransformProp.objectReferenceValue = rootBoneTransform;
            solverPosesProp.arraySize = 0;

            if (rootBone)
            {
                solverPosesProp.arraySize = numBonesProp.intValue;

                var bone = rootBone;

                for (var i = 0; i < numBonesProp.intValue; ++i)
                {
                    var poseProp = solverPosesProp.GetArrayElementAtIndex(i);
                    var poseBoneTransformProp = poseProp.FindPropertyRelative("m_BoneTransform");
                    var localRotationProp = poseProp.FindPropertyRelative("defaultLocalRotation");
                    var solverPositionProp = poseProp.FindPropertyRelative("solverPosition");
                    var solverRotationProp = poseProp.FindPropertyRelative("solverRotation");

                    if (bone)
                    {
                        poseBoneTransformProp.objectReferenceValue = bone.transform;
                        localRotationProp.quaternionValue = bone.transform.localRotation;
                        solverPositionProp.vector3Value = Vector3.zero;
                        solverRotationProp.quaternionValue = Quaternion.identity;

                        bone = bone.child;
                    }
                }
            }
        }

        public static List<Ik2D> BuildIkList(Ik2D ik2D)
        {
            if (ik2D.target) return BuildIkList(ik2D.target);

            return new List<Ik2D>();
        }

        private static List<Ik2D> BuildIkList(Bone2D bone)
        {
            return BuildIkList(bone.chainRoot.gameObject);
        }

        private static List<Ik2D> BuildIkList(GameObject gameObject)
        {
            return gameObject.GetComponentsInChildren<Bone2D>().Select(b => b.attachedIK).Distinct().ToList();
        }

        public static void UpdateAttachedIKs(List<Ik2D> Ik2Ds)
        {
            for (var i = 0; i < Ik2Ds.Count; i++)
            {
                var ik2D = Ik2Ds[i];

                if (ik2D)
                    for (var j = 0; j < ik2D.solver.solverPoses.Count; j++)
                    {
                        var pose = ik2D.solver.solverPoses[j];

                        if (pose.bone) pose.bone.attachedIK = ik2D;
                    }
            }
        }

        public static List<Ik2D> UpdateIK(GameObject gameObject, string undoName, bool recordObject)
        {
            return UpdateIK(gameObject, undoName, recordObject, false);
        }

        public static List<Ik2D> UpdateIK(GameObject gameObject, string undoName, bool recordObject,
            bool updateAttachedIK)
        {
            if (updateAttachedIK)
            {
                var ik2Ds = new List<Ik2D>();
                gameObject.GetComponentsInChildren(ik2Ds);

                UpdateAttachedIKs(ik2Ds);
            }

            var list = BuildIkList(gameObject);

            UpdateIkList(list, undoName, recordObject);

            return list;
        }

        public static List<Ik2D> UpdateIK(Ik2D ik2D, string undoName, bool recordObject)
        {
            if (ik2D && ik2D.target) return UpdateIK(ik2D.target.chainRoot, undoName, recordObject);
            return null;
        }

        public static List<Ik2D> UpdateIK(Bone2D bone, string undoName, bool recordObject)
        {
            var list = BuildIkList(bone.chainRoot.gameObject);

            UpdateIkList(list, undoName, recordObject);

            return list;
        }

        private static void UpdateIkList(List<Ik2D> ikList, string undoName, bool recordObject)
        {
            for (var i = 0; i < ikList.Count; i++)
            {
                var l_ik2D = ikList[i];

                if (l_ik2D && l_ik2D.isActiveAndEnabled)
                {
                    if (!string.IsNullOrEmpty(undoName))
                        for (var j = 0; j < l_ik2D.solver.solverPoses.Count; j++)
                        {
                            var pose = l_ik2D.solver.solverPoses[j];
                            if (pose.bone)
                            {
                                if (recordObject)
                                    Undo.RecordObject(pose.bone.transform, undoName);
                                else
                                    Undo.RegisterCompleteObjectUndo(pose.bone.transform, undoName);
                            }
                        }

                    if (!string.IsNullOrEmpty(undoName) &&
                        l_ik2D.orientChild &&
                        l_ik2D.target &&
                        l_ik2D.target.child)
                    {
                        if (recordObject)
                            Undo.RecordObject(l_ik2D.target.child.transform, undoName);
                        else
                            Undo.RegisterCompleteObjectUndo(l_ik2D.target.child.transform, undoName);
                    }

                    l_ik2D.UpdateIK();

                    for (var j = 0; j < l_ik2D.solver.solverPoses.Count; j++)
                    {
                        var pose = l_ik2D.solver.solverPoses[j];
                        if (pose.bone) BoneUtils.FixLocalEulerHint(pose.bone.transform);

                        if (l_ik2D.orientChild && l_ik2D.target.child)
                            BoneUtils.FixLocalEulerHint(l_ik2D.target.child.transform);
                    }
                }
            }
        }
    }
}