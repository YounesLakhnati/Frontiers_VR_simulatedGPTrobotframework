using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SoundController : MonoBehaviour
{
    public static SoundController Instance { get; private set; }

    public AudioSource audioSource;
    private Queue<AudioClip> clipQueue;

    private void Awake()
    {
        clipQueue = new Queue<AudioClip>();

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

    void Update()
    {
        if (audioSource.isPlaying == false && clipQueue.Count > 0)
        {
            audioSource.clip = clipQueue.Dequeue();
            audioSource.Play();
        }
    }

    public void PlaySound(AudioClip clip)
    {
        clipQueue.Enqueue(clip);
    }
}