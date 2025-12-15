using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

/// <summary>
/// Simple back button for the tutorial scene to return to LoadingScreen
/// </summary>
public class TutorialBackButton : MonoBehaviour
{
    [Header("Button Reference")]
    [Tooltip("Optional - Leave empty if this script is on the button itself")]
    [SerializeField] private Button backButton;

    void Start()
    {
        // If no button assigned, try to get it from this GameObject
        if (backButton == null)
        {
            backButton = GetComponent<Button>();
        }

        // Setup button listener
        if (backButton != null)
        {
            backButton.onClick.AddListener(GoBackToLoadingScreen);
        }
        else
        {
            Debug.LogWarning("TutorialBackButton: No button found!");
        }
    }

    /// <summary>
    /// Return to the loading screen
    /// </summary>
    private void GoBackToLoadingScreen()
    {
        Debug.Log("Returning to LoadingScreen from tutorial");

        // Reset tutorial mode
        GameManager.ResetTutorialMode();

        // Load the loading screen
        SceneManager.LoadScene("LoadingScreen");
    }

    /// <summary>
    /// Public method that can be called from Unity Button onClick event
    /// </summary>
    public void OnBackButtonClicked()
    {
        GoBackToLoadingScreen();
    }
}
