using UnityEngine;
using System.Collections.Generic; // Added for List

public class IndividualWaterSpawner : MonoBehaviour
{
    // === Public Variables ===
    public GameObject holePrefab;

    // Controls the overall rotation of the spawned holes relative to the surface normal
    public float maxRandomZRotation = 360f; // Maximum random Z-axis rotation for the hole (0-360 degrees)

    // Filters valid surfaces for spawning based on their normal direction
    public Vector3 faceNormalFilter = new Vector3(0, 0, 1);

    // Minimum distance between the center of a new leak and any existing leak
    public float minSpawnDistance = 1.5f;

    // === Private Variables ===
    private Transform playerTransform;
    private float nextSpawnInterval;    // The duration to wait (now fetched from GameManager)
    private float timeSinceLastSpawn;   // Counts time since the last spawn
    private Collider wallCollider;      // Collider of the object holes spawn on
    private GameManager gameManager;    // Reference to the GameManager
    private float timeOfLastPatch;      // Tracks the time the last leak was patched

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
        timeOfLastPatch = Time.time; // Initialize with the current time
    }

    void Update()
    {
        // Stop spawning if the game is over or time has run out
        if (gameManager != null && (gameManager.IsGameOver() || gameManager.IsTimeUp()))
        {
            return;
        }

        // Only increment the timer if there are no leaks AND a sufficient interval has passed since the last one was patched.
        // Or if there are active leaks, just keep ticking the timer.
        if (gameManager.activeLeaks.Count == 0 && Time.time - timeOfLastPatch >= 1f)
        {
            timeSinceLastSpawn += Time.deltaTime;
        }
        else if (gameManager.activeLeaks.Count > 0)
        {
            timeSinceLastSpawn += Time.deltaTime;
        }

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
        const int maxAttempts = 50; // Increased max attempts to find a valid spot

        while (!foundValidSpot && attempts < maxAttempts)
        {
            Bounds bounds = wallCollider.bounds;
            // Slightly offset the ray origin to be inside the collider to ensure it hits a surface
            // Using a random point within the bounds for ray origin
            Vector3 randomRayOrigin = new Vector3(
                Random.Range(bounds.min.x, bounds.max.x),
                Random.Range(bounds.min.y, bounds.max.y),
                Random.Range(bounds.min.z, bounds.max.z)
            );

            // Determine ray direction based on the faceNormalFilter
            // We want to cast from inside the volume outwards if faceNormalFilter points outwards,
            // or inwards if faceNormalFilter points inwards relative to the bounding box.
            Vector3 rayDirection = faceNormalFilter.normalized;

            // To ensure the ray always hits an inner surface, we need to cast towards the center
            // or from outside the bounds towards the surface if the filter is consistently facing one direction.
            // For general plane spawning, casting from slightly outside towards the center is robust.
            // Let's adjust randomRayOrigin to be slightly outside the bounds, then cast inwards.

            // Move the ray origin slightly outside the bounds in the direction opposite to the filter normal
            randomRayOrigin += rayDirection * (bounds.extents.magnitude + 0.1f);
            rayDirection = -rayDirection; // Cast inwards

            RaycastHit hit;
            // Cast a ray from slightly outside the collider towards the inside
            if (Physics.Raycast(randomRayOrigin, rayDirection, out hit, bounds.extents.magnitude * 2))
            {
                // Ensure the ray hit our desired wall and its normal aligns with the filter
                if (hit.collider == wallCollider && Vector3.Dot(hit.normal, faceNormalFilter.normalized) > 0.5f)
                {
                    // Check if the proposed spawn point is too close to other leaks
                    if (!IsSpotTooClose(hit.point))
                    {
                        spawnPoint = hit.point;
                        surfaceNormal = hit.normal;
                        foundValidSpot = true;
                    }
                }
            }
            attempts++;
        }

        if (foundValidSpot)
        {
            // Calculate base rotation to align with the surface normal
            Quaternion baseRotation = Quaternion.LookRotation(surfaceNormal);
            // Apply a random Z-axis rotation on top of the base rotation
            Quaternion randomZRotation = Quaternion.Euler(0, 0, Random.Range(0f, maxRandomZRotation));
            Quaternion finalRotation = baseRotation * randomZRotation;

            GameObject newHole = Instantiate(holePrefab, spawnPoint, finalRotation, wallCollider.transform);

            if (AudioManager.instance != null)
            {
                AudioManager.instance.PlayLeakSpawnSound();
            }

            ParticleSystem ps = newHole.GetComponentInChildren<ParticleSystem>();
            if (ps != null)
            {
                ps.Play();
                SetParticleRenderQueue(ps);
            }

            gameManager.AddLeak(newHole);
            Debug.Log("Hole spawned successfully at: " + spawnPoint);
        }
        else
        {
            Debug.LogWarning("Failed to find a valid spawning spot after " + maxAttempts + " attempts.");
        }
    }

    private bool IsSpotTooClose(Vector3 spot)
    {
        // Check against all active leaks in the game manager
        foreach (GameObject leak in gameManager.activeLeaks)
        {
            if (Vector3.Distance(spot, leak.transform.position) < minSpawnDistance)
            {
                return true; // Too close to an existing leak
            }
        }
        return false; // Not too close to any existing leak
    }

    private void SetParticleRenderQueue(ParticleSystem ps)
    {
        var renderer = ps.GetComponent<Renderer>();
        if (renderer != null && renderer.material != null)
        {
            renderer.material.renderQueue = 3001; // Ensure particles render correctly over other transparent objects
        }
        else
        {
            Debug.LogWarning("ParticleSystem Renderer or Material not found for setting Render Queue.");
        }
    }

    // New method to be called from GameManager when the last leak is patched
    public void OnLastLeakPatched()
    {
        timeSinceLastSpawn = 0f;
        nextSpawnInterval = gameManager.GetNextLeakSpawnInterval();
        timeOfLastPatch = Time.time; // Reset the time of the last patch as well
    }
}