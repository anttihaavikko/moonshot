using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

namespace Anima2D
{
    public static class PoseUtils
    {
        public static void SavePose(Pose pose, Transform root)
        {
            var bones = new List<Bone2D>(50);

            root.GetComponentsInChildren(true, bones);

            var poseSO = new SerializedObject(pose);
            var entriesProp = poseSO.FindProperty("m_PoseEntries");

            poseSO.Update();
            entriesProp.arraySize = bones.Count;

            for (var i = 0; i < bones.Count; i++)
            {
                var bone = bones[i];

                if (bone)
                {
                    var element = entriesProp.GetArrayElementAtIndex(i);
                    element.FindPropertyRelative("path").stringValue = BoneUtils.GetBonePath(root, bone);
                    element.FindPropertyRelative("localPosition").vector3Value = bone.transform.localPosition;
                    element.FindPropertyRelative("localRotation").quaternionValue = bone.transform.localRotation;
                    element.FindPropertyRelative("localScale").vector3Value = bone.transform.localScale;
                }
            }

            poseSO.ApplyModifiedProperties();
        }

        public static void LoadPose(Pose pose, Transform root)
        {
            var poseSO = new SerializedObject(pose);
            var entriesProp = poseSO.FindProperty("m_PoseEntries");

            var iks = new List<Ik2D>();

            for (var i = 0; i < entriesProp.arraySize; i++)
            {
                var element = entriesProp.GetArrayElementAtIndex(i);

                var boneTransform = root.Find(element.FindPropertyRelative("path").stringValue);

                if (boneTransform)
                {
                    var boneComponent = boneTransform.GetComponent<Bone2D>();

                    if (boneComponent && boneComponent.attachedIK && !iks.Contains(boneComponent.attachedIK))
                        iks.Add(boneComponent.attachedIK);

                    Undo.RecordObject(boneTransform, "Load Pose");

                    boneTransform.localPosition = element.FindPropertyRelative("localPosition").vector3Value;
                    boneTransform.localRotation = element.FindPropertyRelative("localRotation").quaternionValue;
                    boneTransform.localScale = element.FindPropertyRelative("localScale").vector3Value;
                    BoneUtils.FixLocalEulerHint(boneTransform);
                }
            }

            for (var i = 0; i < iks.Count; i++)
            {
                var ik = iks[i];

                if (ik && ik.target)
                {
                    Undo.RecordObject(ik.transform, "Load Pose");

                    ik.transform.position = ik.target.endPosition;

                    if (ik.orientChild && ik.target.child)
                    {
                        ik.transform.rotation = ik.target.child.transform.rotation;
                        BoneUtils.FixLocalEulerHint(ik.transform);
                    }
                }
            }

            EditorUpdater.SetDirty("Load Pose");
        }
    }
}