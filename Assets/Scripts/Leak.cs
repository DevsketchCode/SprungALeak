using UnityEngine;

public class Leak : MonoBehaviour
{
    // === Public Variables ===
    public GameObject patchObject;

    // === Private Variables ===
    private GameManager gameManager;
    private AudioManager audioManager;
    private bool playerIsNearby = false;
    private bool isPatched = false; // Flag to prevent multiple patches

    void Start()
    {
        // Find the GameManager object in the scene
        gameManager = FindObjectOfType<GameManager>();
        if (gameManager == null)
        {
            Debug.LogError("GameManager not found in the scene.");
        }

        // Find the AudioManager object in the scene
        audioManager = FindObjectOfType<AudioManager>();
        if (audioManager == null)
        {
            Debug.LogError("AudioManager not found in the scene.");
        }
    }

    void Update()
    {
        // Only check for a click if the leak is not already patched, the player is nearby, and they have patches
        if (!isPatched && playerIsNearby && Input.GetMouseButtonDown(0) && gameManager != null && gameManager.patchesHeldByPlayer > 0)
        {
            PatchLeak();
        }
    }

    public void PatchLeak()
    {
        // Set the flag to true immediately to prevent subsequent calls
        if (isPatched) return;
        isPatched = true;

        if (gameManager != null)
        {
            if (audioManager != null && audioManager.patchSound != null)
            {
                audioManager.PlayPatchSound();
                Debug.Log("Patch sound: " + audioManager.patchSound.ToString() + " should have played.");
            }
            gameManager.DecreasePatches();
            gameManager.RemoveLeak(gameObject);
        }

        // Instantiate the patched object and destroy the leak object
        if (patchObject != null)
        {
            Instantiate(patchObject, transform.position, transform.rotation);
        }
        else
        {
            Debug.LogWarning("PatchObject is not assigned to the Leak script.");
        }

        Destroy(gameObject);
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            playerIsNearby = true;
            if (gameManager != null && gameManager.patchesHeldByPlayer > 0)
            {
                Debug.Log("Player is near a leak. Click to patch.");
            }
        }
    }

    private void OnTriggerExit(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            playerIsNearby = false;
            Debug.Log("Player has left the leak area.");
        }
    }
}