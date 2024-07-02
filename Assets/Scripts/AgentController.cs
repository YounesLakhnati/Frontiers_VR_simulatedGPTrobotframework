using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using OpenAI;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.XR.Interaction.Toolkit;

public class AgentController : MonoBehaviour
{
    public static AgentController Instance { get; private set; }

    [SerializeField] public ControllerGPT ControllerGPT;
    [SerializeField] private AgentGPT Pluto;
    [SerializeField] private AgentGPT Neptun;
    [SerializeField] private AgentGPT Jupiter;
    [SerializeField] private XRRayInteractor rayInteractor;

    private RaycastHit pointer;
    private Dictionary<string, AgentGPT> agents = new Dictionary<string, AgentGPT>();

    //Queue<string> requestQueue = new Queue<string>(); //VERSION 1
    Queue<Tuple<string, string>> requestQueue = new Queue<Tuple<string, string>>(); //VERSION 2

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

    // Start is called before the first frame update
    void Start()
    { 
        agents.Add("Pluto", Pluto);
        agents.Add("Neptun", Neptun);
        agents.Add("Jupiter", Jupiter);

    }

    // Update is called once per frame
    void Update()
    {
        //Debug.Log(messages.Last().Content);
    }

    public void EnqueueAgentRequests(string recipients)
    {
        try
        {
            HashSet<string> keywords = new HashSet<string> { "Pluto", "Neptun", "Jupiter" };

            var recipientsData = JsonConvert.DeserializeObject<Dictionary<string, List<RecipientInstructions>>>(recipients);

            foreach (RecipientInstructions recipient in recipientsData["recipient"])
            {
                if (keywords.Contains(recipient.name))
                {
                    agents[recipient.name].SetLabelActive();
                    Debug.Log("agent enqueued:"  + agents[recipient.name]);
                    requestQueue.Enqueue(Tuple.Create(recipient.name, recipient.instructions));
                }
            }
            StartCoroutine(ProcessAgentRequests());
        }
        catch (ArgumentNullException e)
        {
            Debug.Log(e.Message);
        }
    }


    private IEnumerator ProcessAgentRequests()
    {
        while (requestQueue.Count > 0)
        {
            var agentData = requestQueue.Dequeue();
            string agentName = agentData.Item1;
            string instructions = agentData.Item2;
            DistributeCommand(instructions, agents[agentName]);
            yield return StartCoroutine(waitForAgent(agents[agentName]));
        }
    }

    private void DistributeCommand(string message, AgentGPT agent)
    {
        agent.SendReply(message);
    }

    IEnumerator waitForAgent(AgentGPT agent)
    {
        yield return new WaitForSeconds(1.0f);
        agent.SetLabelIdle();
    }

    public void SetPointPosition()
    {
        RaycastHit hit;

        rayInteractor.TryGetCurrent3DRaycastHit(out hit);
        // Use the hit.point where the ray intersected with an object
        //if (hit.collider.gameObject.tag == "Ground")
        //{
        //    Debug.Log("hitting ground");
        //}
        //Debug.Log($"Ray hit at position: {hit.point}");
        pointer = hit;
    }

    public RaycastHit GetPointPosition()
    {
        return pointer;
    }

    //this class could have a parser for a string from the WHISPER transcription to find out what agent is being adressed + context information which agent was last etc
}
