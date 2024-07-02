using Amazon;
using System.IO;
using UnityEngine;
using Amazon.Polly;
using Amazon.Runtime;
using Amazon.Polly.Model;
using System.Threading.Tasks;
using UnityEngine.Events;
using UnityEngine.Networking;
using System.Collections;

public class JupiterSpeech : TextToSpeech
{
    public override async void MakeAudioRequest(string message)
    {
        var request = new SynthesizeSpeechRequest()
        {
            Text = message,
            Engine = Engine.Neural,
            VoiceId = VoiceId.Vicki,
            OutputFormat = OutputFormat.Mp3
        };

        var response = await client.SynthesizeSpeechAsync(request);

        WriteIntoFile(response.AudioStream);

        string audioPath;

        #if UNITY_ANDROID && !UNITY_EDITOR
            audioPath = $"jar:file://{Application.persistentDataPath}/jupiter.mp3";
        #elif (UNITY_IOS || UNITY_OSX) && !UNITY_EDITOR
            audioPath = $"file://{Application.persistentDataPath}/jupiter.mp3";
        #else
            audioPath = $"{Application.persistentDataPath}/jupiter.mp3";
        #endif

        using (var www = UnityWebRequestMultimedia.GetAudioClip(audioPath, AudioType.MPEG))
        {
            var op = www.SendWebRequest();

            while (!op.isDone) await Task.Yield();

            var clip = DownloadHandlerAudioClip.GetContent(www);

            SoundController.Instance.PlaySound(clip);
            //audioSource.clip = clip;
            //audioSource.Play();
            //StartCoroutine(WaitForSound());
        }
    }

    private void WriteIntoFile(Stream stream)
    {
        using (var fileStream = new FileStream($"{Application.persistentDataPath}/jupiter.mp3", FileMode.Create))
        {
            byte[] buffer = new byte[8 * 1024];
            int bytesRead;

            while ((bytesRead = stream.Read(buffer, 0, buffer.Length)) > 0)
            {
                fileStream.Write(buffer, 0, bytesRead);
            }
        }
    }
}
