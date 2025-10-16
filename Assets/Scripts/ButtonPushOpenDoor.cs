using UnityEngine;
using UnityEngine.XR.Interaction.Toolkit;
using UnityEngine.XR.Interaction.Toolkit.Interactables;
public class ButtonPushOpenDoor : MonoBehaviour
{
    public Animator animator;
    public string boolName = "Open";
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        GetComponent<XRSimpleInteractable>().selectEntered.AddListener((x) => OpenDoor());
    }
    public void OpenDoor()
    {
        Debug.LogError("Button Pressed");
        bool isOpen = animator.GetBool(boolName);
        animator.SetBool(boolName, !isOpen);
    }
    //// Update is called once per frame
    //void Update()
    //{

    //}
}