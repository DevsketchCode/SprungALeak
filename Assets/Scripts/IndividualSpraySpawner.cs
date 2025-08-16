using UnityEngine;

public class IndividualWaterSpawner : MonoBehaviour
{
    // === Public Variables ===
    public GameObject holePrefab;
    public Vector3 holeRotationOffset = new Vector3(0, 0, 0);
    public Vector3 faceNormalFilter = new Vector3(0, 0, 1);

    // === Private Variables ===
    private Transform playerTransform;
    private float nextSpawnInterval; // The duration to wait (now fetched from GameManager)
    private float timeSinceLastSpawn; // Counts time since the last spawn
    private Collider wallCollider;
    private GameManager gameManager;

    void Start()
    {
        playerTransform = GameObject.FindWithTag("Player")?.transform;
        wallCollider = GetComponent<Collider>();
        gameManager = FindObjectOfType<GameManager>();

        if (playerTransform == null || wallCollider == null || gameManager == null)
        {
            Debug.LogError("Missing required components. Check tags, colliders, and GameManager.");
            enabled = false;
            return;
        }

        // Initialize the first spawn interval from GameManager
        nextSpawnInterval = gameManager.GetNextLeakSpawnInterval();
        timeSinceLastSpawn = 0f;
    }

    void Update()
    {
        // Stop spawning if the game is over or time has run out
        if (gameManager != null && (gameManager.IsGameOver() || gameManager.IsTimeUp()))
        {
            return;
        }

        // Increment the timer
        timeSinceLastSpawn += Time.deltaTime;

        // Check if the timer has reached the interval
        if (timeSinceLastSpawn >= nextSpawnInterval)
        {
            SpawnNewLeak();
            // Reset the timer and set a new interval for the next spawn
            timeSinceLastSpawn = 0f;
            nextSpawnInterval = gameManager.GetNextLeakSpawnInterval(); // Get new interval from GameManager
        }
    }

    private void SpawnNewLeak()
    {
        Vector3 spawnPoint = Vector3.zero;
        Vector3 surfaceNormal = Vector3.zero;
        bool foundValidSpot = false;

        int attempts = 0;
        const int maxAttempts = 30;

        while (!foundValidSpot && attempts < maxAttempts)
        {
            Bounds bounds = wallCollider.bounds;
            Vector3 minPos = bounds.min + faceNormalFilter * 0.1f;
            Vector3 maxPos = bounds.max + faceNormalFilter * 0.1f;

            Vector3 randomRayOrigin = new Vector3(
                Random.Range(minPos.x, maxPos.x),
                Random.Range(minPos.y, maxPos.y),
                Random.Range(minPos.z, maxPos.z)
            );

            RaycastHit hit;
            if (Physics.Raycast(randomRayOrigin, -faceNormalFilter, out hit, 1f))
            {
                if (hit.collider == wallCollider && Vector3.Dot(hit.normal, faceNormalFilter.normalized) > 0.5f) // Using 0.5f from previous fix
                {
                    spawnPoint = hit.point;
                    surfaceNormal = hit.normal;
                    foundValidSpot = true;
                }
            }
            attempts++;
        }

        if (foundValidSpot)
        {
            Quaternion baseRotation = Quaternion.LookRotation(surfaceNormal);
            Quaternion finalRotation = baseRotation * Quaternion.Euler(holeRotationOffset);
            GameObject newHole = Instantiate(holePrefab, spawnPoint, finalRotation, wallCollider.transform);

            ParticleSystem ps = newHole.GetComponentInChildren<ParticleSystem>();
            if (ps != null)
            {
                ps.Play();
                SetParticleRenderQueue(ps);
            }

            gameManager.AddLeak(newHole);
            Debug.Log("Hole spawned successfully!"); // Keep this for debugging
        }
        else
        {
            Debug.LogWarning("Failed to find a valid spawning spot after " + maxAttempts + " attempts."); // Keep this for debugging
        }
    }

    private void SetParticleRenderQueue(ParticleSystem ps)
    {
        var renderer = ps.GetComponent<Renderer>();
        if (renderer != null && renderer.material != null)
        {
            renderer.material.renderQueue = 3001;
        }
        else
        {
            Debug.LogWarning("ParticleSystem Renderer or Material not found for setting Render Queue.");
        }
    }
}