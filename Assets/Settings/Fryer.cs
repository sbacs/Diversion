using UnityEngine;
using UnityEngine.UIElements;
public class Fryer : Appliance, Iinteractable
{
    public Transform knobTransform;
    public Transform leftGrill, rightGrill;
    private Vector3 upGrillLeft, upGrillRight;
    private Vector3 downGrillLeft, downGrillRight;
    
    [Header("Door Settings")]
    public float openAngle = 90f;
    public float openSpeed = 2f;
    private Quaternion offRotation;
    private Quaternion onRotation;
    
    void Start()
    {
        // Store the initial (closed) rotation
        offRotation = knobTransform.rotation;
        // Calculate the open rotation (rotate 90 degrees on Y axis from initial rotation)
        onRotation = offRotation * Quaternion.Euler(0, openAngle, 0);
        
        // Store both up and down positions at start
        upGrillLeft = leftGrill.position;
        upGrillRight = rightGrill.position;
        
        downGrillLeft = new Vector3(leftGrill.position.x, 1.0f, leftGrill.position.z);
        downGrillRight = new Vector3(rightGrill.position.x, 1.0f, rightGrill.position.z);
    }
    
    public void interact()
    {
        canCook = !canCook;
        Debug.Log("canCook");
    }
    
    void Update()
    {
        // Smoothly rotate the door to target rotation
        Quaternion targetRotation = canCook ? onRotation : offRotation;
        knobTransform.rotation = Quaternion.Slerp(knobTransform.rotation, targetRotation, openSpeed * Time.deltaTime);
        
        // Use stored positions as targets
        Vector3 leftTargetPosition = canCook ? upGrillLeft : downGrillLeft;
        Vector3 rightTargetPosition = canCook ? upGrillRight : downGrillRight;
        
        leftGrill.position = Vector3.Slerp(leftGrill.position, leftTargetPosition, openSpeed * Time.deltaTime);
        rightGrill.position = Vector3.Slerp(rightGrill.position, rightTargetPosition, openSpeed * Time.deltaTime);
    }
}