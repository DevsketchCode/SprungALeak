using UnityEngine;

/// <summary>
/// Defines an object that can be interacted with (e.g., picked up, opened).
/// Attach this script to the GameObject the player can pick up (like the FlareGun).
/// </summary>
public class InteractableItem : MonoBehaviour
{
    [Tooltip("The text displayed on the HUD when the player looks at this item.")]
    public string interactionText = "Pick up Item";
    public GameObject handheldGameObject = null;

    /// <summary>
    /// This method is called by the PlayerInteraction script when the player
    /// successfully interacts (e.g., presses 'E' while looking at the item).
    /// </summary>
    /// <param name="playerRef">A reference to the Player's root GameObject 
    /// where the FlareGun component will be enabled.</param>
    public void Interact(GameObject playerRef)
    {
        Debug.Log($"Player interacting with: {gameObject.name}");

        // --- 1. Perform the specific action (Enabling the FlareGun on the Player) ---

        if (handheldGameObject != null)
        {
            // Enable the FlareGun functionality on the player.
            // Note: You must have a script named 'FlareGun' in your project for this to work.
            handheldGameObject.SetActive(true);
            Debug.Log("FlareGun component enabled on the Player.");
        }
        else
        {
            Debug.LogError("FlareGun component not found on the player reference!");
        }

        // --- 2. Clean up the pickup GameObject ---

        // Note: For simple pickups, we don't need to disable the mesh/collider 
        // separately because we destroy the whole object right away.
        // Destroy(gameObject);

        // Disable the GameObject holding this pickup model.
        gameObject.SetActive(false);
    }
}
