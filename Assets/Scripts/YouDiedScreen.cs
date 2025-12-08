using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using TMPro;
using System.Collections;

/// <summary>
/// Dark Souls-inspired "YOU DIED" death screen effect
/// Shows a dramatic fade to black with text reveal
/// </summary>
public class YouDiedScreen : MonoBehaviour
{
    [Header("UI References")]
    [SerializeField] private GameObject deathScreenPanel;
    [SerializeField] private TextMeshProUGUI youDiedTextTMP;
    [SerializeField] private Text youDiedText; // Legacy UI Text support
    [SerializeField] private Image backgroundImage;
    [SerializeField] private Button restartButton;

    [Header("Animation Settings")]
    [SerializeField] private float fadeInDuration = 2f;
    [SerializeField] private float textDelayDuration = 0.5f;
    [SerializeField] private float textFadeInDuration = 1.5f;
    [SerializeField] private float displayDuration = 0.5f;

    [Header("Visual Settings")]
    [SerializeField] private Color backgroundColor = new Color(0, 0, 0, 0.9f);
    [SerializeField] private Color textColor = new Color(0.8f, 0.1f, 0.1f, 1f); // Dark red

    private static YouDiedScreen instance;

    void Awake()
    {
        // Singleton pattern
        if (instance == null)
        {
            instance = this;
        }
        else
        {
            Destroy(gameObject);
            return;
        }

        // Ensure the death screen is hidden at start
        if (deathScreenPanel != null)
        {
            deathScreenPanel.SetActive(false);
        }

        // Setup restart button
        if (restartButton != null)
        {
            restartButton.onClick.AddListener(RestartGame);
        }
    }

    /// <summary>
    /// Show the YOU DIED screen with Dark Souls-style animation
    /// </summary>
    public static void Show()
    {
        if (instance != null)
        {
            instance.StartCoroutine(instance.ShowDeathScreenCoroutine());
        }
        else
        {
            Debug.LogWarning("YouDiedScreen instance not found in scene!");
        }
    }

    private IEnumerator ShowDeathScreenCoroutine()
    {
        if (deathScreenPanel == null || backgroundImage == null)
        {
            Debug.LogError("YouDiedScreen: Missing UI references!");
            yield break;
        }

        if (youDiedTextTMP == null && youDiedText == null)
        {
            Debug.LogError("YouDiedScreen: No text component assigned (use either TextMeshPro or legacy Text)!");
            yield break;
        }

        // Activate the panel
        deathScreenPanel.SetActive(true);

        // Initialize with transparent background and text
        Color bgColor = backgroundColor;
        bgColor.a = 0f;
        backgroundImage.color = bgColor;

        Color txtColor = textColor;
        txtColor.a = 0f;

        // Set color based on which text component is used
        if (youDiedTextTMP != null)
        {
            youDiedTextTMP.color = txtColor;
        }
        else if (youDiedText != null)
        {
            youDiedText.color = txtColor;
        }

        // Hide restart button initially
        if (restartButton != null)
        {
            CanvasGroup buttonCanvasGroup = restartButton.GetComponent<CanvasGroup>();
            if (buttonCanvasGroup == null)
            {
                buttonCanvasGroup = restartButton.gameObject.AddComponent<CanvasGroup>();
            }
            buttonCanvasGroup.alpha = 0f;
            restartButton.interactable = false;
        }

        // Phase 1: Fade in the background
        float elapsed = 0f;
        while (elapsed < fadeInDuration)
        {
            elapsed += Time.deltaTime;
            float alpha = Mathf.Lerp(0f, backgroundColor.a, elapsed / fadeInDuration);
            bgColor.a = alpha;
            backgroundImage.color = bgColor;
            yield return null;
        }

        // Ensure background is fully visible
        bgColor.a = backgroundColor.a;
        backgroundImage.color = bgColor;

        // Phase 2: Wait before showing text
        yield return new WaitForSeconds(textDelayDuration);

        // Phase 3: Fade in the "YOU DIED" text
        elapsed = 0f;
        while (elapsed < textFadeInDuration)
        {
            elapsed += Time.deltaTime;
            float alpha = Mathf.Lerp(0f, textColor.a, elapsed / textFadeInDuration);
            txtColor.a = alpha;

            if (youDiedTextTMP != null)
            {
                youDiedTextTMP.color = txtColor;
            }
            else if (youDiedText != null)
            {
                youDiedText.color = txtColor;
            }
            yield return null;
        }

        // Ensure text is fully visible
        txtColor.a = textColor.a;
        if (youDiedTextTMP != null)
        {
            youDiedTextTMP.color = txtColor;
        }
        else if (youDiedText != null)
        {
            youDiedText.color = txtColor;
        }

        // Phase 4: Display for duration
        yield return new WaitForSeconds(displayDuration);

        // Phase 5: Fade in restart button
        if (restartButton != null)
        {
            CanvasGroup buttonCanvasGroup = restartButton.GetComponent<CanvasGroup>();
            if (buttonCanvasGroup != null)
            {
                elapsed = 0f;
                float buttonFadeDuration = 0.3f;

                while (elapsed < buttonFadeDuration)
                {
                    elapsed += Time.deltaTime;
                    buttonCanvasGroup.alpha = Mathf.Lerp(0f, 1f, elapsed / buttonFadeDuration);
                    yield return null;
                }

                buttonCanvasGroup.alpha = 1f;
                restartButton.interactable = true;
            }
        }
    }

    /// <summary>
    /// Hide the death screen (useful for restarting)
    /// </summary>
    public static void Hide()
    {
        if (instance != null && instance.deathScreenPanel != null)
        {
            instance.deathScreenPanel.SetActive(false);
        }
    }

    /// <summary>
    /// Create a YOU DIED screen UI programmatically if one doesn't exist
    /// </summary>
    public static void CreateDeathScreenUI()
    {
        // Find or create Canvas
        Canvas canvas = FindAnyObjectByType<Canvas>();
        if (canvas == null)
        {
            GameObject canvasObj = new GameObject("Canvas");
            canvas = canvasObj.AddComponent<Canvas>();
            canvas.renderMode = RenderMode.ScreenSpaceOverlay;
            canvasObj.AddComponent<CanvasScaler>();
            canvasObj.AddComponent<GraphicRaycaster>();
        }

        // Create death screen panel
        GameObject panelObj = new GameObject("YouDiedPanel");
        panelObj.transform.SetParent(canvas.transform, false);

        RectTransform panelRect = panelObj.AddComponent<RectTransform>();
        panelRect.anchorMin = Vector2.zero;
        panelRect.anchorMax = Vector2.one;
        panelRect.sizeDelta = Vector2.zero;

        Image panelImage = panelObj.AddComponent<Image>();
        panelImage.color = new Color(0, 0, 0, 0.9f);

        // Create YOU DIED text
        GameObject textObj = new GameObject("YouDiedText");
        textObj.transform.SetParent(panelObj.transform, false);

        RectTransform textRect = textObj.AddComponent<RectTransform>();
        textRect.anchorMin = new Vector2(0.5f, 0.5f);
        textRect.anchorMax = new Vector2(0.5f, 0.5f);
        textRect.sizeDelta = new Vector2(800, 200);

        Text text = textObj.AddComponent<Text>();
        text.text = "YOU DIED";
        text.font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
        text.fontSize = 72;
        text.alignment = TextAnchor.MiddleCenter;
        text.color = new Color(0.8f, 0.1f, 0.1f, 1f);
        text.fontStyle = FontStyle.Bold;

        // Add YouDiedScreen component
        GameObject screenObj = new GameObject("YouDiedScreen");
        YouDiedScreen screen = screenObj.AddComponent<YouDiedScreen>();
        screen.deathScreenPanel = panelObj;
        screen.youDiedText = text;
        screen.backgroundImage = panelImage;

        panelObj.SetActive(false);
    }

    /// <summary>
    /// Restart the game by loading the LoadingScreen scene
    /// </summary>
    private void RestartGame()
    {
        SceneManager.LoadScene("LoadingScreen");
    }
}
