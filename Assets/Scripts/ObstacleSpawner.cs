using System.Collections.Generic;
using UnityEngine;

public class ObstacleSpawner : MonoBehaviour
{
    public GameObject obstaclePrefab;
    public Transform shipTransform; // Used for obstacle rotation around the ship

    [Header("Player Collision Target")]
    [Tooltip("Drag the GameObject that is the parent of your ship's colliders (e.g., 'Yacht_Colliders') here.")]
    public GameObject shipColliders;

    // NEW: Reference to the YachtCollisionSensor in the scene
    [Header("Collision Sensor")]
    [Tooltip("Drag the GameObject with the YachtCollisionSensor script here.")]
    public YachtCollisionSensor yachtCollisionSensor;

    // These public fields will be set by the GameManager from GameSettingsManager
    public float minShipObstacleSpawnInterval = 2f;
    public float maxShipObstacleSpawnInterval = 5f;
    public float obstacleSpeed = 5f;
    public float displacementRange = 10f;

    [Tooltip("The axis for random displacement. Set to (1, 0, 0) for horizontal.")]
    public Vector3 displacementAxis = Vector3.right;

    private List<GameObject> activeObstacles = new List<GameObject>();
    private float nextSpawnTime;

    private GameManager gameManager; // Reference to the GameManager for penalties

    void Awake()
    {
        // Get reference to the GameManager once
        gameManager = FindObjectOfType<GameManager>();
        if (gameManager == null)
        {
            Debug.LogError("GameManager not found in scene for ObstacleSpawner.");
        }
    }

    void Start()
    {
        if (shipColliders == null)
        {
            Debug.LogError("Player Collision Target (shipColliders) is not assigned in the ObstacleSpawner Inspector! Obstacles will not be able to detect hits properly.");
        }

        // NEW: Find or ensure YachtCollisionSensor reference
        if (yachtCollisionSensor == null)
        {
            yachtCollisionSensor = FindObjectOfType<YachtCollisionSensor>();
            if (yachtCollisionSensor == null)
            {
                Debug.LogError("YachtCollisionSensor not assigned or found in scene. Obstacles will not be able to notify sensor on destruction.");
            }
        }

        SetNextSpawnTime();
    }

    void Update()
    {
        // Only spawn if this spawner is enabled (controlled by GameSettingsManager via GameManager)
        if (this.enabled && Time.time >= nextSpawnTime)
        {
            SpawnObstacle();
            SetNextSpawnTime();
        }
    }

    void SpawnObstacle()
    {
        // Calculate random displacement on the X-axis
        float randomOffset = Random.Range(-displacementRange, displacementRange);
        Vector3 spawnOffset = displacementAxis.normalized * randomOffset;
        Vector3 spawnPosition = transform.position + spawnOffset;

        // Instantiate and set properties
        GameObject newObstacle = Instantiate(obstaclePrefab, spawnPosition, Quaternion.identity);
        activeObstacles.Add(newObstacle);

        // Get the Obstacle script on the new object
        Obstacle obstacleScript = newObstacle.GetComponent<Obstacle>();
        if (obstacleScript != null)
        {
            // Set the direction to be Vector3.back to move towards the player
            obstacleScript.SetProperties(Vector3.back, obstacleSpeed);

            // Pass a reference to THIS ObstacleSpawner to the spawned Obstacle
            obstacleScript.parentSpawner = this;

            // NEW: Pass the YachtCollisionSensor reference to the spawned Obstacle
            obstacleScript.yachtCollisionSensor = yachtCollisionSensor;
        }
    }

    public void ApplySteering(float steeringAmount)
    {
        // Invert the steering amount for the obstacles
        steeringAmount *= -1;

        // We now rotate all obstacles around the ship.
        foreach (GameObject obstacle in activeObstacles)
        {
            if (obstacle != null)
            {
                // We rotate the obstacle around the ship's transform.
                obstacle.transform.RotateAround(shipTransform.position, Vector3.up, steeringAmount * Time.deltaTime);
            }
        }
    }

    // Method to handle an obstacle hitting the player's ship
    public void HandleObstacleHit(Obstacle hitObstacle, GameObject collidedWithGameObject)
    {
        // Confirm the collided object is indeed part of our shipColliders target
        if (shipColliders != null && collidedWithGameObject.transform.root.gameObject == shipColliders)
        {
            Debug.Log("ObstacleSpawner: Detected hit on Player's Ship! Applying effects.");

            // 1. Apply Water Rise Rate Penalty
            if (gameManager != null)
            {
                gameManager.ApplyObstacleHitPenalty();
                Debug.Log("ObstacleSpawner: Water rise penalty applied.");
            }
            else
            {
                Debug.LogWarning("ObstacleSpawner: GameManager is NULL, cannot apply penalty.");
            }

            // 2. Trigger Ship Shake Effect
            ShipCollisionShakeEffect shakeEffect = Camera.main.GetComponent<ShipCollisionShakeEffect>();
            if (shakeEffect != null)
            {
                shakeEffect.Shake();
                Debug.Log("ObstacleSpawner: Ship shake effect triggered.");
            }
            else
            {
                Debug.LogWarning("ObstacleSpawner: ShipCollisionShakeEffect component not found on the Main Camera. Cannot trigger shake.");
            }

            // 3. Destroy the obstacle and remove from active list
            activeObstacles.Remove(hitObstacle.gameObject); // Remove from our tracking list
            Destroy(hitObstacle.gameObject); // Destroy the actual GameObject
            Debug.Log("ObstacleSpawner: Obstacle destroyed.");
            // Note: DecrementObstacleCount for YachtCollisionSensor is now handled by Obstacle.cs before destruction.
        }
        else
        {
            Debug.LogWarning($"ObstacleSpawner: HandleObstacleHit called, but collidedWithGameObject ({collidedWithGameObject.name}) is not part of shipColliders target.");
        }
    }


    void SetNextSpawnTime()
    {
        // Use the public minShipObstacleSpawnInterval and maxShipObstacleSpawnInterval which are set by GameManager
        nextSpawnTime = Time.time + Random.Range(minShipObstacleSpawnInterval, maxShipObstacleSpawnInterval);
    }
}
