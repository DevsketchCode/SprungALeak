using UnityEngine;
using System.Collections; // Required for Coroutines

public class YachtCollisionSensor : MonoBehaviour
{
    [Tooltip("Reference to the GameManager to notify about obstacles.")]
    public GameManager gameManager;

    private int obstaclesInTrigger = 0; // Counter for obstacles currently inside the trigger

    void Start()
    {
        if (gameManager == null)
        {
            gameManager = FindObjectOfType<GameManager>();
            if (gameManager == null)
            {
                Debug.LogError("YachtCollisionSensor: GameManager not found in scene.");
            }
        }

        // Ensure this GameObject has a collider and it's set to Is Trigger
        Collider col = GetComponent<Collider>();
        if (col == null)
        {
            Debug.LogError($"YachtCollisionSensor: GameObject '{gameObject.name}' needs a Collider component set to 'Is Trigger'.");
            enabled = false; // Disable script if no collider
        }
        else if (!col.isTrigger)
        {
            Debug.LogWarning($"YachtCollisionSensor: Collider on '{gameObject.name}' is not set to 'Is Trigger'. Setting it automatically.");
            col.isTrigger = true;
        }
    }

    void OnTriggerEnter(Collider other)
    {
        // Only interested if it's an Obstacle
        if (other.CompareTag("Obstacle"))
        {
            obstaclesInTrigger++; // Increment the counter
            Debug.Log($"YachtCollisionSensor: Obstacle entered. Total in trigger: {obstaclesInTrigger}");

            // If this is the first obstacle, turn on the warning light
            if (obstaclesInTrigger == 1)
            {
                if (gameManager != null)
                {
                    gameManager.NotifyObstacleInFront(true); // Notify GameManager an obstacle is detected
                }
            }
        }
    }

    void OnTriggerExit(Collider other)
    {
        // Only interested if it's an Obstacle leaving naturally (not being destroyed)
        if (other.CompareTag("Obstacle"))
        {
            DecrementObstacleCount(); // Use the new method for consistent logic
            Debug.Log($"YachtCollisionSensor: Obstacle exited naturally. Total in trigger: {obstaclesInTrigger}");
        }
    }

    /// <summary>
    /// Decrements the count of obstacles in the trigger. This can be called externally
    /// (e.g., when an obstacle is destroyed) or internally (from OnTriggerExit).
    /// </summary>
    public void DecrementObstacleCount()
    {
        obstaclesInTrigger--; // Decrement the counter
        obstaclesInTrigger = Mathf.Max(0, obstaclesInTrigger); // Ensure it doesn't go below zero

        // If the last obstacle has left or been destroyed, turn off the warning light
        if (obstaclesInTrigger == 0)
        {
            if (gameManager != null)
            {
                gameManager.NotifyObstacleInFront(false); // Notify GameManager no more obstacles
            }
        }
    }
}
