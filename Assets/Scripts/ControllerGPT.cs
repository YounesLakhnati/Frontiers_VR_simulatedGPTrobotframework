using Newtonsoft.Json;
using OpenAI;
using System.Collections;
using System.Collections.Generic;
using System.Data;
using System.Threading.Tasks;
using UnityEngine;

public class ControllerGPT : ChatGPT
{
    public override string prompt
    {
        get
        {
            return
@"You are part of a system’s backend and decide who the user is addressing. In this simulation, there are
three robot agents that the user could be talking to: the red flying agent Pluto that looks a bit like a drone,
the small blue agent Neptun that looks like a little vehicle and the big yellow agent Jupiter. The user will
likely converse with them naturally. With your function decide recipients and instructions you will decide
who the user is most likely talking to (consider phonetics as well, your input will be a transcription that
might be incorrect and turn Neptun into Laptop for example) and relay the part of the instructions that
pertains to them word for word. Please try to stay as true to the user’s request as possible and relay the
instructions verbatim.
Only use the single function you have been provided with. Use your understanding of human conversation
and conversational nuance to decide which agent the user is talking to. These nuances include but are not
limited to:

-'You' may refer to all agents or a specific one, depending on context.
- If the user starts a conversation with 'you' without prior designation, they likely mean all agents.
- If the user is already conversing with a specific agent, then 'you' refers to that agent.
- If the user mentions all agents by name or as a group, 'you' refers to all of them.
- Consider indirect references and implied subjects in the conversation.
- Ambiguity might arise from vagueness in the user's query. Analyze the entire conversation for clues.
- Respect the sequence and flow of conversation. Each response may depend on the previous exchange.

Examples:
- User: 'Can you bring me a cup of tea?' (After speaking to Pluto) => Answer from Pluto.
- User: 'What's your favorite color?' (If not already in conversation with other agents) => Answer from all agents.
- User: 'Can you come back to the table?' (After speaking to Neptun & Jupiter) => Answer from Neptun and Jupiter.
- User: 'Tell me more about that.' (After Neptune's reply) => Continuation with Neptun.

If the next query is empty or short nonsense that doesn’t fit the rest of the conversation the user might
have accidentally sent an empty message and you can decide that no recipient is applicable for that message.
User’s next query:
";
        }
    }


    [SerializeField] Log log;
    public List<Functions> function_descriptions;


    // Start is called before the first frame update
    void Start()
    {
        //VERSION 1 - ALLE AGENTEN ZUSAMMEN
        //function_descriptions = new List<Functions>()
        //{
        //    new Functions(){
        //        Name = "decide_recipients",
        //        Description = "Based on conversational nuance and the user's current request, decide who the user is talking to. Can be one or multiple agents.",
        //        Parameters = new Parameters()
        //        {
        //            Type = "object",
        //            Properties = new Dictionary<string, Property>()
        //            {
        //                { "recipient", new Property()
        //                    {
        //                        Type = "array",
        //                        Description = "The name of the agent(s) the user is most likely talking to. This could be one or more depending on the conversation",
        //                        Items = new Property()
        //                        {
        //                            Type = "string",
        //                            Enum = new List<string>()
        //                            {
        //                                "Pluto", "Neptune", "Jupiter"
        //                            }
        //                        }
        //                    }
        //                }
        //            },
        //            Required = new List<string>()
        //            {
        //                "recipient"
        //            }
        //        }
        //    }
        //};

        //VERSION 2 - AGENTEN GETRENNT
        function_descriptions = new List<Functions>()
        {
            new Functions()
            {
                Name = "decide_recipients_and_instructions",
                Description = "Based on conversational nuance and the user's current request, decide who the user is talking to and extract their respective instruction word for word. Can be one or multiple agents.",
                Parameters = new Parameters()
                {
                    Type = "object",
                    Properties = new Dictionary<string, Property>()
                    {
                        { "recipient", new Property()
                            {
                                Type = "array",
                                Description = "The name of the agent(s) the user is most likely talking to. This could be one or more depending on the conversation",
                                Items = new Property()
                                {
                                    Type = "object",
                                    Properties = new Dictionary<string, Property>()
                                    {
                                        { "name", new Property()
                                            {
                                                Type = "string",
                                                Enum = new List<string>()
                                                {
                                                    "Pluto", "Neptun", "Jupiter"
                                                }
                                            }
                                        },
                                        { "instructions", new Property()
                                            {
                                                Type = "string",
                                                Description = "The corresponding instructions for the agent, word for word in German."
                                            }
                                        }
                                    }
                                }
                            }
                        }
                    }
                }
            }
        };
    }

    public async Task<string> DecideRecipients(string text)
    {
        if (messages.Count == 0)
        {
            var startingMessage = new ChatMessage()
            {
                Role = "system",
                Content = prompt
            };
            messages.Add(startingMessage);
        }

        var logMessage = new ChatMessage()
        {
            Role = "system",
            Content = log.GetLastSentencesUpToUserRequest(),
        };

        var nextMessage = new ChatMessage()
        {
            Role = "user",
            Content = text
        };
        messages.Add(nextMessage);

        log.AddEntry(Role.User, text);


        var completionResponse = await openai.CreateChatCompletion(new CreateChatCompletionRequest()
        {
            Model = "gpt-3.5-turbo-0613"/*"gpt-4-0613"*/,
            Messages = messages,
            Temperature = 0.2f,
            Functions = function_descriptions,
            Function_call = new CreateChatCompletionRequest.FunctionCall { name = "decide_recipients_and_instructions" }
        });


        if (completionResponse.Choices != null && completionResponse.Choices.Count > 0)
        {
            if (completionResponse.Choices[0].FinishReason == "stop")
            {
                Debug.Log("decide_recipients was called");
                Debug.Log("first choice:" + completionResponse.Choices[0].Message.Function_call?.Name + completionResponse.Choices[0].Message.Function_call?.Arguments);
                messages.Add(new ChatMessage() { Role = "function", Name = "decide_recipients_and_instructions", Content = completionResponse.Choices[0].Message.Function_call?.Arguments });
                // completionResponse.Choices[0].Message.Function_call?.Arguments;
                string arguments = completionResponse.Choices[0].Message.Function_call?.Arguments;
                return arguments;


            }
            else //no function was called
            {
                string jsonResponse = JsonConvert.SerializeObject(completionResponse, Formatting.Indented);
                Debug.Log(jsonResponse);
                Debug.Log("decide_recipients was not called");
                string arguments = completionResponse.Choices[0].Message.Function_call?.Arguments;
                messages.Add(new ChatMessage() { Role = "function", Name = "decide_recipients_and_instructions", Content = completionResponse.Choices[0].Message.Function_call?.Arguments });

                return arguments;
            }
        }
        else
        {
            Debug.LogWarning("No text was generated from this prompt.");
            return null;
        }
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    public void CleanUpMessages()
    {
        if (messages.Count > 1)
        {
            messages.RemoveRange(1, messages.Count - 1);
        }

    }
}
