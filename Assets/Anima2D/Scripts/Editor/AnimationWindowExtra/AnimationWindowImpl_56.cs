using System.Reflection;

namespace Anima2D
{
    public class AnimationWindowImpl_56 : AnimationWindowImpl_51_52_53_54_55
    {
        private PropertyInfo m_CurrentFrameProperty;
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
                        m_StartRecording.Invoke(null, null);
                }
                else
                {
                    if (m_StopRecording != null)
                        m_StopRecording.Invoke(null, null);
                }
            }
        }

        public override int frame
        {
            get
            {
                if (state != null && m_CurrentFrameProperty != null)
                    return (int) m_CurrentFrameProperty.GetValue(state, null);

                return 0;
            }

            set
            {
                if (state != null && m_CurrentFrameProperty != null)
                    m_CurrentFrameProperty.SetValue(state, value, null);
            }
        }


        public override void InitializeReflection()
        {
            base.InitializeReflection();

            m_CurrentFrameProperty = m_AnimationWindowStateType.GetProperty("currentFrame",
                BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance | BindingFlags.Static);
            m_StartRecording =
                m_AnimationWindowStateType.GetMethod("StartRecording", BindingFlags.Public | BindingFlags.Static);
            m_StopRecording =
                m_AnimationWindowStateType.GetMethod("StopRecording", BindingFlags.Public | BindingFlags.Static);
        }
    }
}