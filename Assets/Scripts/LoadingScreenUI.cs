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
        // Setup button listeners (only if buttons are assigned)
        if (playButton != null)
        {
            playButton.onClick.AddListener(StartGame);
        }

        if (tutorialButton != null)
        {
            tutorialButton.onClick.AddListener(StartTutorial);
        }

        if (quitButton != null)
        {
            quitButton.onClick.AddListener(QuitGame);
        }

        // Reset tutorial mode when loading screen starts
        GameManager.ResetTutorialMode();
    }

    /// <summary>
    /// Start the regular game
    /// </summary>
    private void StartGame()
    {
        GameManager.SetTutorialMode(false);
        SceneManager.LoadScene(defaultSceneName);
    }

    /// <summary>
    /// Start the tutorial
    /// </summary>
    private void StartTutorial()
    {
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
