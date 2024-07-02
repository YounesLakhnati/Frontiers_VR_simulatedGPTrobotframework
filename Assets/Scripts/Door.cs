using System.Collections;
using TMPro;
using UnityEngine;
using UnityEngine.AI;

public class Door : MonoBehaviour
{
    private Vector3 closedPosition;
    private Vector3 openedPosition;
    private bool isOpen = false;

    [SerializeField] private const float MOVE_SPEED = 1.4f;
    [SerializeField] protected NavMeshObstacle obstacle;
    [SerializeField] protected TextMeshPro countdownText;
    private Coroutine doorMovement; // To keep track of ongoing movement

    public AudioSource audio;
    public AudioSource clockTicking;

    private void Start()
    {
        closedPosition = transform.position;
        openedPosition = closedPosition + new Vector3(-1.4f, 0, 0);
    }

    public void Open()
    {
        if (doorMovement != null)
        {
            StopCoroutine(doorMovement); // Stop the ongoing movement
        }
        clockTicking.Play();
        doorMovement = StartCoroutine(MoveDoor(openedPosition));
        isOpen = true;

        // Set delay
        StartCoroutine(CloseAfterDelay(3.5f));
    }

    public void Close()
    {
        if (doorMovement != null)
        {
            StopCoroutine(doorMovement); // Stop the ongoing movement
        }
        doorMovement = StartCoroutine(MoveDoor(closedPosition));
        audio.Play();
        clockTicking.Stop();
        obstacle.enabled = true;
        isOpen = false;
    }

    private IEnumerator MoveDoor(Vector3 targetPosition)
    {
        float step = MOVE_SPEED * Time.deltaTime;

        while (Vector3.Distance(transform.position, targetPosition) > step)
        {
            transform.position = Vector3.MoveTowards(transform.position, targetPosition, step);
            yield return null; // Wait for the next frame
        }

        transform.position = targetPosition;
    }

    private IEnumerator CloseAfterDelay(float delay)
    {
        float elapsedTime = 0;
        countdownText.fontSize = 35;

        while (elapsedTime < delay)
        {
            elapsedTime += Time.deltaTime;
            float timeLeft = Mathf.Max(0, delay - elapsedTime);

            // Calculate seconds and milliseconds
            int seconds = (int)timeLeft;
            int milliseconds = (int)((timeLeft - seconds) * 100);

            countdownText.text = $"{seconds}:{milliseconds:00}";

            yield return null; // Wait for the next frame
        }

        countdownText.fontSize = 25.69f;
        Close();
    }
}



//alte version
//using System.Collections;
//using UnityEngine;

//public class Door : MonoBehaviour
//{
//    private Vector3 closedPosition;
//    private Vector3 openedPosition;
//    private bool isOpen = false;

//    [SerializeField] private const float MOVE_SPEED = 1.4f; 
//    private Coroutine doorMovement; // To keep track of ongoing movement

//    private void Start()
//    {
//        closedPosition = transform.position;
//        openedPosition = closedPosition + new Vector3(-1.4f, 0, 0);
//    }

//    private void Update()
//    {

//    }

//    public void Open()
//    {
//        if (doorMovement != null)
//        {
//            StopCoroutine(doorMovement); // Stop the ongoing movement
//        }
//        doorMovement = StartCoroutine(MoveDoor(openedPosition));
//        isOpen = true;
//    }

//    public void Close()
//    {
//        if (doorMovement != null)
//        {
//            StopCoroutine(doorMovement); // Stop the ongoing movement
//        }
//        doorMovement = StartCoroutine(MoveDoor(closedPosition));
//        isOpen = false;
//    }

//    private IEnumerator MoveDoor(Vector3 targetPosition)
//    {
//        float step = MOVE_SPEED * Time.deltaTime;

//        while (Vector3.Distance(transform.position, targetPosition) > step)
//        {
//            transform.position = Vector3.MoveTowards(transform.position, targetPosition, step);
//            yield return null; // Wait for the next frame
//        }

//        transform.position = targetPosition;
//    }
//}






