using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;
using UnityEngine.InputSystem;
using UnityEngine.XR.Interaction.Toolkit;

public class ImageDisplay : MonoBehaviour
{
    public Material[] images;
    public MeshRenderer[] nameMeshes;
    public InputActionReference previewAction;
    public InputActionReference advancePreviewAction;
    public InputActionReference stateForwardAction;
    public InputActionReference stateBackwardAction;

    public XRInteractorLineVisual lineVisual;
    public MeshRenderer quadRenderer;


    //public int currentImage = 0;
    public int currentState = 0;

    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    private void Awake()
    {
        previewAction.action.started += DisplayPreview;
        previewAction.action.canceled += HidePreview;
        advancePreviewAction.action.started += AdvancePreview;
        advancePreviewAction.action.canceled += RewindPreview;
        stateForwardAction.action.performed += AdvanceState;
        stateBackwardAction.action.performed += RewindState;
    }

    private void OnDestroy()
    {
        previewAction.action.started -= DisplayPreview;
        previewAction.action.canceled -= HidePreview;
        advancePreviewAction.action.started -= AdvancePreview;
        advancePreviewAction.action.canceled -= RewindPreview;
        stateForwardAction.action.performed -= AdvanceState;
        stateBackwardAction.action.performed -= RewindState;
    }

    public void DisplayPreview(InputAction.CallbackContext ctx)
    {
        GameObject.Find("left_quest2_mesh(Clone)").GetComponent<MeshRenderer>().enabled = false;
        GameObject.Find("right_quest2_mesh(Clone)").GetComponent<MeshRenderer>().enabled = false;
        //lineVisual.enabled = false;
        quadRenderer.enabled = true;
        foreach (MeshRenderer mr in nameMeshes)
        {
            mr.enabled = false;
        }
    }

    public void HidePreview(InputAction.CallbackContext ctx)
    {
        GameObject.Find("right_quest2_mesh(Clone)").GetComponent<MeshRenderer>().enabled = true;
        //lineVisual.enabled = true;
        quadRenderer.enabled = false;
        foreach (MeshRenderer mr in nameMeshes)
        {
            mr.enabled = true;
        }
    }

    public void AdvancePreview(InputAction.CallbackContext ctx)
    {

    }

    public void RewindPreview(InputAction.CallbackContext ctx)
    {

    }

    public void AdvanceState(InputAction.CallbackContext ctx)
    {
        currentState++;
        StateManager.Instance.LoadState(currentState % 7);
        quadRenderer.material = images[(currentState) % 7];
    }


    public void RewindState(InputAction.CallbackContext ctx)
    {
        currentState--;
        if (currentState < 0) currentState += 7;
        currentState %= 7;
        StateManager.Instance.LoadState(currentState);
        quadRenderer.material = images[currentState];
    }
}
