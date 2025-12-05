using UnityEngine;
using UnityEngine.SceneManagement;
using System.Collections;

public class SceneLoader : MonoBehaviour
{
    [Header("Transition Settings")]
    [SerializeField] private float transitionDuration = 1f;
    [SerializeField] private bool useFadeTransition = true;

    private static SceneLoader instance;
    private CanvasGroup fadeCanvasGroup;
    private Canvas fadeCanvas;
    private bool isTransitioning = false;

    void Awake()
    {
        // Singleton pattern to persist across scenes
        if (instance == null)
        {
            instance = this;
            DontDestroyOnLoad(gameObject);

            if (useFadeTransition)
            {
                SetupFadeCanvas();
            }
        }
        else
        {
            Destroy(gameObject);
        }
    }

    /// <summary>
    /// Setup fade canvas for scene transitions
    /// </summary>
    private void SetupFadeCanvas()
    {
        // Create canvas for fade effect
        GameObject canvasObj = new GameObject("FadeCanvas");
        canvasObj.transform.SetParent(transform);

        fadeCanvas = canvasObj.AddComponent<Canvas>();
        fadeCanvas.renderMode = RenderMode.ScreenSpaceOverlay;
        fadeCanvas.sortingOrder = 9999; // Render on top of everything

        canvasObj.AddComponent<UnityEngine.UI.CanvasScaler>();
        canvasObj.AddComponent<UnityEngine.UI.GraphicRaycaster>();

        // Create black panel for fade
        GameObject panelObj = new GameObject("FadePanel");
        panelObj.transform.SetParent(canvasObj.transform);

        UnityEngine.UI.Image fadeImage = panelObj.AddComponent<UnityEngine.UI.Image>();
        fadeImage.color = Color.black;

        RectTransform rect = panelObj.GetComponent<RectTransform>();
        rect.anchorMin = Vector2.zero;
        rect.anchorMax = Vector2.one;
        rect.sizeDelta = Vector2.zero;
        rect.anchoredPosition = Vector2.zero;

        fadeCanvasGroup = panelObj.AddComponent<CanvasGroup>();
        fadeCanvasGroup.alpha = 0f;
        fadeCanvasGroup.blocksRaycasts = false;
    }

    /// <summary>
    /// Loads a scene by name with optional fade transition
    /// </summary>
    /// <param name="sceneName">Name of the scene to load</param>
    public void LoadScene(string sceneName)
    {
        if (useFadeTransition && !isTransitioning)
        {
            StartCoroutine(LoadSceneWithFade(sceneName));
        }
        else
        {
            SceneManager.LoadScene(sceneName);
        }
    }

    /// <summary>
    /// Loads a scene by build index with optional fade transition
    /// </summary>
    /// <param name="sceneIndex">Build index of the scene to load</param>
    public void LoadSceneByIndex(int sceneIndex)
    {
        if (useFadeTransition && !isTransitioning)
        {
            StartCoroutine(LoadSceneByIndexWithFade(sceneIndex));
        }
        else
        {
            SceneManager.LoadScene(sceneIndex);
        }
    }

    /// <summary>
    /// Load scene with fade transition
    /// </summary>
    private IEnumerator LoadSceneWithFade(string sceneName)
    {
        isTransitioning = true;

        // Fade out
        yield return StartCoroutine(Fade(1f));

        // Load scene
        SceneManager.LoadScene(sceneName);

        // Wait one frame for scene to load
        yield return null;

        // Fade in
        yield return StartCoroutine(Fade(0f));

        isTransitioning = false;
    }

    /// <summary>
    /// Load scene by index with fade transition
    /// </summary>
    private IEnumerator LoadSceneByIndexWithFade(int sceneIndex)
    {
        isTransitioning = true;

        // Fade out
        yield return StartCoroutine(Fade(1f));

        // Load scene
        SceneManager.LoadScene(sceneIndex);

        // Wait one frame for scene to load
        yield return null;

        // Fade in
        yield return StartCoroutine(Fade(0f));

        isTransitioning = false;
    }

    /// <summary>
    /// Fade the screen to target alpha
    /// </summary>
    private IEnumerator Fade(float targetAlpha)
    {
        if (fadeCanvasGroup == null)
        {
            yield break;
        }

        float startAlpha = fadeCanvasGroup.alpha;
        float elapsed = 0f;

        fadeCanvasGroup.blocksRaycasts = true;

        while (elapsed < transitionDuration)
        {
            elapsed += Time.deltaTime;
            fadeCanvasGroup.alpha = Mathf.Lerp(startAlpha, targetAlpha, elapsed / transitionDuration);
            yield return null;
        }

        fadeCanvasGroup.alpha = targetAlpha;
        fadeCanvasGroup.blocksRaycasts = targetAlpha > 0.5f;
    }

    /// <summary>
    /// Quits the application
    /// </summary>
    public void QuitGame()
    {
        Application.Quit();
        #if UNITY_EDITOR
        UnityEditor.EditorApplication.isPlaying = false;
        #endif
    }
}
