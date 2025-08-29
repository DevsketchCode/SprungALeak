using UnityEngine;

public class Obstacle : MonoBehaviour
{
    private Vector3 moveDirection;
    private float speed;
    private GameManager gameManager; // Reference to the GameManager

    // Reference to the ObstacleSpawner that created this obstacle
    public ObstacleSpawner parentSpawner;

    // NEW: Reference to the YachtCollisionSensor to notify when destroyed
    public YachtCollisionSensor yachtCollisionSensor;

    void Awake()
    {
        gameManager = FindObjectOfType<GameManager>();
        if (gameManager == null)
        {
            Debug.LogError("GameManager not found in scene. Obstacle cannot apply penalties.");
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

    // Detects when this obstacle collides with another object
    void OnCollisionEnter(Collision collision)
    {
        // Debug logs for comprehensive analysis
        Debug.Log($"Obstacle collision detected by {gameObject.name} with {collision.gameObject.name}. Its root: {collision.gameObject.transform.root.gameObject.name}. Root Tag: {collision.gameObject.transform.root.gameObject.tag}");
        Debug.Log($"Obstacle: Checking against parentSpawner's target: {(parentSpawner != null && parentSpawner.shipColliders != null ? parentSpawner.shipColliders.name : "NULL/UNASSIGNED")}");


        // Check if we hit the player's ship collision target (obtained from the spawner)
        // Also checking if the root object of the collision has the "ShipColliders" tag.
        if (parentSpawner != null && parentSpawner.shipColliders != null &&
            collision.gameObject.transform.root.gameObject == parentSpawner.shipColliders &&
            collision.gameObject.transform.root.CompareTag("ShipColliders"))
        {
            Debug.Log($">>> Obstacle {gameObject.name} hit the Player's Ship! Notifying spawner. <<<");

            // Notify the parent spawner to handle the hit (applies penalty, shake, destroys obstacle)
            // The actual Destroy(gameObject) call will happen inside ObstacleSpawner.HandleObstacleHit
            parentSpawner.HandleObstacleHit(this, collision.gameObject);

            // Crucially, notify the YachtCollisionSensor that this obstacle is being destroyed
            if (yachtCollisionSensor != null)
            {
                yachtCollisionSensor.DecrementObstacleCount();
                Debug.Log($"Obstacle {gameObject.name} notified YachtCollisionSensor before destruction.");
            }
            else
            {
                Debug.LogWarning($"Obstacle {gameObject.name}: YachtCollisionSensor reference missing, cannot decrement count on destruction.");
            }
        }
        else
        {
            // Debugging for ignored collisions
            if (parentSpawner != null && parentSpawner.shipColliders != null)
            {
                Debug.Log($"Collision condition NOT met for {gameObject.name}. Root of hit object: {collision.gameObject.transform.root.gameObject.name} (Tag: {collision.gameObject.transform.root.gameObject.tag}) vs Expected Ship Target: {parentSpawner.shipColliders.name}. Tag check passed: {collision.gameObject.transform.root.CompareTag("ShipColliders")}");
            }
            else
            {
                Debug.LogWarning("Obstacle's parentSpawner or shipColliders is NULL during collision check. This obstacle might not have been correctly initialized by the spawner.");
            }
        }
    }
}
