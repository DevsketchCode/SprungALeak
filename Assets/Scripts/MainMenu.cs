using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI; // Required for UI elements
using TMPro; // Required for TextMeshProUGUI

public class TitleMenu : MonoBehaviour
{
    [Header("Scene Names")]
    public string gameSceneName = "YourGameSceneName"; // Replace with your actual game scene name
    public string customizationSceneName = "CustomizationSceneName"; // Replace with your customization scene name

    [Header("UI References")]
    public TextMeshProUGUI difficultyText; // Displays the current difficulty (e.g., "Easy", "Normal")
    public Button leftArrowButton;
    public Button rightArrowButton;
    public TextMeshProUGUI playButtonText; // The TextMeshPro component on your "Play" button
    public Button playButton; // The actual Play/Customize button
    public Button quitButton;

    // Internal list of difficulty levels to cycle through
    private DifficultyLevel[] difficultyLevels = new DifficultyLevel[]
    {
        DifficultyLevel.Easy,
        DifficultyLevel.Normal,
        DifficultyLevel.Hard,
        DifficultyLevel.Custom
    };
    private int currentDifficultyIndex = 1; // Start at Normal (index 1)

    void Start()
    {
        // Ensure GameSettingsManager exists and initialize if it doesn't
        if (GameSettingsManager.Instance == null)
        {
            GameObject settingsManagerGO = new GameObject("GameSettingsManager");
            settingsManagerGO.AddComponent<GameSettingsManager>();
        }

        // Set initial difficulty based on GameSettingsManager, or default to Normal
        if (GameSettingsManager.Instance.currentDifficulty != DifficultyLevel.Custom)
        {
            SetDifficulty(GameSettingsManager.Instance.currentDifficulty); // Uses the last set difficulty
        }
        else
        {
            // If it's custom, find the Custom index
            for (int i = 0; i < difficultyLevels.Length; i++)
            {
                if (difficultyLevels[i] == DifficultyLevel.Custom)
                {
                    currentDifficultyIndex = i;
                    break;
                }
            }
            UpdateDifficultyDisplay(); // Update display for custom
        }

        // Assign button listeners
        if (leftArrowButton != null) leftArrowButton.onClick.AddListener(GoLeft);
        if (rightArrowButton != null) rightArrowButton.onClick.AddListener(GoRight);
        if (playButton != null) playButton.onClick.AddListener(OnPlayOrCustomizeClick);
        if (quitButton != null) quitButton.onClick.AddListener(QuitGame);

        UpdateDifficultyDisplay();
    }

    // Called by UI Buttons (Easy, Normal, Hard) or internally
    public void SetDifficulty(DifficultyLevel level)
    {
        if (GameSettingsManager.Instance != null)
        {
            GameSettingsManager.Instance.ApplyPresetSettings(level);
            Debug.Log($"Difficulty set to: {level}");

            // Update the index to match the set level
            for (int i = 0; i < difficultyLevels.Length; i++)
            {
                if (difficultyLevels[i] == level)
                {
                    currentDifficultyIndex = i;
                    break;
                }
            }
            UpdateDifficultyDisplay();
        }
    }

    // Moves to the previous difficulty level in the array
    public void GoLeft()
    {
        if (currentDifficultyIndex > 0)
        {
            currentDifficultyIndex--;
            SetDifficulty(difficultyLevels[currentDifficultyIndex]);
        }
        UpdateDifficultyDisplay();
    }

    // Moves to the next difficulty level in the array
    public void GoRight()
    {
        if (currentDifficultyIndex < difficultyLevels.Length - 1)
        {
            currentDifficultyIndex++;
            SetDifficulty(difficultyLevels[currentDifficultyIndex]);
        }
        UpdateDifficultyDisplay();
    }

    // Updates the displayed text and button visibility
    void UpdateDifficultyDisplay()
    {
        DifficultyLevel currentSelection = difficultyLevels[currentDifficultyIndex];
        if (difficultyText != null)
        {
            difficultyText.text = currentSelection.ToString();
        }

        // Handle arrow button visibility
        if (leftArrowButton != null)
        {
            leftArrowButton.interactable = (currentDifficultyIndex > 0);
        }
        if (rightArrowButton != null)
        {
            rightArrowButton.interactable = (currentDifficultyIndex < difficultyLevels.Length - 1);
        }

        // Update Play/Customize button text and action
        if (playButtonText != null)
        {
            if (currentSelection == DifficultyLevel.Custom)
            {
                playButtonText.text = "Customize";
            }
            else
            {
                playButtonText.text = "Play";
            }
        }

        // Ensure GameSettingsManager's current difficulty is always in sync
        if (GameSettingsManager.Instance != null)
        {
            GameSettingsManager.Instance.currentDifficulty = currentSelection;
        }
    }

    // Handles logic when the Play/Customize button is clicked
    public void OnPlayOrCustomizeClick()
    {
        if (GameSettingsManager.Instance != null)
        {
            DifficultyLevel currentSelection = difficultyLevels[currentDifficultyIndex];
            if (currentSelection == DifficultyLevel.Custom)
            {
                GoToCustomization();
            }
            else
            {
                PlayGame();
            }
        }
        else
        {
            Debug.LogError("GameSettingsManager not found!");
        }
    }

    // Standard PlayGame logic
    public void PlayGame()
    {
        if (GameSettingsManager.Instance != null)
        {
            // Apply settings one last time before loading the game scene, just in case
            if (GameSettingsManager.Instance.currentDifficulty != DifficultyLevel.Custom)
            {
                GameSettingsManager.Instance.ApplyPresetSettings(GameSettingsManager.Instance.currentDifficulty);
            }
            SceneManager.LoadScene(gameSceneName);
        }
        else
        {
            Debug.LogError("GameSettingsManager not found!");
        }
    }

    public void GoToCustomization()
    {
        SceneManager.LoadScene(customizationSceneName);
    }

    public void QuitGame()
    {
        Debug.Log("Quitting game...");
#if UNITY_EDITOR
        UnityEditor.EditorApplication.isPlaying = false;
#else
        Application.Quit();
#endif
    }
}
