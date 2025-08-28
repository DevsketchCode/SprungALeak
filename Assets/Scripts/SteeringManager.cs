using UnityEngine;

public class SteeringManager : MonoBehaviour
{
    public Transform steeringObject;
    public Transform captainPosition;
    public float steeringSpeed = 5f;
    public float maxSteeringRotation = 45f;
    public ObstacleSpawner obstacleSpawner;
    public MonoBehaviour playerMovementScript;

    private bool isSteering = false;
    private bool isPlayerInRange = false;
    private Vector3 originalPlayerPosition;
    private Quaternion originalPlayerRotation;

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

    void EnterSteeringMode()
    {
        isSteering = true;
        originalPlayerPosition = transform.position;
        originalPlayerRotation = transform.rotation;

        transform.position = captainPosition.position;
        transform.rotation = captainPosition.rotation;

        if (playerMovementScript != null)
        {
            playerMovementScript.enabled = false;
        }
    }

    void ExitSteeringMode()
    {
        isSteering = false;
        if (playerMovementScript != null)
        {
            playerMovementScript.enabled = true;
        }

        transform.position = originalPlayerPosition;
        transform.rotation = originalPlayerRotation;

        // Reset the local rotation when exiting steering mode.
        steeringObject.localRotation = Quaternion.identity;
    }
}