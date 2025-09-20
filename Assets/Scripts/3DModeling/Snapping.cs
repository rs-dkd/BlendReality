using UnityEngine;
using UnityEngine.XR.Interaction.Toolkit;
using UnityEngine.XR.Interaction.Toolkit.Interactables;

[RequireComponent(typeof(XRGrabInteractable))]
public class Snapping : MonoBehaviour
{
    [Header("Position Snapping")]
    public bool snapToGrid = false;
    public float gridSize = 0.5f;

    [Header("Rotation Snapping")]
    public bool snapToAngle = false;
    public float snapAngle = 45.0f;

    private XRGrabInteractable grabInteractable;

    // Awake is called when the script instance is being loaded
    private void Awake()
    {
        // Get the XRGrabInteractable component on this object
        grabInteractable = GetComponent<XRGrabInteractable>();
    }

    // LateUpdate is called every frame, after all Update functions have been called
    private void LateUpdate()
    {
        // Check if the object is currently being held (selected)
        if (grabInteractable.isSelected)
        {
            if (snapToGrid)
            {
                SnapPosition();
            }
            if (snapToAngle)
            {
                SnapRotation();
            }
        }
    }

    private void SnapPosition()
    {
        if (gridSize <= 0) return;
        
        // Calculate the target position on the grid based on the hand's position
        float x = Mathf.Round(transform.position.x / gridSize) * gridSize;
        float y = Mathf.Round(transform.position.y / gridSize) * gridSize;
        float z = Mathf.Round(transform.position.z / gridSize) * gridSize;

        Debug.Log(new Vector3(x, y, z));
        transform.position = new Vector3(x, y, z);
    }

    private void SnapRotation()
    {
        if (snapAngle <= 0) return;

        // Calculate the target rotation based on the hand's rotation
        Vector3 currentRotation = transform.eulerAngles;
        float x = Mathf.Round(currentRotation.x / snapAngle) * snapAngle;
        float y = Mathf.Round(currentRotation.y / snapAngle) * snapAngle;
        float z = Mathf.Round(currentRotation.z / snapAngle) * snapAngle;

        transform.rotation = Quaternion.Euler(x, y, z);
    }
}