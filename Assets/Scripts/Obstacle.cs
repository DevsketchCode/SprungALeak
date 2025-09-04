using UnityEngine;

public class Obstacle : MonoBehaviour
{
    private Vector3 moveDirection;
    private float speed;

    // Reference to the ObstacleSpawner that created this obstacle
    private ObstacleSpawner parentSpawner;

    // Reference to the YachtCollisionSensor to notify when destroyed
    private YachtCollisionSensor yachtCollisionSensor;

    // Initialization method to be called by the spawner.
    // This fixes the 'parentSpawner is null' error in Awake().
    public void Initialize(ObstacleSpawner spawner, Vector3 direction, float moveSpeed, YachtCollisionSensor sensor)
    {
        parentSpawner = spawner;
        moveDirection = direction;
        speed = moveSpeed;
        yachtCollisionSensor = sensor;
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

    // This is the critical method that was missing. It's called automatically by Unity
    // just before the object is destroyed.
    void OnDestroy()
    {
        if (yachtCollisionSensor != null)
        {
            yachtCollisionSensor.DecrementObstacleCount();
        }
    }
}
