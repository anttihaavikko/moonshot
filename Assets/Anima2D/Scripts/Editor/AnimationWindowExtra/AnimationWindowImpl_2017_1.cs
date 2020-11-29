using System.Reflection;

namespace Anima2D
{
    public class AnimationWindowImpl_2017_1 : AnimationWindowImpl_56
    {
        private MethodInfo m_StartRecording;
        private MethodInfo m_StopRecording;

        public override bool recording
        {
            get => base.recording;

            set
            {
                if (value)
                {
                    if (m_StartRecording != null)
                        m_StartRecording.Invoke(state, null);
                }
                else
                {
                    if (m_StopRecording != null)
                        m_StopRecording.Invoke(state, null);
                }
            }
        }


        public override void InitializeReflection()
        {
            base.InitializeReflection();

            m_StartRecording =
                m_AnimationWindowStateType.GetMethod("StartRecording", BindingFlags.Public | BindingFlags.Instance);
            m_StopRecording =
                m_AnimationWindowStateType.GetMethod("StopRecording", BindingFlags.Public | BindingFlags.Instance);
        }
    }
}