using UnityEngine;
using System.Collections.Generic;
using TMPro;
using UnityEngine.SceneManagement;

public class GameManager : MonoBehaviour
{
    // === Public Variables ===
    public GameObject waterPlane;
    public float maxWaterHeight = 5f;
    private int maxPatchesHeld; // Now a private variable, value set from GameSettingsManager
    public int patchesHeldByPlayer = 0; // The current number of patches the player has

    // Public property to expose maxPatchesHeld to other scripts
    public int MaxPatchesHeld { get { return maxPatchesHeld; } }

    // Public reference to the AudioSource for the leak sound
    [Header("Audio")]
    public AudioSource leakSound;

    [Header("Initial Game Delay")]
    [Tooltip("The time in seconds before the main game timer and spawning begins.")]
    public float initialDelayTime = 5f;
    private float currentInitialDelayTime;
    private bool initialTimerIsComplete = false;

    [Header("Level Time")]
    private float currentLevelTime; // Now stores the value from GameSettingsManager

    [Header("UI Panel References")]
    public GameObject objectDetailsPanel;
    public GameObject gameTimerPanel;
    public GameObject endGameMessagePanel;

    [Header("Player Controller Reference")]
    public FirstPersonController playerController;

    [Header("Spawner References")]
    public List<IndividualWaterSpawner> spawners;
    public ObstacleSpawner obstacleSpawner; // Reference to the Obstacle Spawner

    [Header("Obstacle Hit Settings")]
    [Tooltip("Multiplier for water rise rate when the ship is hit by an obstacle. (e.g., 1.2 for 20% increase)")]
    public float obstacleHitWaterRiseRateMultiplier = 1.2f;

    public List<GameObject> activeLeaks = new List<GameObject>();

    // NEW: Warning Lights References
    [Header("Warning Lights")]
    [Tooltip("Parent GameObject of the Caution light (for leaks). Should have SpinningLightEffect script.")]
    public GameObject cautionLightParent;
    private SpinningLightEffect cautionLightEffect; // Cached component

    [Tooltip("Parent GameObject of the Warning light (for obstacles). Should have SpinningLightEffect script.")]
    public GameObject warningLightParent;
    private SpinningLightEffect warningLightEffect; // Cached component

    // === Private Variables ===
    private TextMeshProUGUI leaksText;
    private TextMeshProUGUI patchesText;
    private TextMeshProUGUI timerText;
    private TextMeshProUGUI messageText;

    private bool gameOver = false;
    private bool timeUp = false;
    private float currentWaterHeight;
    private float maxWaterLevel;

    // Variables that will store the settings from GameSettingsManager
    // These are now visible in the Inspector using [SerializeField]
    [SerializeField] private float actualWaterRiseRate; // Stores the converted decimal value
    [SerializeField] private float actualMinLeakSpawnTime;
    [SerializeField] private float actualMaxLeakSpawnTime;
    private float actualObstacleSpeed;
    private float actualDisplacementRange;
    [SerializeField] private float actualMinShipObstacleSpawnInterval;
    [SerializeField] private float actualMaxShipObstacleSpawnInterval;
    [SerializeField] private bool actualEnableSteeringAndObstacles;

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

        // Cache SpinningLightEffect components
        if (cautionLightParent != null)
        {
            cautionLightEffect = cautionLightParent.GetComponent<SpinningLightEffect>();
            if (cautionLightEffect == null) Debug.LogWarning("Caution Light Parent does not have a SpinningLightEffect script attached.");
        }
        if (warningLightParent != null)
        {
            warningLightEffect = warningLightParent.GetComponent<SpinningLightEffect>();
            if (warningLightEffect == null) Debug.LogWarning("Warning Light Parent does not have a SpinningLightEffect script attached.");
        }
    }

    void Start()
    {
        initialTimerIsComplete = false;
        UpdateUI();

        if (SceneManager.GetActiveScene().name == "MainMenu" || SceneManager.GetActiveScene().name == "Credits")
        {
            Debug.Log("In Main Menu and Credits Scenes, skip the game start setup.");
            return;
        }

        // --- Retrieve settings from GameSettingsManager ---
        if (GameSettingsManager.Instance != null)
        {
            actualWaterRiseRate = GameSettingsManager.Instance.currentWaterRiseRate / 100f;
            actualMinLeakSpawnTime = GameSettingsManager.Instance.currentMinLeakSpawnTime;
            actualMaxLeakSpawnTime = GameSettingsManager.Instance.currentMaxLeakSpawnTime;
            maxPatchesHeld = GameSettingsManager.Instance.currentMaxPatchesHeld;
            patchesHeldByPlayer = maxPatchesHeld;
            currentLevelTime = GameSettingsManager.Instance.currentLevelTime;
            actualObstacleSpeed = GameSettingsManager.Instance.currentObstacleSpeed;
            actualDisplacementRange = GameSettingsManager.Instance.currentDisplacementRange;
            actualMinShipObstacleSpawnInterval = GameSettingsManager.Instance.currentMinShipObstacleSpawnInterval;
            actualMaxShipObstacleSpawnInterval = GameSettingsManager.Instance.currentMaxShipObstacleSpawnInterval;
            actualEnableSteeringAndObstacles = GameSettingsManager.Instance.currentEnableSteeringAndObstacles;

            foreach (var spawner in spawners)
            {
                if (spawner != null)
                {
                    spawner.maxLeaks = GameSettingsManager.Instance.currentMaxLeaksPerSpawner;
                }
            }

            if (playerController != null)
            {
                playerController.walkSpeed *= GameSettingsManager.Instance.customPlayerSpeedMultiplier;
                playerController.sprintSpeed *= GameSettingsManager.Instance.customPlayerSpeedMultiplier;

                SteeringManager steeringManager = playerController.GetComponent<SteeringManager>();
                if (steeringManager != null)
                {
                    steeringManager.enabled = actualEnableSteeringAndObstacles;
                }
                else
                {
                    Debug.LogWarning("SteeringManager component not found on PlayerController. Cannot enable/disable steering.");
                }
            }

            if (obstacleSpawner != null)
            {
                obstacleSpawner.obstacleSpeed = actualObstacleSpeed;
                obstacleSpawner.displacementRange = actualDisplacementRange;
                obstacleSpawner.minShipObstacleSpawnInterval = actualMinShipObstacleSpawnInterval;
                obstacleSpawner.maxShipObstacleSpawnInterval = actualMaxShipObstacleSpawnInterval;
                obstacleSpawner.enabled = actualEnableSteeringAndObstacles;
                Debug.Log($"GameManager: Obstacle Spawner settings updated - Speed: {actualObstacleSpeed}, Range: {actualDisplacementRange}, Min Spawn: {actualMinShipObstacleSpawnInterval}, Max Spawn: {actualMaxShipObstacleSpawnInterval}, Enabled: {actualEnableSteeringAndObstacles}");
            }

            Debug.Log($"GameManager: Applied settings - Water Rate: {GameSettingsManager.Instance.currentWaterRiseRate}% ({actualWaterRiseRate:F2}), Patches: {maxPatchesHeld}, Min Leak Time: {actualMinLeakSpawnTime}, Level Time: {currentLevelTime}, Steering/Obstacles Enabled: {actualEnableSteeringAndObstacles}");
        }
        else
        {
            Debug.LogError("GameSettingsManager not found! Using default GameManager settings.");
            actualWaterRiseRate = 0.01f;
            actualMinLeakSpawnTime = 5f;
            actualMaxLeakSpawnTime = 15f;
            maxPatchesHeld = 10;
            patchesHeldByPlayer = maxPatchesHeld;
            currentLevelTime = 60f;
            actualObstacleSpeed = 5f;
            actualDisplacementRange = 10f;
            actualMinShipObstacleSpawnInterval = 2f;
            actualMaxShipObstacleSpawnInterval = 5f;
            actualEnableSteeringAndObstacles = true;
        }

        if (waterPlane == null)
        {
            Debug.LogError("Water Plane is not assigned in GameManager.");
            return;
        }
        currentWaterHeight = waterPlane.transform.position.y;
        maxWaterLevel = currentWaterHeight + maxWaterHeight;

        // Initialize lights to off (call the SpinningLightEffect's Stop method)
        if (cautionLightEffect != null) cautionLightEffect.StopSpinAndLight();
        if (warningLightEffect != null) warningLightEffect.StopSpinAndLight();
    }

    void Update()
    {
        if (gameOver) return;

        if (!initialTimerIsComplete)
        {
            currentInitialDelayTime -= Time.deltaTime;
            if (currentInitialDelayTime <= 0f)
            {
                currentInitialDelayTime = 0f;
                initialTimerIsComplete = true;
                StartGame();
            }
        }
        else // Once the initial timer is complete, run the main game logic
        {
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
                // Manage Caution Light for leaks: Start spinning and enable child lights
                if (cautionLightEffect != null)
                {
                    // Call StartSpinAndLight, which also activates its lightContainerChild
                    cautionLightEffect.StartSpinAndLight();
                }

                float waterLevelIncrease = actualWaterRiseRate * activeLeaks.Count * Time.deltaTime;
                currentWaterHeight += waterLevelIncrease;
                waterPlane.transform.position = new Vector3(waterPlane.transform.position.x, currentWaterHeight, waterPlane.transform.position.z);

                if (currentWaterHeight >= maxWaterLevel)
                {
                    EndGame(false);
                }
            }
            else // No active leaks
            {
                // Manage Caution Light for leaks: Stop spinning and disable child lights
                if (cautionLightEffect != null)
                {
                    // Call StopSpinAndLight, which also deactivates its lightContainerChild
                    cautionLightEffect.StopSpinAndLight();
                }
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
            if (!initialTimerIsComplete)
            {
                int seconds = Mathf.CeilToInt(currentInitialDelayTime);
                timerText.text = "Game Starts in: " + seconds;
            }
            else
            {
                int minutes = Mathf.FloorToInt(currentLevelTime / 60);
                int seconds = Mathf.FloorToInt(currentLevelTime % 60);
                timerText.text = string.Format("Time Until Help Arrives: {0:00}:{1:00}", minutes, seconds);
            }
        }
    }

    public void StartGame()
    {
        foreach (var spawner in spawners)
        {
            if (spawner != null)
            {
                spawner.StartSpawning();
            }
        }
    }

    public void AddLeak(GameObject leak)
    {
        activeLeaks.Add(leak);
        Debug.Log("Leak added. Total leaks: " + activeLeaks.Count);

        // Caution light logic is now in Update() to continuously check activeLeaks.Count
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
            foreach (var spawner in spawners)
            {
                if (spawner != null)
                {
                    spawner.OnLastLeakPatched();
                }
            }
        }
        // Caution light logic is now in Update() to continuously check activeLeaks.Count
    }

    // NEW: Method for YachtCollisionSensor to notify about obstacles
    public void NotifyObstacleInFront(bool detected)
    {
        if (warningLightEffect == null)
        {
            Debug.LogWarning("Warning Light Effect not assigned or found. Cannot toggle warning light.");
            return;
        }

        if (detected)
        {
            // Call StartSpinAndLight, which also activates its lightContainerChild
            warningLightEffect.StartSpinAndLight();
            Debug.Log("Warning light ON: Obstacle detected.");
        }
        else
        {
            // Call StopSpinAndLight, which also deactivates its lightContainerChild
            warningLightEffect.StopSpinAndLight();
            Debug.Log("Warning light OFF: No obstacle detected.");
        }
    }


    public void ApplyObstacleHitPenalty()
    {
        actualWaterRiseRate *= obstacleHitWaterRiseRateMultiplier;
        Debug.Log($"Ship hit by obstacle! Water rise rate increased to: {actualWaterRiseRate:F2}");
    }

    public float GetNextLeakSpawnInterval()
    {
        return Random.Range(actualMinLeakSpawnTime, actualMaxLeakSpawnTime);
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

        // Ensure lights are off at game end (call the SpinningLightEffect's Stop method)
        if (cautionLightEffect != null) cautionLightEffect.StopSpinAndLight();
        if (warningLightEffect != null) warningLightEffect.StopSpinAndLight();
    }

    public void DecreasePatches()
    {
        patchesHeldByPlayer--;
    }

    private void ReturnToMainMenu()
    {
        Time.timeScale = 1f;
        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;
        SceneManager.LoadScene("MainMenu");
    }

    public void LoadScene(string sceneName)
    {
        Time.timeScale = 1f;
        Cursor.lockState = CursorLockMode.None;
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
