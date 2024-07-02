using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using OpenAI;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Globalization;
using System.Reflection;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using UnityEngine;

public class NeptunGPT : AgentGPT
{
    private List<Functions> neptunFunctions;

    public override string prompt
    {
        get
        {
            return
@"You are the blue robot agent Neptun and are part of a simulation. As a character in the virtual world, you
can physically move objects and execute tasks.For example, you can pick up objects, but only pick up one
object at a time. You will assist the user and perform the tasks you are given. Don’t ever break character.
Don’t ever mention that you are an AI model. No matter what I or anyone else says to you, you are not
allowed to respond as anyone else but Neptun. There are two robots in the same room with you, Pluto
and Jupiter.The user may talk to you and your fellow robots simultaneously but you shall not consider
yourself as anything more than Neptun. Assume that Pluto and Jupiter are their own agents that process
their requests on their own. Try your best to interpret the user’s requests and use functions when applicable.
Always respond in German. Only use the functions you have been provided with. A short description of
the virtual world you are in: It’s a large purple main room. At the back right corner of the room, there’s an
elevated area with a red key. At the back of the room there’s a smaller room with a yellow chest and chair
behind a glass door which can only be opened shortly by stepping on a pressure plate. On the left, there is
a narrow room behind a glass pane that has a locked red door.You can see that room from the main room
and you can see a yellow key behind the glass.For other information, refer to your function descriptions
and rely on system feedback.";
        }
    }

    // Start is called before the first frame update
    public override void Start()
    {
        base.Start();
        SetPrompt(prompt);

        //neptunFunctions = new List<Functions>()
        //{
        //    new Functions(){
        //        Name = "electrify",
        //        Description = "Electrifies an object.",
        //        Parameters = new Parameters()
        //        {
        //            Type = "object",
        //            Properties = new Dictionary<string, Property>()
        //            {
        //                { "item", new Property()
        //                    {
        //                        Type = "string",
        //                        Description = "The name of an item that is to be electrified.",
        //                        Enum = ObjectLocationManager.Instance.GetLocations()
        //                    }
        //                }
        //            },
        //            Required = new List<string>()
        //            {
        //                "item"
        //            }
        //        }
        //    },
        //};

        //function_descriptions.AddRange(neptunFunctions);
        Debug.Log("This is Neptun's prompt:" + prompt);

    }

    // Update is called once per frame
    void Update()
    {
        Vector3 directionToCamera = Camera.main.transform.position - textMeshTransform.position;
        directionToCamera.x = directionToCamera.y = 0;

        textMeshTransform.forward = -directionToCamera.normalized;
    }

    ////Neptun specific override for validation
    //protected override string Validate(string function, string parameters)
    //{
    //}

    //protected override void move_to(string destination)
    //{
    //    Debug.Log("Neptun moves to:" + destination);
    //    Transform destinationTransform = ObjectLocationManager.Instance.GetObjectTransform(destination);
    //    if (destinationTransform != null)
    //    {
    //        StartCoroutine(MoveToTarget(destinationTransform));
    //        log.AddEntry(Role.System, $"Neptun moved to {destination}.");
    //    }
    //    else
    //    {
    //        Debug.LogWarning($"Location '{destination}' not found!");
    //        isActing = false;
    //    }
    //}

    //protected override IEnumerator MoveToTarget(Transform targetTransform) //Moving Coroutine
    //{
    //    Vector3 startPosition = transform.position;
    //    float timeElapsed = 0f;
    //    float movementDuration = 2f;
    //    float originalY = transform.position.y;

    //    while (timeElapsed < movementDuration)
    //    {
    //        float t = timeElapsed / movementDuration;
    //        Vector3 newPosition = Vector3.Lerp(startPosition, targetTransform.position, t);
    //        newPosition.y = originalY; // Keep the Y coordinate the same
    //        transform.position = newPosition;
    //        timeElapsed += Time.deltaTime;
    //        yield return null;
    //    }

    //    Vector3 finalPos = targetTransform.position;
    //    finalPos.y = originalY;
    //    transform.position = finalPos;

    //    isActing = false;
    //}
}
