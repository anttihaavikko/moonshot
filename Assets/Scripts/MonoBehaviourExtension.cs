using System;
using System.Collections;

namespace UnityEngine
{
    public static class MonoBehaviourExtension
    {
        public static Coroutine StartCoroutine(this MonoBehaviour behaviour, Action action, float delay)
        {
            return behaviour.StartCoroutine(WaitAndDo(delay, action));
        }

        private static IEnumerator WaitAndDo(float time, Action action)
        {
            yield return new WaitForSeconds(time);
            action();
        }
    }
}