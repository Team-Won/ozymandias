using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

[CustomEditor(typeof(DrawGrassInstanced))]
public class DrawGrassIntanceEditor : Editor
{

    DrawGrassInstanced grass;
    SerializedProperty scaleRandomRangeProp;

    private void OnEnable()
    {
        grass = (DrawGrassInstanced)target;
        scaleRandomRangeProp = serializedObject.FindProperty("scaleRandomRange");
    }

    public override void OnInspectorGUI()
    {
        base.OnInspectorGUI();

        GUILayout.Label("Paint Mode");
        grass.CurrentPaintMode = (DrawGrassInstanced.GrassPaintMode)GUILayout.Toolbar((int)grass.CurrentPaintMode, new string[] { "None", "Paint", "Remove" });

        if (GUILayout.Button("Clear All"))
        {
            grass.ClearPositions();
        }

        GUILayout.Label($"Grass Count: {grass.GrassCount}");
        if(GUILayout.Button("Garbage Collect"))
        {
            EditorUtility.UnloadUnusedAssetsImmediate();
            System.GC.WaitForPendingFinalizers();
            System.GC.Collect();
        }
    }


    private void OnSceneGUI()
    {

    }
}
