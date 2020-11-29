using System;
using System.Reflection;
using UnityEditor;
using UnityEngine;

namespace Anima2D
{
    [InitializeOnLoad]
    public class CurveUtility
    {
        private static readonly Type m_CurveUtilityType =
            typeof(EditorWindow).Assembly.GetType("UnityEditor.CurveUtility");

        private static MethodInfo s_HaveKeysInRangeMethod;
        private static MethodInfo s_RemoveKeysInRangeMethod;
        private static MethodInfo s_UpdateTangentsFromModeMethod1;
        private static MethodInfo s_UpdateTangentsFromModeMethod2;
        private static MethodInfo s_CalculateLinearTangentMethod;
        private static MethodInfo s_UpdateTangentsFromModeSurroundingMethod;
        private static MethodInfo s_CalculateSmoothTangentMethod;
        private static MethodInfo s_SetKeyBrokenMethod;
        private static MethodInfo s_GetKeyBrokenMethod;

        static CurveUtility()
        {
            InitializeReflection();
        }

        private static void InitializeReflection()
        {
            Type[] s_UpdateTangentsFromModeMethod1Types = {typeof(AnimationCurve)};
            Type[] s_UpdateTangentsFromModeMethod2Types = {typeof(AnimationCurve), typeof(int)};

            s_HaveKeysInRangeMethod = m_CurveUtilityType.GetMethod("HaveKeysInRange",
                BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance | BindingFlags.Static);
            s_RemoveKeysInRangeMethod = m_CurveUtilityType.GetMethod("RemoveKeysInRange",
                BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance | BindingFlags.Static);
            s_UpdateTangentsFromModeMethod1 = m_CurveUtilityType.GetMethod("UpdateTangentsFromModeMethod",
                BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance | BindingFlags.Static, null,
                s_UpdateTangentsFromModeMethod1Types, null);
            s_UpdateTangentsFromModeMethod2 = m_CurveUtilityType.GetMethod("UpdateTangentsFromModeMethod",
                BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance | BindingFlags.Static, null,
                s_UpdateTangentsFromModeMethod2Types, null);
            s_CalculateLinearTangentMethod = m_CurveUtilityType.GetMethod("CalculateLinearTangent",
                BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance | BindingFlags.Static);
            s_UpdateTangentsFromModeSurroundingMethod = m_CurveUtilityType.GetMethod(
                "UpdateTangentsFromModeSurrounding",
                BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance | BindingFlags.Static);
            s_CalculateSmoothTangentMethod = m_CurveUtilityType.GetMethod("CalculateSmoothTangent",
                BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance | BindingFlags.Static);
            s_SetKeyBrokenMethod = m_CurveUtilityType.GetMethod("SetKeyBroken",
                BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance | BindingFlags.Static);
            s_GetKeyBrokenMethod = m_CurveUtilityType.GetMethod("GetKeyBroken",
                BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance | BindingFlags.Static);
        }

        public static bool HaveKeysInRange(AnimationCurve curve, float beginTime, float endTime)
        {
            if (s_HaveKeysInRangeMethod != null)
            {
                object[] parameters = {curve, beginTime, endTime};
                return (bool) s_HaveKeysInRangeMethod.Invoke(null, parameters);
            }

            return false;
        }

        public static void RemoveKeysInRange(AnimationCurve curve, float beginTime, float endTime)
        {
            if (s_RemoveKeysInRangeMethod != null)
            {
                object[] parameters = {curve, beginTime, endTime};
                s_RemoveKeysInRangeMethod.Invoke(null, parameters);
            }
        }

        public static void UpdateTangentsFromMode(AnimationCurve curve)
        {
            if (s_UpdateTangentsFromModeMethod1 != null)
            {
                object[] parameters = {curve};
                s_UpdateTangentsFromModeMethod1.Invoke(null, parameters);
            }
        }

        public static float CalculateLinearTangent(AnimationCurve curve, int index, int toIndex)
        {
            if (s_CalculateLinearTangentMethod != null)
            {
                object[] parameters = {curve, index, toIndex};
                return (float) s_CalculateLinearTangentMethod.Invoke(null, parameters);
            }

            return 0f;
        }

        public static void UpdateTangentsFromMode(AnimationCurve curve, int index)
        {
            if (s_UpdateTangentsFromModeMethod2 != null)
            {
                object[] parameters = {curve};
                s_UpdateTangentsFromModeMethod2.Invoke(null, parameters);
            }
        }

        public static void UpdateTangentsFromModeSurrounding(AnimationCurve curve, int index)
        {
            if (s_UpdateTangentsFromModeSurroundingMethod != null)
            {
                object[] parameters = {curve, index};
                s_UpdateTangentsFromModeSurroundingMethod.Invoke(null, parameters);
            }
        }

        public static float CalculateSmoothTangent(Keyframe key)
        {
            if (s_CalculateSmoothTangentMethod != null)
            {
                object[] parameters = {key};
                return (float) s_CalculateSmoothTangentMethod.Invoke(null, parameters);
            }

            return 0f;
        }

        public static void SetKeyBroken(ref Keyframe key, bool broken)
        {
            if (s_SetKeyBrokenMethod != null)
            {
                object[] parameters = {key, broken};
                s_SetKeyBrokenMethod.Invoke(null, parameters);
            }
        }

        public static bool GetKeyBroken(Keyframe key)
        {
            if (s_GetKeyBrokenMethod != null)
            {
                object[] parameters = {key};
                return (bool) s_GetKeyBrokenMethod.Invoke(null, parameters);
            }

            return false;
        }
    }
}