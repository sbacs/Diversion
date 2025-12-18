using UnityEngine;

public class Door : MonoBehaviour, Iinteractable
{
    [Header("Door Settings")]
    public float openAngle = 90f;
    public float openSpeed = 2f;
    
    private bool isOpen = false;
    private Quaternion closedRotation;
    private Quaternion openRotation;

    void Start()
    {
        // Store the initial (closed) rotation
        closedRotation = transform.rotation;
        
        // Calculate the open rotation (rotate 90 degrees on Y axis from initial rotation)
        openRotation = closedRotation * Quaternion.Euler(0, openAngle, 0);
    }

    public void interact()
    {
        Debug.Log("Door interacted!");
        
        // Toggle door state
        isOpen = !isOpen;
    }

    void Update()
    {
        // Smoothly rotate the door to target rotation
        Quaternion targetRotation = isOpen ? openRotation : closedRotation;
        transform.rotation = Quaternion.Slerp(transform.rotation, targetRotation, openSpeed * Time.deltaTime);
    }
}
