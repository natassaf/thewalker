using UnityEngine;

public class CameraFollowZ : MonoBehaviour
{
    // The transform of the player to follow
    public Transform playerTransform; 

    // The offset on the Z-axis for the camera
    public float zOffset = -0.2f;     
    public float yOffset = 0f; 

     // Initial position of the camera
    private Vector3 initialPosition;

    void Start()
    {
        // Store the initial position of the camera
        initialPosition = transform.position;
    }

    void LateUpdate()
    {
        // Create a new position for the camera that only changes on the Z-axis
        Vector3 newPosition = new Vector3( playerTransform.position.x, 
                                           initialPosition.y,
                                            playerTransform.position.z + zOffset);

        // Update the camera's position
        transform.position = newPosition;
    }
}
