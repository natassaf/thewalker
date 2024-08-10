using UnityEngine;

public class CameraFollowZ : MonoBehaviour
{
    public Transform playerTransform; // The transform of the player to follow
    public float zOffset = -0.2f; // The offset on the Z-axis for the camera
    public float yOffset = 0f; 
    private Vector3 initialPosition; // Initial position of the camera

    void Start()
    {
        // Store the initial position of the camera
        initialPosition = transform.position;
    }

    void LateUpdate()
    {
        // Create a new position for the camera that only changes on the Z-axis
        Vector3 newPosition = new Vector3( playerTransform.position.x, initialPosition.y, playerTransform.position.z + zOffset);

        // Update the camera's position
        transform.position = newPosition;
    }
}
