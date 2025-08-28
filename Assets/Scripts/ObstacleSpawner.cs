using System.Collections.Generic;
using UnityEngine;

public class ObstacleSpawner : MonoBehaviour
{
    public GameObject obstaclePrefab;
    public float minSpawnInterval = 2f;
    public float maxSpawnInterval = 5f;
    public float obstacleSpeed = 5f;
    public float displacementRange = 10f;

    [Tooltip("The axis for random displacement. Set to (1, 0, 0) for horizontal.")]
    public Vector3 displacementAxis = Vector3.right;

    private List<GameObject> activeObstacles = new List<GameObject>();
    private float nextSpawnTime;

    void Start()
    {
        SetNextSpawnTime();
    }

    void Update()
    {
        if (Time.time >= nextSpawnTime)
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
        }
    }

    public void ApplySteering(float steeringAmount)
    {
        // Invert the steering amount for the obstacles
        steeringAmount *= -1;

        // Move all active obstacles left or right
        foreach (GameObject obstacle in activeObstacles)
        {
            if (obstacle != null)
            {
                obstacle.transform.position += Vector3.right * steeringAmount * Time.deltaTime;
            }
        }
    }

    void SetNextSpawnTime()
    {
        nextSpawnTime = Time.time + Random.Range(minSpawnInterval, maxSpawnInterval);
    }
}