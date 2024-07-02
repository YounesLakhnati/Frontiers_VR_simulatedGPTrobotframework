using UnityEngine;
using System.Collections.Generic;
using System.Linq;

namespace OpenAI
{
    public class ChatGPT : OpenAIFunction
    {
        protected List<ChatMessage> messages = new List<ChatMessage>();
        public virtual string prompt
        {
            get; protected set;
        }

        private void Start()
        {
        }

        public virtual void SetPrompt(string text)
        {
            prompt = text;
        }

        public string GetLastMessage() => messages.Count != 0 ? messages.Last().Content.Trim() : null;

        public async virtual void SendReply(string text)
        {
            // Initialize GPT with the prompt as its first message
            if (messages.Count == 0)
            {
                var startingMessage = new ChatMessage()
                {
                    Role = "system",
                    Content = prompt,
                };
                messages.Add(startingMessage);
            }

            // Create a new message based on the text parameter of this method
            var newMessage = new ChatMessage()
            {
                Role = "user",
                Content = text,
            };


            // Let GPT complete the instruction
            var completionResponse = await openai.CreateChatCompletion(new CreateChatCompletionRequest()
            {
                Model = "gpt-3.5-turbo-0613",
                Messages = messages,
            });

            // If GPT returned a chat completion, extract and trim the content, then add it to the chat history
            if (completionResponse.Choices != null && completionResponse.Choices.Count > 0)
            {
                var message = completionResponse.Choices[0].Message; // locate message
                message.Content = message.Content.Trim(); // trim the message's content (the part that we are interested in)
                messages.Add(message); // add message to history

                string exampleString = message.Content; // we can now use the GPT output
            }
            else
            {
                Debug.LogWarning("No text was generated from this prompt.");
            }
        }
    }  
}
