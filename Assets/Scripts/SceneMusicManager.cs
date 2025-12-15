using UnityEngine;
using UnityEngine.SceneManagement;

/// <summary>
/// Manages music playback for different scenes
/// Automatically plays the correct music track when a scene loads
/// </summary>
public class SceneMusicManager : MonoBehaviour
{
    [System.Serializable]
    public class SceneMusicMapping
    {
        public string sceneName;
        public AudioClip musicClip;
    }

    [Header("Music Tracks")]
    [SerializeField] private AudioClip loadingMusic;
    [SerializeField] private AudioClip tutorialMusic;
    [SerializeField] private AudioClip defaultMusic;
    [SerializeField] private AudioClip scene1Music;
    [SerializeField] private AudioClip scene2Music;

    [Header("Settings")]
    [SerializeField] private bool fadeBetweenTracks = true;
    [SerializeField] private float fadeDuration = 1.5f;
    [SerializeField] private bool playOnAwake = true;

    private string currentSceneName;

    private void Awake()
    {
        // Subscribe to scene loaded event
        SceneManager.sceneLoaded += OnSceneLoaded;
    }

    private void Start()
    {
        // Play music for the initial scene if enabled
        // Using Start() instead of Awake() ensures AudioManager.Instance is initialized
        if (playOnAwake)
        {
            currentSceneName = SceneManager.GetActiveScene().name;
            PlayMusicForScene(currentSceneName);
        }
    }

    private void OnDestroy()
    {
        // Unsubscribe from scene loaded event
        SceneManager.sceneLoaded -= OnSceneLoaded;
    }

    private void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        // Only change music if we're loading a different scene
        if (scene.name != currentSceneName)
        {
            currentSceneName = scene.name;
            PlayMusicForScene(scene.name);
        }
    }

    private void PlayMusicForScene(string sceneName)
    {
        if (AudioManager.Instance == null)
        {
            Debug.LogWarning("SceneMusicManager: AudioManager not found!");
            return;
        }

        AudioClip musicToPlay = GetMusicForScene(sceneName);

        if (musicToPlay != null)
        {
            AudioManager.Instance.PlayMusic(musicToPlay, fadeBetweenTracks, fadeDuration);
            Debug.Log($"SceneMusicManager: Playing music for scene '{sceneName}'");
        }
        else
        {
            Debug.LogWarning($"SceneMusicManager: No music assigned for scene '{sceneName}'");
        }
    }

    private AudioClip GetMusicForScene(string sceneName)
    {
        // Normalize scene name (remove path and extension)
        string normalizedName = sceneName.ToLower();

        // Check for specific scene matches
        if (normalizedName.Contains("loading"))
            return loadingMusic;

        if (normalizedName.Contains("tutorial"))
            return tutorialMusic != null ? tutorialMusic : loadingMusic; // Use loading music for tutorial

        if (normalizedName.Contains("default"))
            return defaultMusic;

        if (normalizedName.Contains("ice") || normalizedName.Contains("scene-1"))
            return scene1Music;

        if (normalizedName.Contains("autumn") || normalizedName.Contains("scene-2"))
            return scene2Music;

        // Default fallback
        return defaultMusic;
    }

    /// <summary>
    /// Manually trigger music for current scene (useful for testing)
    /// </summary>
    [ContextMenu("Play Music For Current Scene")]
    public void PlayCurrentSceneMusic()
    {
        PlayMusicForScene(SceneManager.GetActiveScene().name);
    }

    /// <summary>
    /// Stop all music
    /// </summary>
    public void StopMusic()
    {
        if (AudioManager.Instance != null)
        {
            AudioManager.Instance.StopMusic(fadeBetweenTracks, fadeDuration);
        }
    }
}
