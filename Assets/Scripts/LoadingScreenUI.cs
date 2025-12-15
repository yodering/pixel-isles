using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

/// <summary>
/// UI controller for the LoadingScreen scene
/// Provides buttons to start the game in regular or tutorial mode
/// </summary>
public class LoadingScreenUI : MonoBehaviour
{
    [Header("UI Buttons")]
    [Tooltip("Optional - Leave empty if you already have a Start button set up")]
    [SerializeField] private Button playButton;
    [Tooltip("Required - Assign your Tutorial button here")]
    [SerializeField] private Button tutorialButton;
    [Tooltip("Optional - Leave empty if you already have a Quit button set up")]
    [SerializeField] private Button quitButton;

    [Header("Scene Names")]
    [SerializeField] private string tutorialSceneName = "tutorial";
    [SerializeField] private string defaultSceneName = "default";

    void Start()
    {
        // Ensure time is running (in case it was paused)
        Time.timeScale = 1f;

        // Hide death screen if it still exists from previous scene
        YouDiedScreen.Hide();

        // Setup button listeners (only if buttons are assigned)
        if (playButton != null)
        {
            playButton.onClick.AddListener(StartGame);
            playButton.interactable = true; // Ensure button is interactable
            Debug.Log("Play button listener added successfully");
        }
        else
        {
            Debug.LogWarning("LoadingScreenUI: Play button not assigned in Inspector!");
        }

        if (tutorialButton != null)
        {
            tutorialButton.onClick.AddListener(StartTutorial);
            tutorialButton.interactable = true;
            Debug.Log("Tutorial button listener added successfully");
        }

        if (quitButton != null)
        {
            quitButton.onClick.AddListener(QuitGame);
        }

        // Reset tutorial mode when loading screen starts
        GameManager.ResetTutorialMode();

        // Check for EventSystem
        CheckForEventSystem();
    }

    /// <summary>
    /// Ensure an EventSystem exists for UI interaction
    /// </summary>
    private void CheckForEventSystem()
    {
        UnityEngine.EventSystems.EventSystem eventSystem = FindAnyObjectByType<UnityEngine.EventSystems.EventSystem>();
        if (eventSystem == null)
        {
            Debug.LogWarning("No EventSystem found! Creating one...");
            GameObject eventSystemObj = new GameObject("EventSystem");
            eventSystemObj.AddComponent<UnityEngine.EventSystems.EventSystem>();
            eventSystemObj.AddComponent<UnityEngine.EventSystems.StandaloneInputModule>();
        }
        else
        {
            Debug.Log("EventSystem found - UI should work correctly");
        }
    }

    /// <summary>
    /// Start the regular game
    /// </summary>
    private void StartGame()
    {
        Debug.Log($"StartGame() called - Loading scene: {defaultSceneName}");
        GameManager.SetTutorialMode(false);
        SceneManager.LoadScene(defaultSceneName);
    }

    /// <summary>
    /// Start the tutorial
    /// </summary>
    private void StartTutorial()
    {
        Debug.Log($"StartTutorial() called - Loading scene: {tutorialSceneName}");
        GameManager.SetTutorialMode(true);
        SceneManager.LoadScene(tutorialSceneName);
    }

    /// <summary>
    /// Quit the game
    /// </summary>
    private void QuitGame()
    {
        Application.Quit();
        #if UNITY_EDITOR
        UnityEditor.EditorApplication.isPlaying = false;
        #endif
    }
}
