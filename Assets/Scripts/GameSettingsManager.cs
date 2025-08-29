using UnityEngine;
using System.Collections.Generic;

// Enum to define different difficulty levels
public enum DifficultyLevel
{
    Easy,
    Normal,
    Hard,
    Custom // For when player customizes settings individually
}

// Struct to hold a set of difficulty parameters
[System.Serializable]
public struct DifficultyPreset
{
    public DifficultyLevel level;
    [Header("Level Time (seconds)")]
    public float levelTime;
    [Header("Player Settings")]
    public int maxPatchesHeld;
    [Header("Flood Settings")]
    public float waterRiseRate;
    [Header("Leak Settings")]
    public float minLeakSpawnTime;
    public float maxLeakSpawnTime;
    public int maxLeaksPerSpawner; // Max leaks for each individual spawner
    [Header("Ship Obstacle Settings")]
    public float obstacleSpeed;
    public float displacementRange;
    public float minShipObstacleSpawnInterval;
    public float maxShipObstacleSpawnInterval;
    public bool enableSteeringAndObstacles; //Enable/Disable steering and obstacle spawning
}

public class GameSettingsManager : MonoBehaviour
{
    // Singleton instance
    public static GameSettingsManager Instance { get; private set; }

    [Header("Difficulty Presets")]
    public DifficultyPreset easySettings;
    public DifficultyPreset normalSettings;
    public DifficultyPreset hardSettings;

    // Current active difficulty level
    public DifficultyLevel currentDifficulty = DifficultyLevel.Normal;

    [Header("Current Game Settings (Read-Only in Play Mode)")]
    // These will be the actual values used in the game,
    // which can be set by presets or customized by the player.
    public float currentLevelTime;
    public int currentMaxPatchesHeld;
    public float currentWaterRiseRate;
    public float currentMinLeakSpawnTime;
    public float currentMaxLeakSpawnTime;
    public int currentMaxLeaksPerSpawner;
    public float currentObstacleSpeed;
    public float currentDisplacementRange;
    public float currentMinShipObstacleSpawnInterval;
    public float currentMaxShipObstacleSpawnInterval;
    public bool currentEnableSteeringAndObstacles;

    // Add variables for other custom settings here (e.g., player speed, obstacle speed)
    [Header("Customizable Settings")]
    public float customPlayerSpeedMultiplier = 1.0f;

    void Awake()
    {
        // Ensure only one instance of GameSettingsManager exists
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
        }
        else
        {
            Instance = this;
            DontDestroyOnLoad(gameObject); // Keep this object alive between scenes
            ApplyPresetSettings(currentDifficulty); // Apply default difficulty on start
        }
    }

    // Applies settings based on the selected difficulty preset
    public void ApplyPresetSettings(DifficultyLevel level)
    {
        currentDifficulty = level;
        DifficultyPreset selectedPreset;

        switch (level)
        {
            case DifficultyLevel.Easy:
                selectedPreset = easySettings;
                break;
            case DifficultyLevel.Hard:
                selectedPreset = hardSettings;
                break;
            case DifficultyLevel.Normal:
            default: // Default to Normal if not specified
                selectedPreset = normalSettings;
                break;
            case DifficultyLevel.Custom:
                // For custom, the values are already set via customization scene or other means
                // No need to apply a preset, just ensure currentDifficulty is set to Custom
                return;
        }

        currentLevelTime = selectedPreset.levelTime;
        currentMaxPatchesHeld = selectedPreset.maxPatchesHeld;
        currentWaterRiseRate = selectedPreset.waterRiseRate;
        currentMinLeakSpawnTime = selectedPreset.minLeakSpawnTime;
        currentMaxLeakSpawnTime = selectedPreset.maxLeakSpawnTime;
        currentMaxLeaksPerSpawner = selectedPreset.maxLeaksPerSpawner;
        currentObstacleSpeed = selectedPreset.obstacleSpeed;
        currentDisplacementRange = selectedPreset.displacementRange;
        currentMinShipObstacleSpawnInterval = selectedPreset.minShipObstacleSpawnInterval;
        currentMaxShipObstacleSpawnInterval = selectedPreset.maxShipObstacleSpawnInterval;
        currentEnableSteeringAndObstacles = selectedPreset.enableSteeringAndObstacles;

        Debug.Log($"Applied {currentDifficulty} difficulty settings.");
    }

    // Method to allow external scripts (like CustomizationMenu) to update individual settings
    public void SetCustomLevelTime(float value)
    {
        currentLevelTime = value;
        currentDifficulty = DifficultyLevel.Custom;
    }
    public void SetCustomMaxPatchesHeld(int value)
    {
        currentMaxPatchesHeld = value;
        currentDifficulty = DifficultyLevel.Custom;
    }
    public void SetCustomWaterRiseRate(float value)
    {
        currentWaterRiseRate = value;
        currentDifficulty = DifficultyLevel.Custom;
    }
    public void SetCustomMinLeakSpawnTime(float value)
    {
        currentMinLeakSpawnTime = value;
        currentDifficulty = DifficultyLevel.Custom;
    }
    public void SetCustomMaxLeakSpawnTime(float value)
    {
        currentMaxLeakSpawnTime = value;
        currentDifficulty = DifficultyLevel.Custom;
    }
    public void SetCustomMaxLeaksPerSpawner(int value)
    {
        currentMaxLeaksPerSpawner = value;
        currentDifficulty = DifficultyLevel.Custom;
    }
    public void SetCustomObstacleSpeed(float value)
    {
        currentObstacleSpeed = value;
        currentDifficulty = DifficultyLevel.Custom;
    }
    public void SetCustomDisplacementRange(float value)
    {
        currentDisplacementRange = value;
        currentDifficulty = DifficultyLevel.Custom;
    }
    public void SetCustomMinShipObstacleSpawnInterval(float value)
    {
        currentMinShipObstacleSpawnInterval = value;
        currentDifficulty = DifficultyLevel.Custom;
    }
    public void SetCustomMaxShipObstacleSpawnInterval(float value)
    {
        currentMaxShipObstacleSpawnInterval = value;
        currentDifficulty = DifficultyLevel.Custom;
    }
    public void SetCustomEnableSteeringAndObstacles(bool value)
    {
        currentEnableSteeringAndObstacles = value;
        currentDifficulty = DifficultyLevel.Custom;
    }
    public void SetCustomPlayerSpeedMultiplier(float value)
    {
        customPlayerSpeedMultiplier = value;
        currentDifficulty = DifficultyLevel.Custom;
    }
}
