using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class SceneState
{
    public GameObject[] activeObjects; // Objects that should be active in this state
    public GameObject[] objectPositions;
    public Transform[] agentPositions; // Target positions for agents
}