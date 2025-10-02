using UnityEngine;
using System.Collections; // Not strictly needed here, but often useful

/// <summary>
/// Handles player interaction logic using raycasting from the center of the screen.
/// It checks for InteractableItem components within range and triggers them.
/// </summary>
public class PlayerInteraction : MonoBehaviour
{
    [Tooltip("The maximum distance from the camera an item can be to be interacted with.")]
    public float interactionDistance = 3.0f;

    // You would typically link a UI element here to display interaction prompts.
    // [SerializeField] private Text interactTextUI; 

    // A reference to the root player object. This is often necessary to pass 
    // to the item being picked up (e.g., to enable a FlareGun component on the player).
    private GameObject playerRoot;

    void Start()
    {
        // Get the top-level parent object, which is usually the main Player object.
        playerRoot = transform.root.gameObject;

        if (playerRoot == null)
        {
            Debug.LogError("PlayerInteraction could not find a root player GameObject.");
        }
    }

    void Update()
    {
        // 1. Define the ray from the center of the camera's viewport
        Ray ray = new Ray(transform.position, transform.forward);
        RaycastHit hit;

        // Reset UI text (if using a dedicated UI element)
        // if (interactTextUI != null)
        // {
        //     interactTextUI.text = "";
        // }

        // 2. Perform the raycast check (pointing at item AND within distance)
        if (Physics.Raycast(ray, out hit, interactionDistance))
        {
            // 3. Check if the object hit has an InteractableItem component
            InteractableItem interactable = hit.collider.GetComponent<InteractableItem>();

            if (interactable != null)
            {
                // Player is looking at an item they can interact with

                // Display prompt (using Debug.Log as a simple visual feedback)
                Debug.Log($"[Can Interact] Looking at: {interactable.gameObject.name}. Press 'E' to {interactable.interactionText}.");

                // Update UI prompt
                // if (interactTextUI != null)
                // {
                //     interactTextUI.text = $"Press E to {interactable.interactionText}";
                // }

                // 4. Check for the Interaction input (e.g., 'E' key)
                if (Input.GetKeyDown(KeyCode.E))
                {
                    // Call the Interact method on the item, passing the player reference
                    interactable.Interact(playerRoot);

                    // Clear the prompt immediately after interaction
                    // if (interactTextUI != null)
                    // {
                    //     interactTextUI.text = "";
                    // }
                } 
                else if (Input.GetKeyDown(KeyCode.D))
                {
                    // Drop item logic could go here
                }
            }
        }
    }
}
