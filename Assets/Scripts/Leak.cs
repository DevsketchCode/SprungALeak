using UnityEngine;

public class Leak : MonoBehaviour
{
    // Public reference for the patch prefab
    public GameObject patchPrefab;

    // Public reference for the cracking sound source
    public AudioSource crackSoundSource;

    private GameManager gameManager;
    private bool playerIsNearby = false;

    void Start()
    {
        gameManager = FindObjectOfType<GameManager>();
        if (gameManager == null)
        {
            Debug.LogError("GameManager not found in scene.");
        }

        // Play the cracking sound one time when the leak is created
        if (crackSoundSource != null)
        {
            crackSoundSource.Play();
        }
    }

    void Update()
    {
        // Only check for a click if the player is nearby and has patches
        if (playerIsNearby && Input.GetMouseButtonDown(0) && gameManager.patchesHeldByPlayer > 0)
        {
            PatchLeak();
        }
    }

    private void PatchLeak()
    {
        GameObject newPatch = Instantiate(patchPrefab, transform.position, transform.rotation, transform.parent);

        // No audio call here. The GameManager handles the water flow sound.

        gameManager.RemoveLeak(gameObject);
        gameManager.DecreasePatches();

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