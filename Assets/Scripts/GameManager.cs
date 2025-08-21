using UnityEngine;
using System.Collections.Generic;
using TMPro;
using UnityEngine.SceneManagement;

public class GameManager : MonoBehaviour
{
    // === Public Variables ===
    public GameObject waterPlane;
    public float maxWaterHeight = 5f;
    public float waterRiseRate = 0.01f;
    public int maxPatchesHeld = 10;
    public int patchesHeldByPlayer = 0;

    // Public reference to the AudioSource for the leak sound
    [Header("Audio")]
    public AudioSource leakSound;

    [Header("Level Timer")]
    public float levelTime = 60f;
    private float currentLevelTime;

    [Header("Leak Spawn Times (Centralized)")]
    public float minLeakSpawnTime = 5f;
    public float maxLeakSpawnTime = 15f;

    [Header("UI Panel References")]
    public GameObject objectDetailsPanel;
    public GameObject gameTimerPanel;
    public GameObject endGameMessagePanel;

    [Header("Player Controller Reference")]
    public FirstPersonController playerController;

    // --- UPDATED Public Variable ---
    [Header("Spawner References")]
    public List<IndividualWaterSpawner> spawners; // Changed from a single reference to a list

    public List<GameObject> activeLeaks = new List<GameObject>();

    // === Private Variables ===
    private TextMeshProUGUI leaksText;
    private TextMeshProUGUI patchesText;
    private TextMeshProUGUI timerText;
    private TextMeshProUGUI messageText;

    private bool gameOver = false;
    private bool timeUp = false;
    private float currentWaterHeight;
    private float maxWaterLevel;

    void Awake()
    {
        if (objectDetailsPanel != null)
        {
            TextMeshProUGUI[] texts = objectDetailsPanel.GetComponentsInChildren<TextMeshProUGUI>();
            if (texts.Length > 0) leaksText = texts[0];
            if (texts.Length > 1) patchesText = texts[1];
        }

        if (gameTimerPanel != null)
        {
            timerText = gameTimerPanel.GetComponentInChildren<TextMeshProUGUI>();
        }

        if (endGameMessagePanel != null)
        {
            messageText = endGameMessagePanel.GetComponentInChildren<TextMeshProUGUI>();
        }
    }

    void Start()
    {
        currentLevelTime = levelTime;
        UpdateUI();

        if(SceneManager.GetActiveScene().name == "MainMenu" || SceneManager.GetActiveScene().name == "Credits")
        {
            Debug.Log("In Main Menu and Credits Scenes, skip the game start setup.");
            return;
        }

        if (waterPlane == null)
        {
            Debug.LogError("Water Plane is not assigned in GameManager.");
            return;
        }
        currentWaterHeight = waterPlane.transform.position.y;
        maxWaterLevel = currentWaterHeight + maxWaterHeight;
    }

    void Update()
    {
        if (gameOver) return;

        // --- NEW: Check for Escape key to return to Main Menu ---
        //if (Input.GetKeyDown(KeyCode.Escape))
        //{
        //    ReturnToMainMenu();
        //}

        if (!timeUp)
        {
            currentLevelTime -= Time.deltaTime;
            if (currentLevelTime <= 0f)
            {
                currentLevelTime = 0f;
                timeUp = true;
                EndGame(true);
            }
        }

        if (activeLeaks.Count > 0)
        {
            float waterLevelIncrease = waterRiseRate * activeLeaks.Count * Time.deltaTime;
            currentWaterHeight += waterLevelIncrease;
            waterPlane.transform.position = new Vector3(waterPlane.transform.position.x, currentWaterHeight, waterPlane.transform.position.z);

            if (currentWaterHeight >= maxWaterLevel)
            {
                EndGame(false);
            }
        }

        UpdateUI();
    }

    private void UpdateUI()
    {
        if (leaksText != null)
        {
            leaksText.text = "Active Leaks: " + activeLeaks.Count;
        }
        if (patchesText != null)
        {
            patchesText.text = "Patches Left: " + patchesHeldByPlayer;
        }
        if (timerText != null)
        {
            int minutes = Mathf.FloorToInt(currentLevelTime / 60);
            int seconds = Mathf.FloorToInt(currentLevelTime % 60);
            timerText.text = string.Format("Time Until Help Arrives: {0:00}:{1:00}", minutes, seconds);
        }
    }

    public void AddLeak(GameObject leak)
    {
        activeLeaks.Add(leak);
        Debug.Log("Leak added. Total leaks: " + activeLeaks.Count);

        if (activeLeaks.Count == 1)
        {
            if (leakSound != null && !leakSound.isPlaying)
            {
                leakSound.Play();
            }
        }
    }

    public void RemoveLeak(GameObject leak)
    {
        activeLeaks.Remove(leak);
        Debug.Log("Leak removed. Total leaks: " + activeLeaks.Count);

        if (activeLeaks.Count == 0)
        {
            Debug.Log("No active leaks left. Stopping leak sound: " + leakSound.isPlaying);
            if (leakSound != null && leakSound.isPlaying)
            {
                Debug.Log("Stopping leak sound.");
                leakSound.Stop();
                Debug.Log("Leak Sound should be stopped: " + leakSound.isPlaying);
            }
            // --- UPDATED Code ---
            // Notify ALL spawners to reset their timers
            foreach (var spawner in spawners)
            {
                if (spawner != null)
                {
                    spawner.OnLastLeakPatched();
                }
            }
        }
    }

    public float GetNextLeakSpawnInterval()
    {
        return Random.Range(minLeakSpawnTime, maxLeakSpawnTime);
    }

    public bool IsGameOver()
    {
        return gameOver;
    }

    public bool IsTimeUp()
    {
        return timeUp;
    }

    public void EndGame(bool survivedTime)
    {
        if (gameOver) return;
        gameOver = true;

        if (leakSound != null && leakSound.isPlaying)
        {
            leakSound.Stop();
        }

        if (objectDetailsPanel != null) objectDetailsPanel.SetActive(false);
        if (gameTimerPanel != null) gameTimerPanel.SetActive(false);

        if (endGameMessagePanel != null) endGameMessagePanel.SetActive(true);

        if (messageText != null)
        {
            if (survivedTime)
            {
                messageText.text = "You survived! The Coast Guard is on its way.";
                messageText.color = Color.green;
            }
            else
            {
                messageText.text = "Game Over. You took on too much water, the boat sank.";
                messageText.color = Color.red;
            }
        }

        if (playerController != null)
        {
            playerController.cameraCanMove = false;
            playerController.playerCanMove = false;
        }

        Cursor.lockState = CursorLockMode.None;
        Time.timeScale = 0f;
    }

    public void DecreasePatches()
    {
        patchesHeldByPlayer--;
    }

    // --- NEW: Method for stopping sounds and returning to the main menu ---
    private void ReturnToMainMenu()
    {
        // Return to the MainMenu scene
        Time.timeScale = 1f;
        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;
        SceneManager.LoadScene("MainMenu");
    }

    public void LoadScene(string sceneName)
    {
        Time.timeScale = 1f;
        SceneManager.LoadScene(sceneName);
    }

    public void QuitGame()
    {
#if UNITY_EDITOR
        UnityEditor.EditorApplication.isPlaying = false;
#else
        Application.Quit();
#endif
    }
}