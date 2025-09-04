using UnityEngine;

public class Obstacle : MonoBehaviour
{
    private Vector3 moveDirection;
    private float speed;

    // Reference to the ObstacleSpawner that created this obstacle
    public ObstacleSpawner parentSpawner;

    // NEW: Reference to the YachtCollisionSensor to notify when destroyed
    public YachtCollisionSensor yachtCollisionSensor;

    void Awake()
    {
        // This is now handled by the Spawner before the object is created.
        // If it's null, we know something went wrong in initialization.
        if (parentSpawner == null)
        {
            Debug.LogError("Obstacle not initialized by a spawner. It will not function correctly.");
        }

        // We find the YachtCollisionSensor on awake
        yachtCollisionSensor = FindObjectOfType<YachtCollisionSensor>();
        if (yachtCollisionSensor == null)
        {
            Debug.LogError("YachtCollisionSensor not found in scene. Obstacle cannot decrement its count.");
        }
    }

    public void SetProperties(Vector3 direction, float moveSpeed)
    {
        moveDirection = direction;
        speed = moveSpeed;
    }

    void Update()
    {
        // The obstacle moves along its initial Z-axis.
        transform.position += moveDirection * speed * Time.deltaTime;
    }

    void OnCollisionEnter(Collision collision)
    {
        // This log helps confirm collision detection is working.
        Debug.Log($"Obstacle collision detected by {gameObject.name} with {collision.gameObject.name}. Its root: {collision.gameObject.transform.root.gameObject.name}.");

        // We only care about collisions with the player's ship.
        // We now check against the tag, which is the most reliable method.
        if (collision.gameObject.transform.root.CompareTag("ShipColliders"))
        {
            Debug.Log($">>> Obstacle {gameObject.name} hit the Player's Ship! Notifying spawner. <<<");

            // Notify the parent spawner to handle the hit.
            if (parentSpawner != null)
            {
                parentSpawner.HandleObstacleHit(this, collision.gameObject);
            }
            else
            {
                // This is a safety check. If we get here, the obstacle wasn't spawned correctly.
                Debug.LogError($"Obstacle {gameObject.name}: parentSpawner is NULL, cannot handle hit.");
            }
        }
        else
        {
            // Optional debug log for when we hit something else.
            Debug.Log($"Obstacle {gameObject.name} collided with an object that is not the player's ship. Ignoring.");
        }
    }
}
