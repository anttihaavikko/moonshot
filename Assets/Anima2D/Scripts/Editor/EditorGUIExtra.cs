using System.Reflection;
using UnityEditor;
using UnityEngine;

namespace Anima2D
{
    public class EditorGUIExtra
    {
        public static int s_MinMaxSliderHash = "MinMaxSlider".GetHashCode();

        public static BoneWeight Weight(BoneWeight boneWeight, int weightIndex, string[] names,
            bool mixedBoneIndex = false, bool mixedWeight = false)
        {
            var boneIndex = 0;
            var weight = 0f;

            boneWeight.GetWeight(weightIndex, out boneIndex, out weight);

            EditorGUIUtility.labelWidth = 30f;

            EditorGUILayout.BeginHorizontal();

            EditorGUI.BeginChangeCheck();

            EditorGUI.showMixedValue = mixedBoneIndex;
            var newBoneIndex = EditorGUILayout.Popup(boneIndex + 1, names, GUILayout.Width(100f)) - 1;

            EditorGUI.BeginDisabledGroup(newBoneIndex == -1);

            EditorGUI.showMixedValue = mixedWeight;
            weight = EditorGUILayout.Slider(weight, 0f, 1f);

            EditorGUI.EndDisabledGroup();

            EditorGUILayout.EndHorizontal();

            if (EditorGUI.EndChangeCheck())
            {
                if (newBoneIndex == -1) boneWeight.Unassign(boneIndex);
                boneWeight.SetWeight(weightIndex, newBoneIndex, weight);
            }

            return boneWeight;
        }


        public static void SortingLayerField(GUIContent label, SerializedProperty layerID, GUIStyle style,
            GUIStyle labelStyle)
        {
            var methodInfo = typeof(EditorGUILayout).GetMethod("SortingLayerField",
                BindingFlags.Static | BindingFlags.NonPublic, null,
                new[] {typeof(GUIContent), typeof(SerializedProperty), typeof(GUIStyle), typeof(GUIStyle)}, null);

            if (methodInfo != null)
            {
                object[] parameters = {label, layerID, style, labelStyle};
                methodInfo.Invoke(null, parameters);
            }
        }

        public static void MinMaxScroller(Rect position, int id, ref float value, ref float size, float visualStart,
            float visualEnd, float startLimit, float endLimit, GUIStyle slider, GUIStyle thumb, GUIStyle leftButton,
            GUIStyle rightButton, bool horiz)
        {
            var editorGUIExtType = typeof(EditorWindow).Assembly.GetType("UnityEditor.EditorGUIExt");

            var minMaxScrollerMethod =
                editorGUIExtType.GetMethod("MinMaxScroller", BindingFlags.Static | BindingFlags.Public);

            if (minMaxScrollerMethod != null)
            {
                object[] parameters =
                {
                    position, id, value, size, visualStart, visualEnd, startLimit, endLimit, slider, thumb, leftButton,
                    rightButton, horiz
                };
                minMaxScrollerMethod.Invoke(null, parameters);

                value = (float) parameters[2];
                size = (float) parameters[3];
            }
        }

        public static GUIContent TempContent(string t)
        {
            var editorGUIUtilityType = typeof(EditorGUIUtility);

            var tempContentMethod = editorGUIUtilityType.GetMethod("TempContent",
                BindingFlags.Static | BindingFlags.Public | BindingFlags.NonPublic, null, new[] {typeof(string)}, null);

            if (tempContentMethod != null)
            {
                object[] parameters = {t};
                return (GUIContent) tempContentMethod.Invoke(null, parameters);
            }

            return null;
        }

        public static int GetPermanentControlID()
        {
            var guiUtilityType = typeof(GUIUtility);

            var getPermanentControlIDMethod = guiUtilityType.GetMethod("GetPermanentControlID",
                BindingFlags.Static | BindingFlags.Public | BindingFlags.NonPublic);

            if (getPermanentControlIDMethod != null) return (int) getPermanentControlIDMethod.Invoke(null, null);

            return 0;
        }
    }
}