using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class PressurePlate : MonoBehaviour
{
    [SerializeField] protected AudioSource audioSource;
    [SerializeField] protected TextMeshPro tmp;
    [SerializeField] protected Door door;

    public float requiredWeight = 5f;       
    public Animator plateAnimator; // Reference to the Animator component

    private float currentWeight = 0f;
    private bool isActivated = false;

    void OnTriggerEnter(Collider other)
    {
        Debug.Log("trigger enter" + other.name);

        GameObject topmostParent = GetTopmostParent(other.gameObject);
        Weight weightComponent = topmostParent.GetComponent<Weight>();

        if (weightComponent != null)
        {
            currentWeight += weightComponent.weightValue;
            tmp.text = currentWeight + " kg";
            

            if (currentWeight >= requiredWeight && !isActivated)
            {
                door.Open();
                tmp.color = Color.green;
                audioSource.Play();
                plateAnimator.SetTrigger("enter"); // Play the sinking animation
                isActivated = true;
            }

        }
    }

    void OnTriggerExit(Collider other)
    {
        Debug.Log("trigger exit");

        GameObject topmostParent = GetTopmostParent(other.gameObject);
        Weight weightComponent = topmostParent.GetComponent<Weight>();

        if (weightComponent != null)
        {
            currentWeight -= weightComponent.weightValue;
            tmp.text = currentWeight + " kg";

            if (currentWeight < requiredWeight && isActivated)
            {
                door.Close();
                tmp.color = Color.white;
                audioSource.Play();
                plateAnimator.SetTrigger("exit"); // Play the rising animation
                isActivated = false;
            }
        }
    }

    // Function to retrieve the topmost parent
    GameObject GetTopmostParent(GameObject obj)
    {
        while (obj.transform.parent != null)
        {
            obj = obj.transform.parent.gameObject;
        }
        return obj;
    }
}

