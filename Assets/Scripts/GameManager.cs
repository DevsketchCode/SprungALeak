using UnityEngine;
using System.Collections.Generic;
using TMPro;

public class GameManager : MonoBehaviour
{
    // === Public Variables ===
    public GameObject waterPlane;
    public float maxWaterHeight = 5f;
    public float waterRiseRate = 0.01f;
    public int totalPatches = 5;

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
    public FirstPersonController playerController; // Drag your Player Controller here

    // === Private Variables ===
    private TextMeshProUGUI leaksText;
    private TextMeshProUGUI patchesText;
    private TextMeshProUGUI timerText;
    private TextMeshProUGUI messageText;

    private List<GameObject> activeLeaks = new List<GameObject>();
    private bool gameOver = false;
    private bool timeUp = false;

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
    }

    void Update()
    {
        if (gameOver) return;

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
            float riseAmount = waterRiseRate * activeLeaks.Count * Time.deltaTime;
            waterPlane.transform.localScale += new Vector3(0, riseAmount, 0);

            if (waterPlane.transform.localScale.y >= maxWaterHeight)
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
            patchesText.text = "Patches Left: " + totalPatches;
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
    }

    public void RemoveLeak(GameObject leak)
    {
        activeLeaks.Remove(leak);
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

        // Hide UI panels
        if (objectDetailsPanel != null) objectDetailsPanel.SetActive(false);
        if (gameTimerPanel != null) gameTimerPanel.SetActive(false);

        // Show end game message panel
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

        // --- New code to disable the player controller ---
        if (playerController != null)
        {
            playerController.cameraCanMove = false;
            playerController.playerCanMove = false;
        }

        Cursor.lockState = CursorLockMode.None; // Release the cursor
        Time.timeScale = 0f;
    }

    public void DecreasePatches()
    {
        totalPatches--;
    }
}