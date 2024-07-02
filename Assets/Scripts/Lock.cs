using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

public class Lock : MonoBehaviour
{
    [SerializeField] protected string requiredKey;
    [SerializeField] protected AudioSource audioSource;

    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    //returns true if appropriate key is given as string
    public bool TryKey(string key)
    {
        return requiredKey == key;       
    }

    public void Unlock()
    {
        StartCoroutine(Unlocking());
    }

    private IEnumerator Unlocking()
    {
        audioSource.Play();

        // Wait for audio
        yield return new WaitForSeconds(2);

        MeshRenderer[] renderers = gameObject.transform.parent.GetComponentsInChildren<MeshRenderer>();

        // Loop through the renderers and disable each one
        foreach (MeshRenderer renderer in renderers)
        {
            renderer.enabled = false;
        }

        NavMeshObstacle obstacle = gameObject.transform.parent.GetComponent<NavMeshObstacle>();
        if (obstacle != null)
        {
            obstacle.enabled = false;
        }
    }
}
