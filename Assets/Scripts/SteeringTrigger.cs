using UnityEngine;

public class SteeringTrigger : MonoBehaviour
{
    // A public reference to the steering manager on the player
    public SteeringManager steeringManager;

    // Called when another collider enters the trigger
    private void OnTriggerEnter(Collider other)
    {
        // Check if the entering object is the player
        if (other.CompareTag("Player"))
        {
            // Tell the steering manager that the player is in range
            steeringManager.SetPlayerInRange(true);
        }
    }

    // Called when another collider exits the trigger
    private void OnTriggerExit(Collider other)
    {
        // Check if the exiting object is the player
        if (other.CompareTag("Player"))
        {
            // Tell the steering manager that the player is out of range
            steeringManager.SetPlayerInRange(false);
        }
    }
}