using System;
using TMPro;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.UI;

namespace OpenAI
{
    public class Whisper : OpenAIFunction
    {
        public InputActionReference recordAction;
        [SerializeField] private TextMeshProUGUI transcriptionText;
        [SerializeField] private int maxDuration = 60;
        [SerializeField] private float minDuration = 0.6f; 

        private string MicrophoneName; 
        private readonly string fileName = "output.wav";
        private AudioClip clip;
        private bool isRecording;


        private void Start()
        {
#if UNITY_EDITOR
            // This code will only run in the Unity Editor (Play Mode)
            MicrophoneName = Microphone.devices[0];
#else
            MicrophoneName = "Android voice recognition input";
#endif
        }

        private void Awake()
        {
            recordAction.action.started += RecordStarted;
            recordAction.action.canceled += RecordStopped;
        }

        private void OnDestroy()
        {
            recordAction.action.started -= RecordStarted;
            recordAction.action.canceled -= RecordStopped;
        }

        private void RecordStarted(InputAction.CallbackContext ctx)
        {
            if (isRecording) // Check if recording is already happening
            {
                return;
            }

            transcriptionText.text = "RECORDING...";

            isRecording = true;

            clip = Microphone.Start(MicrophoneName, false, maxDuration, 44100);
            Debug.Log("Recording started with " + MicrophoneName);
        }

        private async void RecordStopped(InputAction.CallbackContext ctx)
        {
            try
            {
                if (!isRecording) // Check if recording is not currently happening
                {
                    return; //exit
                }

                transcriptionText.text = "TRANSKRIBIERUNG ERFOLGT..."; // inform user that input is being transcribed

                var position = Microphone.GetPosition(MicrophoneName); // set primer for trimming
                Microphone.End(MicrophoneName); // stop recording
                isRecording = false; // allow further recording

                AgentController.Instance.SetPointPosition(); // pointing util

                float[] samples = new float[position]; //begin trim
                clip.GetData(samples, 0);

                AudioClip newClip = AudioClip.Create("Trimmed Clip", position, 1, 44100, false);
                newClip.SetData(samples, 0);

                byte[] data = SaveWav.Save(fileName, newClip); // end trim

                Debug.Log($"Sending audio of length {newClip.length} seconds for transcription");

                if (newClip.length > minDuration) // only send the clip if it's longer than minDuration
                {
                    var req = new CreateAudioTranscriptionsRequest // set up API request data structure
                    {
                        FileData = new FileData() { Data = data, Name = "audio.wav" },
                        Model = "whisper-1",
                        Prompt = "Transcribe the input in German",
                        Language = "de"
                    };

                    var res = await openai.CreateAudioTranscription(req); // send clip to OpenAI for transcription and wait for it
                    transcriptionText.text = res.Text; // transcription is made visible to the user
                    Debug.Log("res.text:" + res.Text);
                    string recipients_and_instructions = await AgentController.Instance.ControllerGPT.DecideRecipients(res.Text); // transcription is passed on to GPT
                    AgentController.Instance.EnqueueAgentRequests(recipients_and_instructions); // transcription is passed on to GPT
                }
                else
                {
                    transcriptionText.text = "";
                }
            }
            catch (Exception e)
            {
                Debug.Log(e.Message);
            }
        }
    }
}

 