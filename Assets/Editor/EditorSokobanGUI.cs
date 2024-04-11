using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;
[CustomEditor(typeof(SokobanManager))]
public class EditorSokobanGUI : Editor
{
    public override void OnInspectorGUI()
    {
        DrawDefaultInspector();
        SokobanManager manager = (SokobanManager) target;
        //column = manager.column;
        if(GUILayout.Button("set up grid"))
            manager.InitSokoban();
        
        if(GUILayout.Button("policy"))
            manager.PolicyEvaluation();
        
        if(GUILayout.Button("value"))
            manager.ValueIteration();
    }
}
