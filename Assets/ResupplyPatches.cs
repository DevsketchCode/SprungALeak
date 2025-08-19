using UnityEngine;

public class ResupplyPatches : MonoBehaviour
{
    private GameManager gameManager;
    private bool playerIsNearby = false;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        gameManager = FindObjectOfType<GameManager>();
        if (gameManager == null)
        {
            Debug.LogError("GameManager not found in scene.");
        }
    }

    void Update()
    {
        // Only check for a click if the player is nearby and patch inventory is not already full
        if (playerIsNearby && Input.GetMouseButtonDown(0) && gameManager.patchesHeldByPlayer < gameManager.maxPatchesHeld)
        {
            gameManager.patchesHeldByPlayer = gameManager.maxPatchesHeld;
        }
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            playerIsNearby = true;
            Debug.Log("Player is near the patchbox.");
        }
    }

    private void OnTriggerExit(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            playerIsNearby = false;
            Debug.Log("Player has left the patchbox area.");
        }
    }
}
