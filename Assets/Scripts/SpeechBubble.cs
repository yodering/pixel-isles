using UnityEngine;
using TMPro;

public class SpeechBubble : MonoBehaviour
{
    [SerializeField] private Canvas bubbleCanvas;
    [SerializeField] private TextMeshProUGUI textField;
    [SerializeField] private float defaultDuration = 2f;
    [SerializeField] private bool debugMode = true;
    [SerializeField] private bool showOnStart = true;

    [Header("Text Styling")]
    [SerializeField] private Color textColor = new Color(0.9f, 0.85f, 0.7f, 1f);
    [SerializeField] private Color outlineColor = new Color(0.2f, 0.1f, 0.05f, 1f);
    [SerializeField] private float outlineWidth = 0.2f;
    [SerializeField] private float textPadding = 15f;
    [SerializeField] private float fontSize = 24f; // Responsive font size

    [Header("Canvas Scaling")]
    [SerializeField] private bool autoConfigureCanvas = true;
    [SerializeField] private Vector2 referenceResolution = new Vector2(1920, 1080);
    [SerializeField] private float matchWidthOrHeight = 0.5f;

    [Header("Dialogue Box Layout")]
    [SerializeField] private bool usePokemonStyleLayout = true;
    [SerializeField] private float dialogueBoxHeight = 200f; // Height of the dialogue box
    [SerializeField] private float dialogueBoxWidthPercent = 0.9f; // 90% of screen width
    [SerializeField] private float bottomMargin = 20f; // Distance from bottom of screen

    void Start()
    {
        if (autoConfigureCanvas && bubbleCanvas != null)
        {
            ConfigureCanvasScaler();
        }

        if (usePokemonStyleLayout)
        {
            ConfigurePokemonStyleLayout();
        }

        if (textField != null)
        {
            ConfigureTextStyle();
        }

        if (debugMode)
        {
            if (bubbleCanvas == null)
                Debug.LogError("SpeechBubble: Bubble Canvas is not assigned!");
            else
                Debug.Log($"SpeechBubble: Canvas found - Active: {bubbleCanvas.gameObject.activeSelf}");

            if (textField == null)
                Debug.LogError("SpeechBubble: Text Field is not assigned!");
            else
                Debug.Log($"SpeechBubble: Text field found - FontSize: {textField.fontSize}");
        }

        if (showOnStart && bubbleCanvas != null && textField != null && !string.IsNullOrEmpty(textField.text))
        {
            ShowText(textField.text, defaultDuration);
        }
    }

    private void ConfigureCanvasScaler()
    {
        UnityEngine.UI.CanvasScaler scaler = bubbleCanvas.GetComponent<UnityEngine.UI.CanvasScaler>();
        if (scaler == null)
        {
            scaler = bubbleCanvas.gameObject.AddComponent<UnityEngine.UI.CanvasScaler>();
        }

        scaler.uiScaleMode = UnityEngine.UI.CanvasScaler.ScaleMode.ScaleWithScreenSize;
        scaler.referenceResolution = referenceResolution;
        scaler.matchWidthOrHeight = matchWidthOrHeight;

        if (debugMode)
        {
            Debug.Log($"SpeechBubble: Configured Canvas Scaler - Reference: {referenceResolution}, Match: {matchWidthOrHeight}");
        }
    }

    private void ConfigurePokemonStyleLayout()
    {
        // Find or get the RectTransform components
        RectTransform canvasRect = bubbleCanvas.GetComponent<RectTransform>();
        RectTransform textRect = textField.GetComponent<RectTransform>();

        // Get the parent container (dialogue box background)
        Transform dialogueBoxTransform = textField.transform.parent;
        RectTransform dialogueBoxRect = null;

        if (dialogueBoxTransform != null)
        {
            dialogueBoxRect = dialogueBoxTransform.GetComponent<RectTransform>();
        }

        // Configure dialogue box positioning (bottom of screen, centered)
        if (dialogueBoxRect != null)
        {
            // Anchor to bottom center
            dialogueBoxRect.anchorMin = new Vector2(0.5f, 0f);
            dialogueBoxRect.anchorMax = new Vector2(0.5f, 0f);
            dialogueBoxRect.pivot = new Vector2(0.5f, 0f);

            // Set size based on reference resolution
            float width = referenceResolution.x * dialogueBoxWidthPercent;
            dialogueBoxRect.sizeDelta = new Vector2(width, dialogueBoxHeight);

            // Position at bottom with margin
            dialogueBoxRect.anchoredPosition = new Vector2(0f, bottomMargin);

            if (debugMode)
            {
                Debug.Log($"SpeechBubble: Configured dialogue box - Width: {width}, Height: {dialogueBoxHeight}, Bottom Margin: {bottomMargin}");
            }
        }

        // Configure text field to fill the dialogue box
        if (textRect != null)
        {
            textRect.anchorMin = Vector2.zero;
            textRect.anchorMax = Vector2.one;
            textRect.offsetMin = new Vector2(textPadding, textPadding);
            textRect.offsetMax = new Vector2(-textPadding, -textPadding);
            textRect.anchoredPosition = Vector2.zero;

            if (debugMode)
            {
                Debug.Log($"SpeechBubble: Configured text field with padding: {textPadding}");
            }
        }
    }

    private void ConfigureTextStyle()
    {
        textField.enableAutoSizing = false;
        textField.fontSize = fontSize;
        textField.color = new Color(textColor.r, textColor.g, textColor.b, 1f);
        textField.outlineWidth = outlineWidth;
        textField.outlineColor = outlineColor;
        textField.enabled = true;
        textField.fontStyle = FontStyles.Bold;

        // Use left-aligned text for Pokemon-style, center for speech bubble style
        if (usePokemonStyleLayout)
        {
            textField.alignment = TextAlignmentOptions.TopLeft;
        }
        else
        {
            textField.alignment = TextAlignmentOptions.Center;
        }

        textField.textWrappingMode = TextWrappingModes.Normal;
        textField.overflowMode = TextOverflowModes.Overflow;
        textField.margin = new Vector4(textPadding, textPadding, textPadding, textPadding);
    }

    public void ShowText(string message, float duration = -1f)
    {
        if (duration <= 0f)
            duration = defaultDuration;

        if (bubbleCanvas == null || textField == null)
        {
            Debug.LogError("SpeechBubble: Canvas or TextField is null!");
            return;
        }

        textField.text = message;
        ConfigureTextStyle();

        textField.enableAutoSizing = false;
        textField.fontSize = fontSize;
        textField.raycastTarget = false;
        textField.ForceMeshUpdate();
        textField.fontSize = fontSize;

        bubbleCanvas.gameObject.SetActive(true);

        if (debugMode)
        {
            Debug.Log($"SpeechBubble: FontSize = {textField.fontSize}");
        }

        CancelInvoke(nameof(Hide));
        Invoke(nameof(Hide), duration);
    }

    private void Hide()
    {
        if (bubbleCanvas != null)
        {
            bubbleCanvas.gameObject.SetActive(false);
            if (debugMode)
                Debug.Log("SpeechBubble: Hiding bubble");
        }
    }

    [ContextMenu("Test Show Text")]
    public void TestShowText()
    {
        ShowText("TEST MESSAGE!", 5f);
    }
}
