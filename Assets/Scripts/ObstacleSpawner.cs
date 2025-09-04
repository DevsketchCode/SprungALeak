using System.Collections.Generic;
using UnityEngine;

public class ObstacleSpawner : MonoBehaviour
{
    [Header("Obstacle Prefabs")]
    [Tooltip("Drag all the obstacle prefabs you want to spawn into this list.")]
    public List<GameObject> obstaclePrefabs;

    [Tooltip("Used for obstacle rotation around the ship")]
    public Transform shipTransform; // 

    [Header("Player Collision Target")]
    [Tooltip("Drag the GameObject that is the parent of your ship's colliders (e.g., 'Yacht_Colliders') here.")]
    public GameObject shipColliders;

    // Reference to the YachtCollisionSensor in the scene
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

    [Header("Audio Manager")]
    [Tooltip("Audio Manager for access to Sound Effects")]
    public AudioManager audioManager;

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

        // Get reference to the AudioManager once
        audioManager = FindObjectOfType<AudioManager>();
        if (audioManager == null)
        {
            Debug.LogError("AudioManager not found in scene for ObstacleSpawner.");
        }
    }

    void Start()
    {
        if (shipColliders == null)
        {
            Debug.LogError("Player Collision Target (shipColliders) is not assigned in the ObstacleSpawner Inspector! Obstacles will not be able to detect hits properly.");
        }

        // Ensure that there is at least one obstacle prefab in the list
        if (obstaclePrefabs == null || obstaclePrefabs.Count == 0)
        {
            Debug.LogError("Obstacle Prefabs list is empty! No obstacles will be spawned.");
            this.enabled = false; // Disable the spawner
            return;
        }

        // Find or ensure YachtCollisionSensor reference
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
        // Check if the list is not empty before attempting to spawn
        if (obstaclePrefabs.Count == 0)
        {
            Debug.LogWarning("Cannot spawn obstacle, the prefab list is empty.");
            return;
        }

        // Calculate random displacement on the X-axis
        float randomOffset = Random.Range(-displacementRange, displacementRange);
        Vector3 spawnOffset = displacementAxis.normalized * randomOffset;
        Vector3 spawnPosition = transform.position + spawnOffset;

        // Randomly select a prefab from the list
        int randomIndex = Random.Range(0, obstaclePrefabs.Count);
        GameObject selectedPrefab = obstaclePrefabs[randomIndex];

        // Instantiate and set properties
        GameObject newObstacle = Instantiate(selectedPrefab, spawnPosition, Quaternion.identity);
        activeObstacles.Add(newObstacle);

        // Get the Obstacle script on the new object
        Obstacle obstacleScript = newObstacle.GetComponent<Obstacle>();
        if (obstacleScript != null)
        {
            // Use the Initialize method to set all properties at once
            // This fixes the 'parentSpawner is null' error in Obstacle's Awake()
            obstacleScript.Initialize(this, Vector3.back, obstacleSpeed, yachtCollisionSensor);
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
        // Check if the obstacle already collided
        if (!activeObstacles.Contains(hitObstacle.gameObject))
        {
            Debug.Log($"ObstacleSpawner: Already handled collision for {hitObstacle.name}. Ignoring.");
            return;
        }

        // Confirm the collided object is indeed part of our shipColliders target
        if (shipColliders != null && collidedWithGameObject.transform.root.gameObject == shipColliders)
        {
            Debug.Log("ObstacleSpawner: Detected hit on Player's Ship! Applying effects.");

            // 1. Play Collision Sound
            if (audioManager.collisionSoundSource != null)
            {
                audioManager.PlayCollisionSound();
                //Debug.Log("Playing collision sound for collided obstacle: " + audioManager.collisionSoundSource.isPlaying + "collionsoundsource: " + audioManager.collisionSoundSource);
            }
            else
            {
                Debug.LogWarning("ObstacleSpawner: AudioManager's collisionSoundSource is NULL, cannot play sound.");
            }

            // 2. Trigger Ship Shake Effect
            // Find the currently active camera to get the shake effect.
            Camera activeCamera = GetActiveCamera();
            if (activeCamera != null)
            {
                ShipCollisionShakeEffect shakeEffect = activeCamera.GetComponent<ShipCollisionShakeEffect>();
                if (shakeEffect != null)
                {
                    shakeEffect.Shake();
                    Debug.Log("ObstacleSpawner: Ship shake effect triggered.");
                }
                else
                {
                    Debug.LogWarning("ObstacleSpawner: ShipCollisionShakeEffect component not found on the active camera. Cannot trigger shake.");
                }
            }
            else
            {
                Debug.LogWarning("ObstacleSpawner: No active camera found in the scene. Cannot trigger shake.");
            }

            // 3. Apply Water Rise Rate Penalty
            if (gameManager != null)
            {
                gameManager.ApplyObstacleHitPenalty();
                Debug.Log("ObstacleSpawner: Water rise penalty applied.");
            }
            else
            {
                Debug.LogWarning("ObstacleSpawner: GameManager is NULL, cannot apply penalty.");
            }

            // 4. Destroy the obstacle and remove from active list
            activeObstacles.Remove(hitObstacle.gameObject); // Remove from our tracking list
            Destroy(hitObstacle.gameObject, 0.1f); // Destroy the actual GameObject with a tiny delay for audio
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

    // Find the currently active camera in the scene.
    private Camera GetActiveCamera()
    {
        Camera[] allCameras = FindObjectsOfType<Camera>();
        foreach (Camera cam in allCameras)
        {
            if (cam.enabled)
            {
                return cam;
            }
        }
        return null;
    }
}
