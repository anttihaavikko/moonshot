using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

namespace Anima2D
{
    public class MaskCreator
    {
        [MenuItem("Window/Anima2D/Create Mask", true)]
        private static bool CreateMaskValidate()
        {
            return Selection.activeGameObject && Selection.activeGameObject.GetComponent<Animator>();
        }

        [MenuItem("Window/Anima2D/Create Mask", false, 20)]
        private static void CreateMask()
        {
            Animator animator;

            if (!Selection.activeGameObject) return;

            animator = Selection.activeGameObject.GetComponent<Animator>();

            if (!animator) return;

            var transforms = new List<Transform>();

            var avatarMask = new AvatarMask();

            animator.GetComponentsInChildren(true, transforms);

            avatarMask.transformCount = transforms.Count;

            var index = 0;

            foreach (var transform in transforms)
            {
                avatarMask.SetTransformPath(index,
                    AnimationUtility.CalculateTransformPath(transform, animator.transform));
                avatarMask.SetTransformActive(index, true);
                index++;
            }

            ScriptableObjectUtility.CreateAssetWithSavePanel(avatarMask, "Create Mask", animator.name + ".mask", "mask",
                "Create a new Avatar Mask");
        }
    }
}