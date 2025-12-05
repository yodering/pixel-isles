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

    private const float FONT_SIZE = 10f; // Fixed font size

    void Start()
    {
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

    private void ConfigureTextStyle()
    {
        textField.enableAutoSizing = false;
        textField.fontSize = FONT_SIZE;
        textField.color = new Color(textColor.r, textColor.g, textColor.b, 1f);
        textField.outlineWidth = outlineWidth;
        textField.outlineColor = outlineColor;
        textField.enabled = true;
        textField.fontStyle = FontStyles.Bold;
        textField.alignment = TextAlignmentOptions.Center;
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
        textField.fontSize = FONT_SIZE;
        textField.raycastTarget = false;
        textField.ForceMeshUpdate();
        textField.fontSize = FONT_SIZE;
        
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
