using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI; // Required for UI elements like Slider, Toggle, InputField
using TMPro; // Required for TextMeshProUGUI and TMP_InputField

public class CustomizationMenu : MonoBehaviour
{
    [Header("UI References")]
    public Slider waterRiseRateSlider;
    public TMP_InputField minLeakSpawnTimeInput; // Using TextMeshPro Input Field
    public TMP_InputField maxLeakSpawnTimeInput;
    public TMP_InputField maxLeaksPerSpawnerInput;
    public TMP_InputField maxPatchesHeldInput;
    public Slider playerSpeedMultiplierSlider;
    public Slider obstacleSpeedSlider;
    public Slider displacementRangeSlider;
    public TMP_InputField levelTimeInput;
    public Toggle enableSteeringAndObstaclesToggle;
    public TMP_InputField minShipObstacleSpawnIntervalInput; // Min Spawn Interval for Obstacle Spawner
    public TMP_InputField maxShipObstacleSpawnIntervalInput; // Max Spawn Interval for Obstacle Spawner
    public Button backButton;

    [Header("Scene Names")]
    public string titleSceneName = "TitleSceneName"; // Replace with your actual title scene name

    void Start()
    {
        // Ensure GameSettingsManager exists and initialize if it doesn't
        if (GameSettingsManager.Instance == null)
        {
            GameObject settingsManagerGO = new GameObject("GameSettingsManager");
            settingsManagerGO.AddComponent<GameSettingsManager>();
        }

        // Load current settings into UI elements
        if (GameSettingsManager.Instance != null)
        {
            // Multiply by 100f to display as percentage on the slider (e.g., 1-15)
            waterRiseRateSlider.value = GameSettingsManager.Instance.currentWaterRiseRate;
            minLeakSpawnTimeInput.text = GameSettingsManager.Instance.currentMinLeakSpawnTime.ToString("F1");
            maxLeakSpawnTimeInput.text = GameSettingsManager.Instance.currentMaxLeakSpawnTime.ToString("F1");
            maxLeaksPerSpawnerInput.text = GameSettingsManager.Instance.currentMaxLeaksPerSpawner.ToString();
            maxPatchesHeldInput.text = GameSettingsManager.Instance.currentMaxPatchesHeld.ToString();
            playerSpeedMultiplierSlider.value = GameSettingsManager.Instance.customPlayerSpeedMultiplier;
            obstacleSpeedSlider.value = GameSettingsManager.Instance.currentObstacleSpeed;
            displacementRangeSlider.value = GameSettingsManager.Instance.currentDisplacementRange;
            levelTimeInput.text = GameSettingsManager.Instance.currentLevelTime.ToString("F0");
            enableSteeringAndObstaclesToggle.isOn = GameSettingsManager.Instance.currentEnableSteeringAndObstacles;
            minShipObstacleSpawnIntervalInput.text = GameSettingsManager.Instance.currentMinShipObstacleSpawnInterval.ToString("F1"); // NEW
            maxShipObstacleSpawnIntervalInput.text = GameSettingsManager.Instance.currentMaxShipObstacleSpawnInterval.ToString("F1"); // NEW
        }

        // Add listeners to update settings on UI change
        waterRiseRateSlider.onValueChanged.AddListener(SetWaterRiseRate);
        minLeakSpawnTimeInput.onEndEdit.AddListener(SetMinLeakSpawnTime);
        maxLeakSpawnTimeInput.onEndEdit.AddListener(SetMaxLeakSpawnTime);
        maxLeaksPerSpawnerInput.onEndEdit.AddListener(SetMaxLeaksPerSpawner);
        maxPatchesHeldInput.onEndEdit.AddListener(SetMaxPatchesHeld);
        playerSpeedMultiplierSlider.onValueChanged.AddListener(SetPlayerSpeedMultiplier);
        obstacleSpeedSlider.onValueChanged.AddListener(SetObstacleSpeed);
        displacementRangeSlider.onValueChanged.AddListener(SetDisplacementRange);
        levelTimeInput.onEndEdit.AddListener(SetLevelTime);
        enableSteeringAndObstaclesToggle.onValueChanged.AddListener(SetEnableSteeringAndObstacles);
        minShipObstacleSpawnIntervalInput.onEndEdit.AddListener(SetMinShipObstacleSpawnInterval); 
        maxShipObstacleSpawnIntervalInput.onEndEdit.AddListener(SetMaxShipObstacleSpawnInterval); 
        backButton.onClick.AddListener(GoToTitleScreen);
    }

    // Setter methods for custom settings
    public void SetWaterRiseRate(float value)
    {
        // NEW: Send the raw percentage value (e.g., 1-15) to GameSettingsManager
        GameSettingsManager.Instance?.SetCustomWaterRiseRate(value);
    }
    public void SetMinLeakSpawnTime(string value)
    {
        if (float.TryParse(value, out float result)) GameSettingsManager.Instance?.SetCustomMinLeakSpawnTime(result);
    }
    public void SetMaxLeakSpawnTime(string value)
    {
        if (float.TryParse(value, out float result)) GameSettingsManager.Instance?.SetCustomMaxLeakSpawnTime(result);
    }
    public void SetMaxLeaksPerSpawner(string value)
    {
        if (int.TryParse(value, out int result)) GameSettingsManager.Instance?.SetCustomMaxLeaksPerSpawner(result);
    }
    public void SetMaxPatchesHeld(string value)
    {
        if (int.TryParse(value, out int result)) GameSettingsManager.Instance?.SetCustomMaxPatchesHeld(result);
    }
    public void SetPlayerSpeedMultiplier(float value)
    {
        GameSettingsManager.Instance?.SetCustomPlayerSpeedMultiplier(value);
    }
    public void SetObstacleSpeed(float value)
    {
        GameSettingsManager.Instance?.SetCustomObstacleSpeed(value);
    }
    public void SetDisplacementRange(float value)
    {
        GameSettingsManager.Instance?.SetCustomDisplacementRange(value);
    }
    public void SetLevelTime(string value)
    {
        if (float.TryParse(value, out float result)) GameSettingsManager.Instance?.SetCustomLevelTime(result);
    }
    public void SetEnableSteeringAndObstacles(bool value)
    {
        GameSettingsManager.Instance?.SetCustomEnableSteeringAndObstacles(value);
    }
    public void SetMinShipObstacleSpawnInterval(string value) // NEW
    {
        if (float.TryParse(value, out float result)) GameSettingsManager.Instance?.SetCustomMinShipObstacleSpawnInterval(result);
    }
    public void SetMaxShipObstacleSpawnInterval(string value) // NEW
    {
        if (float.TryParse(value, out float result)) GameSettingsManager.Instance?.SetCustomMaxShipObstacleSpawnInterval(result);
    }

    public void GoToTitleScreen()
    {
        SceneManager.LoadScene(titleSceneName);
    }
}
