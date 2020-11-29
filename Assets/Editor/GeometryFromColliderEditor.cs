using UnityEditor;
using UnityEngine;

[CustomEditor(typeof(GeometryFromColliderScript))]
public class GeometryFromColliderEditor : Editor
{
    public override void OnInspectorGUI()
    {
        DrawDefaultInspector();

        var myScript = (GeometryFromColliderScript) target;

        if (GUILayout.Button("Generate Geometry")) myScript.GenerateGeometry();
    }
}