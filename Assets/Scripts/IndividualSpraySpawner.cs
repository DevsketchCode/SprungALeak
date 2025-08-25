using System.Collections.Generic;
using UnityEngine;

public class IndividualWaterSpawner : MonoBehaviour
{
    // === Public Variables ===
    public GameObject leakPrefab;
    public GameManager gameManager;

    [Header("Spawn Zones")]
    [Tooltip("Drag all of your spawn zone GameObjects into this list.")]
    public List<Transform> leakSpawnZones;
    public int maxLeaks = 10;

    [Header("Leak Section Reference")]
    [Tooltip("The invisible object where holes will be spawned. Drag the Hull_LeakSection here.")]
    public Transform leakSectionTransform;

    [Header("Raycast Spawning")]
    [Tooltip("The layer of the leak section object. Essential for raycasting.")]
    public LayerMask leakSectionLayer;
    [Tooltip("The number of attempts to find a valid spawn point via raycast.")]
    public int raycastAttempts = 10;

    // === Private Variables ===
    private float nextLeakSpawnTime;
    private bool isSpawningAllowed = false;

    void Start()
    {
        if (gameManager == null)
        {
            gameManager = FindObjectOfType<GameManager>();
            if (gameManager == null)
            {
                Debug.LogError("GameManager not found in the scene.");
            }
        }

        if (leakSpawnZones == null || leakSpawnZones.Count == 0)
        {
            Debug.LogError("Leak Spawn Zones list is empty. Please assign at least one.");
            return;
        }

        if (leakSectionTransform == null)
        {
            Debug.LogError("Leak Section Transform is not assigned. Cannot spawn leaks.");
            return;
        }

        SetNextLeakSpawnTime();
    }

    void Update()
    {
        if (isSpawningAllowed && Time.time >= nextLeakSpawnTime)
        {
            SpawnNewLeak();
            SetNextLeakSpawnTime();
        }
    }

    public void StartSpawning()
    {
        isSpawningAllowed = true;
    }

    private void SetNextLeakSpawnTime()
    {
        nextLeakSpawnTime = Time.time + gameManager.GetNextLeakSpawnInterval();
    }

    public void OnLastLeakPatched()
    {
        isSpawningAllowed = true;
        SetNextLeakSpawnTime();
    }

    public void SpawnNewLeak()
    {
        if (gameManager.activeLeaks.Count >= maxLeaks)
        {
            isSpawningAllowed = false;
            return;
        }

        Vector3 spawnPoint;
        Quaternion spawnRotation;
        if (TrySpawnLeakOnSurface(out spawnPoint, out spawnRotation))
        {
            GameObject newLeak = Instantiate(leakPrefab, spawnPoint, spawnRotation);
            gameManager.AddLeak(newLeak);

            Debug.Log("Hole spawned successfully at: " + newLeak.transform.position);
        }
    }

    private bool TrySpawnLeakOnSurface(out Vector3 spawnPosition, out Quaternion spawnRotation)
    {
        Transform selectedZone = leakSpawnZones[Random.Range(0, leakSpawnZones.Count)];
        Collider zoneCollider = selectedZone.GetComponent<Collider>();

        if (zoneCollider == null)
        {
            Debug.LogError("Selected spawn zone has no Collider component. Cannot raycast.");
            spawnPosition = Vector3.zero;
            spawnRotation = Quaternion.identity;
            return false;
        }

        for (int i = 0; i < raycastAttempts; i++)
        {
            Vector3 randomPoint = GetRandomPointOnColliderSurface(zoneCollider);

            Ray ray = new Ray(randomPoint, (leakSectionTransform.position - randomPoint).normalized);
            RaycastHit hit;

            if (Physics.Raycast(ray, out hit, 100f, leakSectionLayer))
            {
                spawnPosition = hit.point;

                // Use the hit normal from the leak section to get a perfectly flat rotation
                // Since the normals are already pointing inward, we use them directly.
                spawnRotation = Quaternion.LookRotation(hit.normal);

                return true;
            }
        }

        spawnPosition = Vector3.zero;
        spawnRotation = Quaternion.identity;
        Debug.LogWarning("Failed to find a valid spawn point after " + raycastAttempts + " attempts.");
        return false;
    }

    private Vector3 GetRandomPointOnColliderSurface(Collider collider)
    {
        Bounds bounds = collider.bounds;
        Vector3 point = Vector3.zero;
        int side = Random.Range(0, 6);

        if (side == 0) point = new Vector3(bounds.min.x, Random.Range(bounds.min.y, bounds.max.y), Random.Range(bounds.min.z, bounds.max.z));
        else if (side == 1) point = new Vector3(bounds.max.x, Random.Range(bounds.min.y, bounds.max.y), Random.Range(bounds.min.z, bounds.max.z));
        else if (side == 2) point = new Vector3(Random.Range(bounds.min.x, bounds.max.x), bounds.min.y, Random.Range(bounds.min.z, bounds.max.z));
        else if (side == 3) point = new Vector3(Random.Range(bounds.min.x, bounds.max.x), bounds.max.y, Random.Range(bounds.min.z, bounds.max.z));
        else if (side == 4) point = new Vector3(Random.Range(bounds.min.x, bounds.max.x), Random.Range(bounds.min.y, bounds.max.y), bounds.min.z);
        else point = new Vector3(Random.Range(bounds.min.x, bounds.max.x), Random.Range(bounds.min.y, bounds.max.y), bounds.max.z);

        return point;
    }
}