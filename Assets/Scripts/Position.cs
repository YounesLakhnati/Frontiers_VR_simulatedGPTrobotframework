using System;
using UnityEngine;

public class Position : MonoBehaviour
{
    public event Action<GameObject> OnTransformChanged;

    private Vector3 lastPosition;

    private void Start()
    {
        lastPosition = transform.position;
    }

    private void Update()
    {
        if (Vector3.Distance(lastPosition, transform.position) > 0.01f)
        {
            lastPosition = transform.position;
            OnTransformChanged?.Invoke(gameObject);
        }
    }
}