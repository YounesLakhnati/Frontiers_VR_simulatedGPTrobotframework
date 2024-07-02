using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Hover : MonoBehaviour
{
    public float amplitude = 0.08f;
    public float speed = 0.8f;

    private float startingY;

    private void Start()
    {
        startingY = transform.position.y;
    }

    private void Update()
    {

        if (transform.parent == null)
        {
            Vector3 p = transform.position;
            p.y = startingY + amplitude * Mathf.Cos(Time.time * speed);
            transform.position = p;

        }
    }
}
