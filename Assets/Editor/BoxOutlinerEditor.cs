using UnityEditor;
using UnityEngine;

[CustomEditor(typeof(BoxOutlinerScript))]
public class BoxOutlinerEditor : Editor
{
    public override void OnInspectorGUI()
    {
        DrawDefaultInspector();

        var myScript = (BoxOutlinerScript) target;
        if (GUILayout.Button("Fix outline")) myScript.DoOutline();
    }
}