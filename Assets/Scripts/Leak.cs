using UnityEngine;

public class Leak : MonoBehaviour
{
    public GameObject patchPrefab;

    private GameManager gameManager;
    private bool playerIsNearby = false;

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
        // Only check for a click if the player is nearby and has patches
        if (playerIsNearby && Input.GetMouseButtonDown(0) && gameManager.totalPatches > 0)
        {
            PatchLeak();
        }
    }

    private void PatchLeak()
    {
        GameObject newPatch = Instantiate(patchPrefab, transform.position, transform.rotation, transform.parent);

        gameManager.RemoveLeak(gameObject);
        gameManager.DecreasePatches(); // Call the new method to decrease patches

        Destroy(gameObject);
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            playerIsNearby = true;
            Debug.Log("Player is near a leak.");
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