using System.Reflection;
using UnityEditor;

namespace Anima2D
{
    [InitializeOnLoad]
    public class ToolsExtra
    {
        private static readonly PropertyInfo s_ViewToolActivePropertyInfo;

        static ToolsExtra()
        {
            s_ViewToolActivePropertyInfo = typeof(Tools).GetProperty("viewToolActive",
                BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance | BindingFlags.Static);
        }

        public static bool viewToolActive => (bool) s_ViewToolActivePropertyInfo.GetValue(null, null);
    }
}