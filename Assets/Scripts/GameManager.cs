using UnityEngine;

/// <summary>
/// Global game state manager that persists across scenes
/// Tracks tutorial mode and other game-wide settings
/// </summary>
public class GameManager : MonoBehaviour
{
    private static GameManager instance;

    [Header("Game Mode")]
    [SerializeField] private bool isTutorialMode = false;

    void Awake()
    {
        // Singleton pattern
        if (instance == null)
        {
            instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else
        {
            Destroy(gameObject);
        }
    }

    /// <summary>
    /// Set whether the game is in tutorial mode
    /// </summary>
    public static void SetTutorialMode(bool enabled)
    {
        if (instance != null)
        {
            instance.isTutorialMode = enabled;
            Debug.Log($"Tutorial mode {(enabled ? "enabled" : "disabled")}");
        }
    }

    /// <summary>
    /// Check if the game is currently in tutorial mode
    /// </summary>
    public static bool IsTutorialMode()
    {
        return instance != null && instance.isTutorialMode;
    }

    /// <summary>
    /// Reset tutorial mode (useful when returning to menu)
    /// </summary>
    public static void ResetTutorialMode()
    {
        SetTutorialMode(false);
    }
}
