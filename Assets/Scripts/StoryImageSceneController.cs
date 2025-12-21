using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Collections;
using UnityEngine.SceneManagement;

/// <summary>
/// Controls story/tale scenes with vertical images, titles, and subtitles on black background
/// </summary>
public class StoryImageSceneController : MonoBehaviour
{
    [Header("Story Content")]
    [SerializeField] private Sprite storyImage; // The vertical tale image
    [SerializeField] private string storyTitle = "THE GREAT PURGE"; // Main title
    [SerializeField] private string storySubtitle = "Necessary sacrifices for a brighter tomorrow"; // Sarcastic subtitle
    [SerializeField] private string nextSceneName = ""; // Scene to load after

    [Header("UI References")]
    [SerializeField] private Canvas mainCanvas;
    [SerializeField] private Image storyImageDisplay;
    [SerializeField] private TextMeshProUGUI titleText;
    [SerializeField] private TextMeshProUGUI subtitleText;
    [SerializeField] private TextMeshProUGUI skipHintText;

    [Header("Layout Settings")]
    [SerializeField] private float imageWidthPercent = 0.5f; // Image takes 50% of screen width
    [SerializeField] private float displayDuration = 6f; // How long to show the image
    [SerializeField] private float fadeDuration = 1f;

    [Header("Auto Layout (recommended)")]
    [SerializeField] private bool useAutoLayout = true;
    [SerializeField] private bool useManualPositioning = false; // If true, preserves manual positions from Scene view
    [SerializeField, Range(0.5f, 1f)] private float maxImageHeightPercent = 0.92f;
    [SerializeField] private float sidePadding = 60f; // padding inside the left black bar
    [SerializeField] private float topPadding = 180f;  // distance from top for title block

    [Header("Title Positioning")]
    [SerializeField] private Vector2 titlePosition = new Vector2(-0.75f, 0.7f); // Legacy/manual positioning
    [SerializeField] private float titleMaxWidth = 300f;
    [SerializeField] private float subtitleOffset = 60f; // Distance below title

    [Header("Input Settings")]
    [SerializeField] private KeyCode skipKey = KeyCode.Space;
    [SerializeField] private bool allowMouseClick = true;

    private bool canSkip = false;

    void Start()
    {
        // Auto-populate storyImage from storyImageDisplay if not set
        if (storyImage == null && storyImageDisplay != null && storyImageDisplay.sprite != null)
        {
            storyImage = storyImageDisplay.sprite;
            Debug.Log($"StoryImageSceneController: Auto-assigned storyImage from storyImageDisplay.sprite");
        }
        
        if (storyImage == null)
        {
            Debug.LogError("StoryImageSceneController: No story image assigned! Assign it to 'Story Image' field or 'Story Image Display' component.");
            TransitionToNextScene();
            return;
        }

        // Wait one frame so CanvasScaler/layout has correct sizes (prevents tiny images / mispositioned text).
        StartCoroutine(InitAndPlay());
    }

    private IEnumerator InitAndPlay()
    {
        yield return null;
        SetupUI();
        yield return null;
        StartCoroutine(DisplayStorySequence());
    }

    void Update()
    {
        if (canSkip && (Input.GetKeyDown(skipKey) || (allowMouseClick && Input.GetMouseButtonDown(0))))
        {
            SkipToNextScene();
        }
    }

    private void SetupUI()
    {
        if (mainCanvas == null)
        {
            mainCanvas = GetComponentInChildren<Canvas>();
        }

        // Black background
        if (mainCanvas != null)
        {
            Image bgImage = mainCanvas.GetComponent<Image>();
            if (bgImage == null)
            {
                bgImage = mainCanvas.gameObject.AddComponent<Image>();
            }
            bgImage.color = Color.black;
        }

        // Determine layout space in Canvas units (use CanvasScaler reference resolution when available for stability).
        float canvasWidth = Screen.width;
        float canvasHeight = Screen.height;
        if (mainCanvas != null)
        {
            var scaler = mainCanvas.GetComponent<UnityEngine.UI.CanvasScaler>();
            if (scaler != null && scaler.uiScaleMode == UnityEngine.UI.CanvasScaler.ScaleMode.ScaleWithScreenSize)
            {
                canvasWidth = scaler.referenceResolution.x;
                canvasHeight = scaler.referenceResolution.y;
            }
            else
            {
                RectTransform canvasRect = mainCanvas.GetComponent<RectTransform>();
                if (canvasRect != null && canvasRect.rect.width > 0 && canvasRect.rect.height > 0)
                {
                    canvasWidth = canvasRect.rect.width;
                    canvasHeight = canvasRect.rect.height;
                }
            }
        }

        // Setup story image
        if (storyImageDisplay != null && storyImage != null)
        {
            storyImageDisplay.sprite = storyImage;
            storyImageDisplay.preserveAspect = true;
            storyImageDisplay.color = new Color(1, 1, 1, 0); // Start transparent

            // Only auto-position if not using manual positioning
            if (!useManualPositioning)
            {
                // Center the image, but constrain width
                RectTransform imageRect = storyImageDisplay.GetComponent<RectTransform>();
                imageRect.anchorMin = new Vector2(0.5f, 0.5f);
                imageRect.anchorMax = new Vector2(0.5f, 0.5f);
                imageRect.pivot = new Vector2(0.5f, 0.5f);
                imageRect.anchoredPosition = Vector2.zero;

                // Calculate size maintaining aspect ratio using CANVAS units (stable across resolutions).
                float clampedWidthPercent = Mathf.Clamp(imageWidthPercent, 0.25f, 0.9f);
                float maxWidth = canvasWidth * clampedWidthPercent;
                float maxHeight = canvasHeight * maxImageHeightPercent;

                float imageAspect = 1f;
                if (storyImage.texture != null && storyImage.texture.width > 0)
                {
                    imageAspect = (float)storyImage.texture.height / storyImage.texture.width; // height / width
                }

                // Fit image into (maxWidth, maxHeight)
                float targetWidth = maxWidth;
                float targetHeight = targetWidth * imageAspect;
                if (targetHeight > maxHeight)
                {
                    targetHeight = maxHeight;
                    targetWidth = targetHeight / imageAspect;
                }

                imageRect.sizeDelta = new Vector2(targetWidth, targetHeight);
            }
        }

        // Setup title text
        if (titleText != null)
        {
            titleText.text = storyTitle;
            titleText.fontSize = 28; // Reduced from 36
            titleText.fontStyle = FontStyles.Bold;
            titleText.color = new Color(0.9f, 0.85f, 0.7f, 0); // Start transparent
            titleText.alignment = TextAlignmentOptions.Left;
            titleText.enableWordWrapping = true;
            titleText.overflowMode = TextOverflowModes.Overflow;

            // Only auto-position if not using manual positioning
            if (!useManualPositioning)
            {
                RectTransform titleRect = titleText.GetComponent<RectTransform>();

                if (useAutoLayout && storyImageDisplay != null)
                {
                    RectTransform imageRect = storyImageDisplay.GetComponent<RectTransform>();
                    float imageWidth = imageRect != null ? imageRect.sizeDelta.x : (canvasWidth * imageWidthPercent);
                    float leftBarWidth = Mathf.Max(0f, (canvasWidth - imageWidth) * 0.5f);

                    float paddingX = Mathf.Clamp(sidePadding, 24f, Mathf.Max(24f, leftBarWidth * 0.35f));
                    float maxTextWidth = Mathf.Max(160f, leftBarWidth - paddingX * 2f);

                    titleRect.anchorMin = new Vector2(0f, 1f);
                    titleRect.anchorMax = new Vector2(0f, 1f);
                    titleRect.pivot = new Vector2(0f, 1f);
                    titleRect.sizeDelta = new Vector2(maxTextWidth, 100f); // Reduced from 140f
                    titleRect.anchoredPosition = new Vector2(paddingX, -topPadding);
                }
                else
                {
                    // Legacy/manual positioning
                    titleRect.anchorMin = new Vector2(0, 0.5f);
                    titleRect.anchorMax = new Vector2(0, 0.5f);
                    titleRect.pivot = new Vector2(0, 0.5f);
                    titleRect.sizeDelta = new Vector2(titleMaxWidth, 100);
                    float leftBarWidth = (canvasWidth * (1f - imageWidthPercent)) / 2f;
                    float xPos = leftBarWidth / 2f - titleMaxWidth / 2f;
                    titleRect.anchoredPosition = new Vector2(xPos, canvasHeight * titlePosition.y - canvasHeight * 0.5f);
                }
            }
        }

        // Setup subtitle text
        if (subtitleText != null)
        {
            subtitleText.text = storySubtitle;
            subtitleText.fontSize = 18;
            subtitleText.fontStyle = FontStyles.Italic;
            subtitleText.color = new Color(0.7f, 0.65f, 0.5f, 0); // Start transparent
            subtitleText.alignment = TextAlignmentOptions.Left;
            subtitleText.enableWordWrapping = true;
            subtitleText.overflowMode = TextOverflowModes.Overflow;

            // Only auto-position if not using manual positioning
            if (!useManualPositioning)
            {
                RectTransform subtitleRect = subtitleText.GetComponent<RectTransform>();
                RectTransform titleRect = titleText != null ? titleText.GetComponent<RectTransform>() : null;
                if (useAutoLayout && titleRect != null)
                {
                    subtitleRect.anchorMin = titleRect.anchorMin;
                    subtitleRect.anchorMax = titleRect.anchorMax;
                    subtitleRect.pivot = titleRect.pivot;
                    subtitleRect.sizeDelta = new Vector2(titleRect.sizeDelta.x, 100f); // Reduced from 120f
                    // Position subtitle below title with smaller gap
                    subtitleRect.anchoredPosition = titleRect.anchoredPosition - new Vector2(0, titleRect.sizeDelta.y + 20f);
                }
                else
                {
                    subtitleRect.anchorMin = new Vector2(0, 0.5f);
                    subtitleRect.anchorMax = new Vector2(0, 0.5f);
                    subtitleRect.pivot = new Vector2(0, 0.5f);
                    subtitleRect.sizeDelta = new Vector2(titleMaxWidth, 80);
                    if (titleRect != null)
                    {
                        subtitleRect.anchoredPosition = titleRect.anchoredPosition - new Vector2(0, subtitleOffset);
                    }
                }
            }
        }

        // Skip hint
        if (skipHintText != null)
        {
            skipHintText.text = "Press SPACE or Click to Continue";
            skipHintText.gameObject.SetActive(true);
        }
    }

    private IEnumerator DisplayStorySequence()
    {
        yield return new WaitForSeconds(0.3f);

        // Fade in title
        if (titleText != null)
        {
            yield return StartCoroutine(FadeText(titleText, 0f, 1f, fadeDuration));
        }

        yield return new WaitForSeconds(0.2f);

        // Fade in subtitle
        if (subtitleText != null)
        {
            yield return StartCoroutine(FadeText(subtitleText, 0f, 1f, fadeDuration));
        }

        yield return new WaitForSeconds(0.3f);

        // Fade in image
        if (storyImageDisplay != null)
        {
            yield return StartCoroutine(FadeImage(storyImageDisplay, 0f, 1f, fadeDuration));
        }

        // Allow skipping
        canSkip = true;

        // Wait for display duration
        yield return new WaitForSeconds(displayDuration);

        // Fade out everything
        yield return StartCoroutine(FadeOutAll());

        // Transition to next scene
        TransitionToNextScene();
    }

    private IEnumerator FadeText(TextMeshProUGUI text, float startAlpha, float targetAlpha, float duration)
    {
        float elapsed = 0f;
        Color color = text.color;

        while (elapsed < duration)
        {
            elapsed += Time.deltaTime;
            float t = elapsed / duration;
            color.a = Mathf.Lerp(startAlpha, targetAlpha, t);
            text.color = color;
            yield return null;
        }

        color.a = targetAlpha;
        text.color = color;
    }

    private IEnumerator FadeImage(Image image, float startAlpha, float targetAlpha, float duration)
    {
        float elapsed = 0f;
        Color color = image.color;

        while (elapsed < duration)
        {
            elapsed += Time.deltaTime;
            float t = elapsed / duration;
            color.a = Mathf.Lerp(startAlpha, targetAlpha, t);
            image.color = color;
            yield return null;
        }

        color.a = targetAlpha;
        image.color = color;
    }

    private IEnumerator FadeOutAll()
    {
        float duration = fadeDuration * 0.5f;
        
        Coroutine titleFade = null;
        Coroutine subtitleFade = null;
        Coroutine imageFade = null;

        if (titleText != null)
            titleFade = StartCoroutine(FadeText(titleText, 1f, 0f, duration));
        if (subtitleText != null)
            subtitleFade = StartCoroutine(FadeText(subtitleText, 1f, 0f, duration));
        if (storyImageDisplay != null)
            imageFade = StartCoroutine(FadeImage(storyImageDisplay, 1f, 0f, duration));

        yield return new WaitForSeconds(duration);
    }

    private void SkipToNextScene()
    {
        StopAllCoroutines();
        TransitionToNextScene();
    }

    private void TransitionToNextScene()
    {
        if (string.IsNullOrEmpty(nextSceneName))
        {
            Debug.LogWarning("StoryImageSceneController: No next scene configured!");
            return;
        }

        SceneLoader sceneLoader = FindAnyObjectByType<SceneLoader>();

        if (sceneLoader != null)
        {
            sceneLoader.LoadScene(nextSceneName);
        }
        else
        {
            SceneManager.LoadScene(nextSceneName);
        }
    }
}

