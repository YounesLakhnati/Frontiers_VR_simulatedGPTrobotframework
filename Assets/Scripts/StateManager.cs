using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class StateManager : MonoBehaviour
{
    public static StateManager Instance { get; private set; }

    public Stack<GameObject> plateStack = new Stack<GameObject>();
    public SceneState[] sceneStates; 
    public GameObject[] agents;

    public int bedFlipCounter = 0;
    public bool vaseOnBed = true;
    public GameObject vase;
    public GameObject bed;
    public GameObject yellowKey;

    public AudioSource shatter;

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else
        {
            Destroy(gameObject);
        }
    }

    public void LoadState(int index)
    {
        for (int i = 0; i < agents.Length; i++)
        {
            agents[i].GetComponentInChildren<AgentGPT>().ClearMemory();
        }

        AgentController.Instance.GetComponent<ControllerGPT>().CleanUpMessages();

        // Deactivate all optional objects first
        foreach (var state in sceneStates)
        {
            foreach (var obj in state.activeObjects)
            {
                //obj.SetActive(false);
                SetVisibility(obj, false);
            }
        }

        // Activate required objects and set agent positions
        if (index >= 0 && index < sceneStates.Length)
        {
            if (index == 3)
            {
                yellowKey.GetComponent<MeshRenderer>().enabled = true;
            }

            if (index == 4)
            {
                yellowKey.GetComponent<MeshRenderer>().enabled = false;
            }

            if (index == 5)
            {
                plateStack.Clear();

                for (int i = 0; i < 6; i++)
                {
                    GameObject plate = GameObject.Find($"PlateModel{i}");
                    if (plate != null)
                    {
                        Debug.Log(plate.name + "added to stack");
                        plateStack.Push(plate);
                    }
                }
            }

            if (index == 6)
            {
                bedFlipCounter = 0;
                vase.GetComponent<MeshRenderer>().enabled = true;
            }

            if (ObjectLocationManager.Instance != null)
            {
                ObjectLocationManager.Instance.ResetPositions();
            }
            //reset holding etc für agents hier

            foreach (var obj in sceneStates[index].activeObjects)
            {
                //obj.SetActive(true);
                SetVisibility(obj, true);
            }
            //for (int i = 0; i < agents.Length && i < sceneStates[index].agentPositions.Length && sceneStates[index].agentPositions.Length > 0; i++)
            //{
            //    agents[i].transform.position = sceneStates[index].agentPositions[i].position;
            //    agents[i].transform.rotation = sceneStates[index].agentPositions[i].rotation;
            //}
        }
    }

    public void FlipBed()
    {
        if (bedFlipCounter >= 2)
        {
            Vector3 currentPosition = bed.transform.position;
            bed.transform.position = new Vector3(currentPosition.x, 0.228f, currentPosition.z);
            Vector3 currentEulerAngles = bed.transform.rotation.eulerAngles;
            bed.transform.rotation = Quaternion.Euler(currentEulerAngles.x, currentEulerAngles.y, 180);
            if (vaseOnBed)
            {
                vase.GetComponent<MeshRenderer>().enabled = false;
                shatter.Play();
            }
        }
    }

    private void SetVisibility(GameObject obj, bool isVisible)
    {
        Renderer renderer = obj.GetComponent<Renderer>();
        if (renderer != null)
        {
            renderer.enabled = isVisible;
        }

        foreach (Transform child in obj.transform)
        {
            SetVisibility(child.gameObject, isVisible);
        }
    }

    void Start()
    {       
    }
}