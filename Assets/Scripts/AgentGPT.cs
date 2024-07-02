using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using OpenAI;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;
using TMPro;
using UnityEngine;
using UnityEngine.AI;

public class AgentGPT : ChatGPT
{
    public Transform attachPoint;
    public string holding;
    public GameObject heldItem;
    public TextMeshPro nameText;
    public GameObject loadingCircle; //purely visual
    public float grabRange = 2.8f;
    public bool isActing;
    public bool disabled = false;
    [SerializeField] protected NavMeshAgent navMeshAgent;
    [SerializeField] protected Role role;
    [SerializeField] protected TextToSpeech textToSpeech;
    [SerializeField] protected Log log;
    protected Transform textMeshTransform;

    [SerializeField] protected NavMeshObstacle glassDoor;
    public AudioSource binSound;

    // functions that are shared among all agents
    protected List<Functions> function_descriptions; 

    public virtual void Start()
    {
        textMeshTransform = nameText.transform;
        holding = "nothing";

        function_descriptions = new List<Functions>()
        {
            new Functions(){
                Name = "move_to",
                Description = "Moves to a location. Only the destinations listed in the destination property enum are valid, for other destinations this function must not be called.",
                Parameters = new Parameters()
                {
                    Type = "object",
                    Properties = new Dictionary<string, Property>()
                    {
                        { "destination", new Property()
                            {
                                Type = "string",
                                Description = "The name of an object or location to to move to, e.g. Kühlschrank",
                                Enum = ObjectLocationManager.Instance.GetLocations()
                            }
                        }
                    },
                    Required = new List<string>()
                    {
                        "destination"
                    }
                }
            },
            new Functions()
            {
                Name = "pick_up",
                Description = "Picks up an item. Only the items listed in the item property enum are valid, for other items this function must not be called.",
                Parameters = new Parameters()
                {
                    Type = "object",
                    Properties = new Dictionary<string, Property>()
                    {
                        { "item", new Property()
                            {
                                Type = "string",
                                Description = "The name of an object to pick up and carry with you, e.g. Kerze",
                                Enum = ObjectLocationManager.Instance.GetGrabbables()
                            }
                        }
                    },
                    Required = new List<string>()
                    {
                        "item"
                    }
                }
            },
            /*new Functions()
            {
                Name = "give_item_to_other_agent",
                Description = "Gives the item currently held by the agent to another agent. Only call this when you are asked to give an item you are holding to another agent that isn't you.",
                Parameters = new Parameters()
                {
                    Type = "object",
                    Properties = new Dictionary<string, Property>()
                    {
                        { "agent", new Property()
                            {
                                Type = "string",
                                Description = "The name of the agent the currenlty held item should be given to, e.g. Jupiter",
                                Enum = new List<string>()
                                {
                                    "Pluto", "Neptune", "Jupiter"
                                }
                            }
                        }
                    },
                    Required = new List<string>()
                    {
                        "agent"
                    }
                }
            },*/
            new Functions()
            {
                Name = "place_on",
                Description = "Places currently held object onto the designated surface. Only the surfaces listed in the surface property enum are valid, for other surfaces this function must not be called.",
                Parameters = new Parameters()
                {
                    Type = "object",
                    Properties = new Dictionary<string, Property>()
                    {
                        { "surface", new Property()
                            {
                                Type = "string",
                                Description = "The name of the surface to place currently held object onto, e.g. Bett",
                                Enum = new List<string>()
                                {
                                    "Bett", "Boden"
                                }
                            }
                        }
                    },
                    Required = new List<string>()
                    {
                        "surface"
                    }
                }
            },
            new Functions()
            {
                Name = "open_lock",
                Description = "Attempts to open a lock on a door or chest with a key the agent might or might not be holding.",
                Parameters = new Parameters()
                {
                    Type = "object",
                    Properties = new Dictionary<string, Property>()
                    {
                        { "locked_object", new Property()
                            {
                                Type = "string",
                                Description = "The name of the locked object the agent should attempt to open. For example, the blue key is for the blue door and the yellow key for the yellow chest.",
                                Enum = ObjectLocationManager.Instance.GetLockedObjects()
                            }
                        }
                    },
                    Required = new List<string>()
                    {
                        "locked_object"
                    }
                }
            },
            new Functions()
            {
                Name = "put_in_the_trash",
                Description = "Puts the currently held item into the trashcan.",
                Parameters = new Parameters()
                {
                    Type = "object",
                    Properties = new Dictionary<string, Property>()
                    {
                        { "item", new Property()
                            {
                                Type = "string",
                                Description = "The name of the item that should be put in the trash can.",
                                Enum = ObjectLocationManager.Instance.GetGrabbables()
                            }
                        }
                    },
                    Required = new List<string>()
                    {
                        "item"
                    }
                }
            },
            new Functions()
            {
                Name = "flip_bed",
                Description = "Flips the bed upside down.",
                Parameters = new Parameters()
                {
                    Type = "object",
                    Properties = new Dictionary<string, Property>()
                    {
                        { "bed", new Property()
                            {
                                Type = "string",
                                Description = "The bed that should be flipped.",
                            }
                        }
                    },
                    Required = new List<string>()
                    {
                        "bed"
                    }
                }
            },
            //new Functions()
            //{
            //    Name = "deliver_message_to_other_agent",
            //    Description = "Delivers a message to another agent. Should only be called if the user strictly told you to deliver a message to another agent or a function_call suggested cooperation.",
            //    Parameters = new Parameters()
            //    {
            //        Type = "object",
            //        Properties = new Dictionary<string, Property>()
            //        {
            //            { "agent", new Property()
            //                {
            //                    Type = "string",
            //                    Enum = new List<string>() { "Pluto", "Neptun", "Jupiter" }
            //                }
            //            },
            //            { "message", new Property()
            //                {
            //                    Type = "string",
            //                    Description = "The message to relay to the agent."
            //                }
            //            }
            //        }
            //    }
            //}
        };
    }

    // Update is called once per frame
    void Update()
    {
    }

    public async override void SendReply(string text)
    {
        if (!disabled) 
        {
            nameText.color = Color.green;

            if (messages.Count == 0)
            {
                var startingMessage = new ChatMessage()
                {
                    Role = "system",
                    Content = prompt,
                };
                messages.Add(startingMessage);
            }

            //toggle on/of for log
            var logMessage = new ChatMessage()
            {
                Role = "system",
                Content = log.GetLastSentencesUpToUserRequest(),
            };

            var newMessage = new ChatMessage()
            {
                Role = "user",
                Content = text,
            };
            messages.Add(newMessage);

            //try
            //{

            var completionResponse = await openai.CreateChatCompletion(new CreateChatCompletionRequest()
            {
                Model = "gpt-3.5-turbo-0613",
                //Model = "gpt-4-0613",
                Messages = messages,
                Functions = function_descriptions
            });


            if (completionResponse.Choices != null && completionResponse.Choices.Count > 0)
            {
                while (completionResponse.Choices[0].FinishReason == "function_call") //nullref if functioncall syntax error
                {
                    string functionName = completionResponse.Choices[0].Message.Function_call?.Name; //move_to
                    string functionArguments = completionResponse.Choices[0].Message.Function_call?.Arguments; //destination:candle

                    Debug.Log("first choice:" + completionResponse.Choices[0].Message.Function_call?.Name + completionResponse.Choices[0].Message.Function_call?.Arguments);

                    if (functionName != null && functionName.Contains('.'))  //prevent GPT from agent.functioncalls
                    {
                        functionName = functionName.Split('.').Last();
                    }

                    // reminds the model "This is the function I chose"
                    messages.Add(new ChatMessage()
                    {
                        Role = "function",
                        Name = functionName,
                        Content = functionArguments 
                    });

                    // system message giving textual feedback of chosen methods
                    messages.Add(new ChatMessage()
                    {
                            Role = "system",
                            Content = Validate(functionName, functionArguments)
                    });

                    // execute the chosen method
                    await ExecuteFunction(functionName, functionArguments);

                    // next step - continue functions or natural language response 
                    completionResponse = await openai.CreateChatCompletion(new CreateChatCompletionRequest()
                    {
                        //Model = "gpt-3.5-turbo-0613",
                        Model = "gpt-4-0613",
                        Messages = messages,
                        Functions = function_descriptions
                    });

                    //keep looping while the finish reason remains being "function_call"
                }

                // the agent has no more functions to call and responded in natural language
                if (completionResponse.Choices != null && completionResponse.Choices.Count > 0)
                {                  
                    loadingCircle.SetActive(false);

                    // extract the agent's concluding natural language response
                    var message = completionResponse.Choices[0].Message;

                    if (message.Content != null)
                    {
                        message.Content = message.Content.Trim();
                        messages.Add(message); //add to history
                        textToSpeech.MakeAudioRequest(message.Content); // text-to-speech
                        log.AddEntry(role, message.Content); // log
                    }
                    Debug.Log(message.Content);
                    Debug.Log("is processing:" + disabled);
                }
                else //no function was called, agent responds
                {
                    loadingCircle.SetActive(false);

                    var message = completionResponse.Choices[0].Message;
                    message.Content = message.Content.Trim();

                    Debug.Log(message.Content);

                    messages.Add(message);
                    textToSpeech.MakeAudioRequest(message.Content);
                    log.AddEntry(role, message.Content);
                    Debug.Log(log.GetLog());
                    Debug.Log("is processing:" + disabled);
                }
            }
            else
            {
                loadingCircle.SetActive(false);
                Debug.Log("No text was generated from this prompt.");
                //isProcessing = false;
            }
            /*}
            catch (Exception e)
            {
                Debug.Log(e.Message);

            }*/
        }
        else
        {
            //when agent is talked to while already processing
        }
    }

    protected virtual string Validate(string function, string parameters)
    {
        MethodInfo method = GetType().GetMethod(function, BindingFlags.Instance | BindingFlags.NonPublic | BindingFlags.Public);
        if (method != null)
        {
            JObject jsonObject = JObject.Parse(parameters);
            switch (function)
            {
                case "move_to":
                    string destination = (string)jsonObject["destination"];

                    if (ObjectLocationManager.Instance.GetLocations().Contains(destination))
                    {
                        if (CanMoveTo(ObjectLocationManager.Instance.GetObjectTransform(destination)))
                        {
                            return $"Success: Valid location, moved succesfully to {destination}.";
                        }
                        else
                        {
                            return "Error: Valid location but cannot reach it. Obstructed or not accessible.";
                        }
                    }
                    return "Error: Invalid location, function call unsuccessful. Location does not exist in the game world.";

                case "pick_up":
                    string item = (string)jsonObject["item"];
                    if (ObjectLocationManager.Instance.GetGrabbables().Contains(item))
                    {
                        if (holding == "nothing")
                        {
                            Debug.Log("distance to grabbable object" + Vector3.Distance(transform.position, GameObject.Find(item).transform.position));
                            Debug.Log("navMeshAgent remaining distance:" + navMeshAgent.remainingDistance + "nav MeshAgent dstopping distance" + navMeshAgent.stoppingDistance);
                            if(item == "Teller" && StateManager.Instance.plateStack.Count == 0)
                            {
                                return "Function call unsuccesful, the stack of plates it empty and there are no more plates to be picked up";
                            }
                            else if (Vector3.Distance(transform.position, GameObject.Find(item).transform.position) <= grabRange && item == "Teller")
                            {
                                Debug.Log($"attempting pick up of { item }");
                                return $"Picked up a singular { item }.";
                            }
                            else if (Vector3.Distance(transform.position, GameObject.Find(item).transform.position) <= grabRange)
                            {
                                Debug.Log($"attempting pick up of { item }");
                                return $"Function call pick_up succesful. Picked up { item }";
                            }
                            else
                            {
                                return $"Error: Not in range to pick up {item}.";
                            }
                        }
                        else if (item == holding)
                        {

                            Debug.Log("holding same item");
                            return "Error: Already holding the same object.";
                        }
                        else if (item != holding)
                        {
                            Debug.Log("swap attempt");
                            return "Error: Already holding another object."; 
                        }
                    }
                    else
                    {
                        return $"Error: {item} can not be picked up or doesn't exist in the current context.";
                    }
                    break;

                case "place_on":
                    string surface = (string)jsonObject["surface"];
                    if (holding == "nothing")
                    {
                        return "Error: Not holding any object. An object needs to be held before being able to place.";
                    }
                    else if (surface == "Bett")
                    {
                        return $"Function call place_on succesful. {holding} has been placed on the bed.";
                    }
                    else if (surface == "Boden")
                    {
                        return $"{holding} has been placed on the ground.";
                    }
                    break;

                case "open_lock":
                    string lockName = (string)jsonObject["locked_object"];
                    if (ObjectLocationManager.Instance.GetLockedObjects().Contains(lockName))
                    {                       
                        GameObject lockedObject = GameObject.Find(lockName);
                        if (!lockedObject.GetComponent<Lock>().TryKey(holding))
                        {
                            return $"Error: The agent is not holding a proper key for the lock on {lockName}.";
                        }
                        else if (Vector3.Distance(transform.position, lockedObject.transform.position) >= grabRange)
                        {
                            return $"Error: Not in range to attempt to unlock {lockedObject}.";
                        }
                        else if (lockedObject.GetComponent<Lock>().TryKey(holding))
                        {
                            return $"Success: Agent used their {holding} and the {lockName} was unlocked.";
                        }                      
                    }
                    else
                    {
                        return $"Error: {lockName} doesn't exist or doesn't have a lock."; //check
                    }
                    break;
                case "flip_bed":                   
                    if (ObjectLocationManager.Instance.GetLocations().Contains("Bett"))
                    {
                        if (role == Role.Pluto)
                        {
                            return $"Error: Since Pluto is flying he doesn't have the power required to flip the bed.";
                        }
                        else if (holding != "nothing")
                        {
                            return $"Can't attempt to flip the bed while holding an item.";
                        }
                        else if (Vector3.Distance(transform.position, GameObject.Find("Bett").transform.position) <= grabRange)
                        {
                            if (!(StateManager.Instance.bedFlipCounter >= 1) && role == Role.Jupiter)
                            {
                                StateManager.Instance.bedFlipCounter++;
                                return $"Grabbed onto the bed but to fully flip it, help will be required.";
                            }
                            else if (!(StateManager.Instance.bedFlipCounter >= 1) && role == Role.Neptun)
                            {
                                StateManager.Instance.bedFlipCounter++;
                                return $"Grabbed onto the bed but to fully flip it, help will be required.";
                            }
                            else
                            {
                                StateManager.Instance.bedFlipCounter++; 
                                return $"Success: Jupiter and Neptun flipped the bed.";
                            }
                        }
                        else
                        {
                            return $"Error: Not close enough to the Bed to flip.";
                        }

                    }
                    break;
                //case "deliver_message_to_other_agent":
                //    string receiver = (string)jsonObject["agent"];
                //    string message = (string)jsonObject["message"];
                //    return $"Delivered the message {message} to {receiver}";
                //    break;
                case "put_in_the_trash":
                    string trash = (string)jsonObject["item"];
                    if (ObjectLocationManager.Instance.GetGrabbables().Contains(trash))
                    {
                        if (holding != "nothing")
                        {
                            Debug.Log("distance to grabbable object" + Vector3.Distance(transform.position, GameObject.Find("Mülleimer").transform.position));
                            if (Vector3.Distance(transform.position, GameObject.Find("Mülleimer").transform.position) <= grabRange)
                            {                                
                                return $"Success: {holding} has been thrown in the trash.";
                            }
                            else
                            {
                                return $"Error: Not close enough to the Trashcan to put {holding} in the trash.";
                            }
                        }
                        else 
                        {
                        
                            return "Error: Not holding anything that could be thrown in the trash.";
                        }
                    }
                    else
                    {
                        return $"Error: {trash} can't be found or doesn't exist in the current context.";
                    }                
            }
            return "";
        }
        else
        {
            return "Function couldn't be found and is invalid";
        }
    }

    protected virtual async Task ExecuteFunction(string function, string parameters)
    {
        // retrieve the method from the current type based on the provided function name
        MethodInfo method = GetType().GetMethod(function, BindingFlags.Instance | BindingFlags.NonPublic | BindingFlags.Public);

        // check if the method exists
        if (method != null)
        {
            // convert the parameters from JSON format to a dictionary
            var args = JsonConvert.DeserializeObject<Dictionary<string, string>>(parameters);

            // get information about the parameters expected by the method
            var parameterInfos = method.GetParameters();

            // array to hold the arguments to pass to the method
            var methodArgs = new object[parameterInfos.Length];

            // loop through each expected parameter of the method
            // note: param name has to be the same as json property name
            for (int i = 0; i < parameterInfos.Length; i++)
            {
                var paramName = parameterInfos[i].Name;

                // if the argument exists in the passed parameters, add it to the methodArgs array
                if (args.TryGetValue(paramName, out string value))
                {
                    methodArgs[i] = value;
                }
                else
                {
                    // argument was expected but not provided
                    Debug.LogWarning($"Parameter '{paramName}' not found in JSON input");
                    methodArgs[i] = null; // Assign null for missing arguments
                }
            }

            //isActing = true;

            // invoke the identified method with the parsed arguments
            await (Task)method.Invoke(this, methodArgs);
        }
        else
        {
            // no method matched the provided function name
            Debug.Log("Method not found: " + function);
        }
    }

    protected virtual async Task move_to(string destination)
    {
        float startTime = Time.time;
        float maxDuration = 3.5f;

        if (destination == "User_pointing_location")
        {
            navMeshAgent.destination = AgentController.Instance.GetPointPosition().point;
            while (Vector3.Distance(transform.position, AgentController.Instance.GetPointPosition().point) > grabRange && Time.time - startTime < maxDuration)
            {
                await Task.Delay(30);
            }
            navMeshAgent.isStopped = true;
            navMeshAgent.ResetPath();
            log.AddEntry(Role.System, $"{role} moved to {destination}.");
        }
        else
        {
            Vector3 destinationTransform = ObjectLocationManager.Instance.GetObjectTransform(destination);

            if (destinationTransform != null && CanMoveTo(destinationTransform))
            {
                navMeshAgent.destination = destinationTransform;

                while (Vector3.Distance(transform.position, destinationTransform) > grabRange && Time.time - startTime < maxDuration)
                {
                    await Task.Delay(30);
                }
                navMeshAgent.isStopped = true;
                navMeshAgent.ResetPath();
                log.AddEntry(Role.System, $"{role} moved to {destination}.");

                return;
            }
            else
            {
                Debug.LogWarning($"Location '{destination}' not found or unreachable!");
            }
        }
    }


    protected async Task pick_up(string item)
    {
        if (item == "Teller")
        {
            if (StateManager.Instance.plateStack.Count > 0)
            {   
                if (Vector3.Distance(transform.position, GameObject.Find(item).transform.position) <= grabRange)
                {
                    GameObject grabbablePlate = StateManager.Instance.plateStack.Pop();

                    grabbablePlate.transform.SetParent(attachPoint);
                    grabbablePlate.transform.localPosition = Vector3.zero;
                    grabbablePlate.transform.localRotation = transform.parent.localRotation;
                    heldItem = grabbablePlate;
                    holding = item;
                    log.AddEntry(Role.System, $"{role} picked up a {item}.");
                }
            }
            else
            {
                Debug.Log("No plates left to pick up!");
            }
        }
        else if (item == "Vase")
        {
            StateManager.Instance.vaseOnBed = false;
            //GameObject.Find("Neptun").GetComponent<AgentGPT>().ResetHeldItems();
            //GameObject.Find("Jupiter").GetComponent<AgentGPT>().ResetHeldItems();
            GameObject vase = GameObject.Find("Vase");

            if (Vector3.Distance(transform.position, vase.transform.position) <= grabRange)
            {
                if (vase.transform.parent != null)
                {
                    if (vase.transform.parent.CompareTag("JupiterAttachPoint"))
                    {
                        GameObject.Find("JupiterCore").GetComponent<AgentGPT>().ResetHeldItems();
                    }
                    else if (vase.transform.parent.CompareTag("NeptunAttachPoint"))
                    {
                        GameObject.Find("NeptunCore").GetComponent<AgentGPT>().ResetHeldItems();
                    }
                    else if (vase.transform.parent.CompareTag("PlutoAttachPoint"))
                    {
                        GameObject.Find("PlutoCore").GetComponent<AgentGPT>().ResetHeldItems();
                    }
                }

                vase.transform.SetParent(attachPoint);

                vase.transform.localPosition = Vector3.zero;
                vase.transform.localRotation = transform.parent.localRotation;
                heldItem = vase;
                holding = item;
                log.AddEntry(Role.System, $"{role} picked up {item}.");

            }
        }
        else
        {
            GameObject grabbableObject = GameObject.Find(item + "Model");

            if (ObjectLocationManager.Instance.GetGrabbables().Contains(item))
            {
                if (grabbableObject != null && Vector3.Distance(transform.position, grabbableObject.transform.position) <= grabRange)
                {
                    // attachpoint in Inspektor, taking away from another agent
                    if (grabbableObject.transform.parent != null) {
                        if (grabbableObject.transform.parent.CompareTag("JupiterAttachPoint"))
                        {
                            GameObject.Find("JupiterCore").GetComponent<AgentGPT>().ResetHeldItems();
                        }
                        else if (grabbableObject.transform.parent.CompareTag("NeptunAttachPoint"))
                        {
                            GameObject.Find("NeptunCore").GetComponent<AgentGPT>().ResetHeldItems();
                        }
                        else if (grabbableObject.transform.parent.CompareTag("PlutoAttachPoint"))
                        {
                            GameObject.Find("PlutoCore").GetComponent<AgentGPT>().ResetHeldItems();
                        }
                    }
                    

                    grabbableObject.transform.SetParent(attachPoint);
                    GameObject origin = GameObject.Find(item);
                    if (origin != null)
                    {
                        origin.transform.SetParent(attachPoint);
                    }

                    grabbableObject.transform.localPosition = Vector3.zero;
                    grabbableObject.transform.localRotation = transform.parent.localRotation;
                    heldItem = grabbableObject;
                    holding = item;
                    log.AddEntry(Role.System, $"{role} picked up {item}.");

                }
            }

            else
            {
                Debug.Log("Object not found or not grabbable: " + item);
            }
        }
    }

    protected async Task give_item_to_other_agent(string agent)
    {

    }

    protected async Task place_on(string surface)
    {
        if (heldItem != null)
        {
            GameObject surfaceObject = GameObject.Find(surface+"Surface");
            if (surfaceObject != null)
            {
                heldItem.transform.SetParent(surfaceObject.transform);
                GameObject origin = GameObject.Find(holding);
                if (origin != null)
                {
                    origin.transform.SetParent(surfaceObject.transform);
                }

                heldItem.transform.localPosition = Vector3.zero;
                heldItem.transform.localRotation = Quaternion.identity;

                heldItem = null;
                holding = "nothing";
            }
            
        }
        /*else if (heldItem != null && surface == "Boden")
        {
            heldItem.transform.SetParent(null);
            heldItem.transform.localPosition = Vector3.zero;

            heldItem = null;
            holding = "nothing";
            //// add physics?
            //Rigidbody rb = heldItem.GetComponent<Rigidbody>();
            //if (rb != null)
            //{
            //    rb.isKinematic = false;
            //}
        }*/
    }

    protected async Task open_lock(string locked_object)
    {
        if (ObjectLocationManager.Instance.GetLockedObjects().Contains(locked_object))
        {
            GameObject lockedObject = GameObject.Find(locked_object);
            Lock lockedComponent = lockedObject.GetComponent<Lock>();

            if (Vector3.Distance(transform.position, lockedObject.transform.position) <= grabRange && lockedComponent != null)
            {
                if (lockedComponent.TryKey(holding))
                {
                    lockedComponent.Unlock();
                    heldItem.GetComponent <MeshRenderer>().enabled = false;
                    heldItem = null;
                    holding = "nothing";
                } 
            }
        }
    }

    protected async Task put_in_the_trash(string item)
    {
        if (heldItem != null && Vector3.Distance(transform.position, GameObject.Find("Mülleimer").transform.position) <= grabRange)
        {
            heldItem.GetComponent<MeshRenderer>().enabled = false;
            heldItem = null;
            holding = "nothing";
            binSound.Play();
        }
    }

    protected async Task flip_bed(string bed)
    {
        if (holding == "nothing" && Vector3.Distance(transform.position, GameObject.Find("Bett").transform.position) <= grabRange)
        {
            StateManager.Instance.FlipBed();
        }
    }

    protected async Task deliver_message_to_other_agent(string agent, string message)
    {
        GameObject.Find(agent).GetComponent<AgentGPT>().SendReply(message);
    }

    public bool CanMoveTo(Vector3 destination)
    {
        NavMeshPath path = new NavMeshPath();
        if (navMeshAgent.CalculatePath(destination, path))
        {
            if (path.status == NavMeshPathStatus.PathComplete)
            {
                return true;
            }
        }
        return false;
    }

    public void SetLabelActive()
    {
        //var materialsList = new List<Material>(rend.materials);
        //materialsList.Add(outlineMaterial);
        //rend.materials = materialsList.ToArray();

        //yellow = waiting, green = active and is set by SendReply
        loadingCircle.SetActive(true);
        nameText.color = Color.yellow;
    }

    public void SetLabelIdle()
    {
        //rend.materials = defaultMaterials;
        loadingCircle.SetActive(false);
        nameText.color = Color.white;
    }

    public void ClearMemory()
    {
        if (messages.Count > 1)
        {
            messages.RemoveRange(1, messages.Count - 1);
        }
        if (heldItem != null)
        {
            heldItem.transform.SetParent(null);
        }
        heldItem = null;
        holding = "nothing";
    }

    public void ResetHeldItems()
    {
        holding = "nothing";
    }
}