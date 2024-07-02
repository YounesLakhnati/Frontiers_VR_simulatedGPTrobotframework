using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

[CustomEditor(typeof(StateManager))]
public class StateManagerEditor : Editor
{
    public override void OnInspectorGUI()
    {
        StateManager stateManager = (StateManager)target;

        // Draw default inspector properties
        DrawDefaultInspector();

        GUILayout.Space(10);

        // Create buttons for each state
        for (int i = 0; i < stateManager.sceneStates.Length; i++)
        {
            if (GUILayout.Button("Load State " + i))
            {
                stateManager.LoadState(i);
            }
        }
    }
}