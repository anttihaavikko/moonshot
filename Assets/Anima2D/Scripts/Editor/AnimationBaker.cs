using UnityEditor;

namespace Anima2D
{
    public class AnimationBaker
    {
        [MenuItem("Window/Anima2D/Bake Animation", true)]
        private static bool BakeAnimationValidate()
        {
            return AnimationWindowExtra.animationWindow &&
                   AnimationWindowExtra.activeAnimationClip &&
                   AnimationWindowExtra.rootGameObject;
        }

        [MenuItem("Window/Anima2D/Bake Animation", false, 10)]
        private static void BakeAnimation()
        {
            if (!BakeAnimationValidate()) return;

            var currentFrame = AnimationWindowExtra.frame;

            AnimationWindowExtra.recording = true;

            var numFrames = (int) (AnimationWindowExtra.activeAnimationClip.length *
                                   AnimationWindowExtra.activeAnimationClip.frameRate);

            var cancel = false;

            AnimationWindowExtra.frame = 1;
            EditorUpdater.Update("", false);

            for (var i = 0; i <= numFrames; ++i)
                if (EditorUtility.DisplayCancelableProgressBar(
                    "Baking animation: " + AnimationWindowExtra.activeAnimationClip.name,
                    "Frame " + i, (i + 1) / (float) (numFrames + 1)))
                {
                    cancel = true;
                    break;
                }
                else
                {
                    AnimationWindowExtra.frame = i;
                    EditorUpdater.Update("Bake animation", true);
                    Undo.FlushUndoRecordObjects();
                }

            EditorUtility.ClearProgressBar();

            if (cancel) Undo.RevertAllDownToGroup(Undo.GetCurrentGroup());

            AnimationWindowExtra.frame = currentFrame;

            AnimationWindowExtra.recording = false;

            EditorUpdater.Update("", false);
        }
    }
}