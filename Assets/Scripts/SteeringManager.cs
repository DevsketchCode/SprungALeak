using UnityEngine;

public class SteeringManager : MonoBehaviour
{
    // Public properties
    public Transform steeringObject;
    public Transform captainPosition;
    public float steeringSpeed = 5f;
    public float maxSteeringRotation = 45f;
    public ObstacleSpawner obstacleSpawner;
    public MonoBehaviour playerMovementScript;

    // Camera references for the player and steering views
    public Camera playerCamera;
    public Camera steeringCamera;

    // A public property to allow other scripts to safely read the steering state.
    public bool IsSteering { get { return isSteering; } }

    // Private fields
    private bool isSteering = false;
    private bool isPlayerInRange = false;

    // We'll add a Start() method to ensure camera state is correct from the beginning.
    void Start()
    {
        // Make sure the player camera is active and the steering camera is not.
        if (playerCamera != null)
        {
            playerCamera.enabled = true;
        }
        if (steeringCamera != null)
        {
            steeringCamera.enabled = false;
        }
    }

    void Update()
    {
        if (isPlayerInRange && Input.GetKeyDown(KeyCode.E))
        {
            if (!isSteering)
            {
                EnterSteeringMode();
            }
            else
            {
                ExitSteeringMode();
            }
        }

        if (isSteering)
        {
            float horizontalInput = Input.GetAxis("Horizontal");
            float steeringAmount = horizontalInput * steeringSpeed;

            if (obstacleSpawner != null)
            {
                obstacleSpawner.ApplySteering(steeringAmount);
            }

            float targetRotationZ = -horizontalInput * maxSteeringRotation;
            // Use localRotation to rotate around the steering wheel's local axis.
            steeringObject.localRotation = Quaternion.Euler(0, 0, targetRotationZ);
        }
    }

    public void SetPlayerInRange(bool inRange)
    {
        isPlayerInRange = inRange;
    }

    // Making these methods public so FirstPersonController can access them.
    public void EnterSteeringMode()
    {
        isSteering = true;

        // Disable the player's movement script to "freeze" their position
        if (playerMovementScript != null)
        {
            playerMovementScript.enabled = false;
        }

        // Switch to the steering camera
        if (playerCamera != null)
        {
            playerCamera.enabled = false;
        }
        if (steeringCamera != null)
        {
            steeringCamera.enabled = true;
        }
    }

    // Making these methods public so FirstPersonController can access them.
    public void ExitSteeringMode()
    {
        isSteering = false;

        // Re-enable the player's movement script
        if (playerMovementScript != null)
        {
            playerMovementScript.enabled = true;
        }

        // Switch back to the player's camera
        if (playerCamera != null)
        {
            playerCamera.enabled = true;
        }
        if (steeringCamera != null)
        {
            steeringCamera.enabled = false;
        }

        // Reset the local rotation when exiting steering mode.
        steeringObject.localRotation = Quaternion.identity;
    }
}
