using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

[CustomEditor(typeof(GridWorldManager))]
public class EditorGUI : Editor
{
    public int column;
    public override void OnInspectorGUI()
    {
        DrawDefaultInspector();
        GridWorldManager manager = (GridWorldManager) target;
        //column = manager.column;
        if(GUILayout.Button("set up grid"))
            manager.SetUpGrid();
        
        if(GUILayout.Button("policy"))
            manager.PolicyEvaluation();
        
        if(GUILayout.Button("value"))
            manager.ValueIteration();
    }
}
