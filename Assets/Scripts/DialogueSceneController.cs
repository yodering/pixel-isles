using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Collections;
using System.Collections.Generic;
using UnityEngine.SceneManagement;

/// <summary>
/// Controls dialogue scenes with sequential animated dialogue boxes on black screen
/// </summary>
public class DialogueSceneController : MonoBehaviour
{
    [System.Serializable]
    public class DialogueLine
    {
        public string speaker; // "Voice" or "Knight"
        [TextArea(2, 4)]
        public string text;
    }

    [Header("Dialogue Data")]
    [SerializeField] private List<DialogueLine> dialogueLines = new List<DialogueLine>();
    [SerializeField] private string nextSceneName = ""; // Scene to load after dialogue completes

    [Header("UI References")]
    [SerializeField] private Canvas dialogueCanvas;
    [SerializeField] private GameObject dialogueBoxPrefab; // Prefab for dialogue box
    [SerializeField] private Transform dialogueBoxContainer; // Parent container for dialogue boxes
    [SerializeField] private TextMeshProUGUI skipHintText;

    [Header("Dialogue Box Settings")]
    [SerializeField] private float dialogueDisplayDuration = 0.8f; // Very fast display
    [SerializeField] private float timeBetweenLines = 0.05f; // Very fast transitions
    [SerializeField] private float boxFadeInDuration = 0.15f; // Very fast fade-in
    [SerializeField] private float boxSlideDistance = 50f;
    [SerializeField] private int maxVisibleBoxes = 5; // After this many, fade out oldest and replace with new
    [SerializeField] private float boxFadeOutDuration = 0.2f; // Very fast normal fade out
    [SerializeField] private float boxFadeOutDurationFast = 0.1f; // Ultra fast fade out after hitting max
    [SerializeField] private float initialDelay = 0.2f; // Initial delay before first dialogue
    [SerializeField] private float endDelay = 1f; // Delay before auto-transition

    [Header("Input Settings")]
    [SerializeField] private KeyCode skipKey = KeyCode.Space;
    [SerializeField] private bool allowMouseClick = true;

    private int currentDialogueIndex = 0;
    private bool isDisplaying = false;
    private bool canSkip = false;
    private List<GameObject> spawnedDialogueBoxes = new List<GameObject>();

    void Start()
    {
        if (dialogueLines.Count == 0)
        {
            Debug.LogError("DialogueSceneController: No dialogue lines configured!");
            TransitionToNextScene();
            return;
        }

        SetupUI();
        StartCoroutine(DisplayDialogueSequence());
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
        // Ensure canvas has black background
        if (dialogueCanvas != null)
        {
            Image bgImage = dialogueCanvas.GetComponent<Image>();
            if (bgImage == null)
            {
                bgImage = dialogueCanvas.gameObject.AddComponent<Image>();
            }
            bgImage.color = Color.black;
        }

        // Show skip hint
        if (skipHintText != null)
        {
            skipHintText.text = "Press SPACE or Click to Skip";
            skipHintText.fontSize = 16; // Smaller skip hint
            skipHintText.gameObject.SetActive(true);
        }
    }

    private IEnumerator DisplayDialogueSequence()
    {
        yield return new WaitForSeconds(initialDelay); // Use serialized field
        
        // Allow skipping immediately
        canSkip = true;

        while (currentDialogueIndex < dialogueLines.Count)
        {
            DialogueLine line = dialogueLines[currentDialogueIndex];
            yield return StartCoroutine(DisplayDialogueLine(line));
            currentDialogueIndex++;

            // Wait between lines
            if (currentDialogueIndex < dialogueLines.Count)
            {
                yield return new WaitForSeconds(timeBetweenLines);
            }
        }

        // Wait before auto-transitioning (use serialized field)
        yield return new WaitForSeconds(endDelay);
        TransitionToNextScene();
    }

    private IEnumerator DisplayDialogueLine(DialogueLine line)
    {
        isDisplaying = true;

        bool isAtMaxCapacity = spawnedDialogueBoxes.Count >= maxVisibleBoxes;
        
        // If we have max visible boxes, fade out the oldest one (and do it fast!)
        if (isAtMaxCapacity)
        {
            GameObject oldestBox = spawnedDialogueBoxes[0];
            if (oldestBox != null)
            {
                // Use fast fade-out after we hit max capacity
                StartCoroutine(FadeOutDialogueBox(oldestBox, boxFadeOutDurationFast));
                spawnedDialogueBoxes.RemoveAt(0);
                Destroy(oldestBox, boxFadeOutDurationFast); // Destroy after fast fade completes
            }
            
            // Also fade out the next oldest ones faster for smoother rolling effect
            for (int i = 0; i < Mathf.Min(2, spawnedDialogueBoxes.Count); i++)
            {
                GameObject box = spawnedDialogueBoxes[i];
                if (box != null)
                {
                    CanvasGroup cg = box.GetComponent<CanvasGroup>();
                    if (cg != null && cg.alpha > 0.6f)
                    {
                        // Gradually reduce alpha on older boxes (very fast)
                        StartCoroutine(FadeToAlpha(cg, 0.6f - (i * 0.15f), 0.1f));
                    }
                }
            }
        }

        // Create dialogue box from prefab or create simple one
        GameObject dialogueBox = CreateDialogueBox(line);
        spawnedDialogueBoxes.Add(dialogueBox);

        // Animate box appearing (slide in + fade)
        yield return StartCoroutine(AnimateDialogueBoxIn(dialogueBox));

        // Wait for display duration
        float elapsed = 0f;
        while (elapsed < dialogueDisplayDuration)
        {
            elapsed += Time.deltaTime;
            yield return null;
        }

        isDisplaying = false;
    }

    private GameObject CreateDialogueBox(DialogueLine line)
    {
        GameObject box;

        if (dialogueBoxPrefab != null)
        {
            box = Instantiate(dialogueBoxPrefab, dialogueBoxContainer);
        }
        else
        {
            // Create simple dialogue box
            box = new GameObject("DialogueBox_" + currentDialogueIndex);
            box.transform.SetParent(dialogueBoxContainer);

            RectTransform rectTransform = box.AddComponent<RectTransform>();
            rectTransform.sizeDelta = new Vector2(1600, 300); // Doubled from 800x150

            // Background - pure black
            Image bgImage = box.AddComponent<Image>();
            bgImage.color = new Color(0f, 0f, 0f, 0.95f);

            // Speaker text
            GameObject speakerObj = new GameObject("Speaker");
            speakerObj.transform.SetParent(box.transform);
            RectTransform speakerRect = speakerObj.AddComponent<RectTransform>();
            speakerRect.anchorMin = new Vector2(0, 1);
            speakerRect.anchorMax = new Vector2(0, 1);
            speakerRect.pivot = new Vector2(0, 1);
            speakerRect.anchoredPosition = new Vector2(40, -20); // Doubled padding
            speakerRect.sizeDelta = new Vector2(1520, 60); // Doubled from 760x30

            TextMeshProUGUI speakerText = speakerObj.AddComponent<TextMeshProUGUI>();
            speakerText.text = line.speaker + ":";
            speakerText.fontSize = 40; // Doubled from 20
            speakerText.fontStyle = FontStyles.Bold;
            speakerText.color = line.speaker == "Voice" ? new Color(1f, 0.8f, 0.2f) : new Color(0.8f, 0.9f, 1f);

            // Dialogue text
            GameObject textObj = new GameObject("Text");
            textObj.transform.SetParent(box.transform);
            RectTransform textRect = textObj.AddComponent<RectTransform>();
            textRect.anchorMin = new Vector2(0, 0);
            textRect.anchorMax = new Vector2(1, 1);
            textRect.offsetMin = new Vector2(40, 30); // Doubled padding
            textRect.offsetMax = new Vector2(-40, -90); // Doubled padding

            TextMeshProUGUI dialogueText = textObj.AddComponent<TextMeshProUGUI>();
            dialogueText.text = line.text;
            dialogueText.fontSize = 36; // Doubled from 18
            dialogueText.color = new Color(0.95f, 0.95f, 0.95f);
            dialogueText.alignment = TextAlignmentOptions.TopLeft;
        }

        // Position box (stack downward from top-left, reset to top when max boxes reached)
        RectTransform boxRect = box.GetComponent<RectTransform>();
        if (boxRect != null)
        {
            boxRect.anchorMin = new Vector2(0f, 1f); // Top-left anchor
            boxRect.anchorMax = new Vector2(0f, 1f);
            boxRect.pivot = new Vector2(0, 1f); // Pivot at top-left
            
            // Calculate position based on current box count
            int positionIndex = spawnedDialogueBoxes.Count; // Use current count (before adding this box)
            float yOffset = -100f - (positionIndex * 170f); // Stack from top
            boxRect.anchoredPosition = new Vector2(100f - boxSlideDistance, yOffset);
        }

        return box;
    }

    private IEnumerator AnimateDialogueBoxIn(GameObject box)
    {
        RectTransform rectTransform = box.GetComponent<RectTransform>();
        CanvasGroup canvasGroup = box.GetComponent<CanvasGroup>();
        if (canvasGroup == null)
        {
            canvasGroup = box.AddComponent<CanvasGroup>();
        }

        Vector2 startPos = rectTransform.anchoredPosition;
        Vector2 endPos = startPos + new Vector2(boxSlideDistance, 0);

        canvasGroup.alpha = 0f;

        float elapsed = 0f;
        while (elapsed < boxFadeInDuration)
        {
            elapsed += Time.deltaTime;
            float t = elapsed / boxFadeInDuration;

            // Ease out
            t = 1f - Mathf.Pow(1f - t, 3f);

            rectTransform.anchoredPosition = Vector2.Lerp(startPos, endPos, t);
            canvasGroup.alpha = t;

            yield return null;
        }

        rectTransform.anchoredPosition = endPos;
        canvasGroup.alpha = 1f;
    }

    private IEnumerator FadeOutDialogueBox(GameObject box, float duration = -1f)
    {
        if (box == null) yield break;
        
        if (duration < 0) duration = boxFadeOutDuration;

        CanvasGroup canvasGroup = box.GetComponent<CanvasGroup>();
        if (canvasGroup == null)
        {
            canvasGroup = box.AddComponent<CanvasGroup>();
        }

        float startAlpha = canvasGroup.alpha;
        float elapsed = 0f;

        while (elapsed < duration)
        {
            // Check if object still exists before accessing it
            if (box == null || canvasGroup == null) yield break;
            
            elapsed += Time.deltaTime;
            float t = elapsed / duration;
            canvasGroup.alpha = Mathf.Lerp(startAlpha, 0f, t);
            yield return null;
        }

        // Final check before setting alpha
        if (box != null && canvasGroup != null)
        {
            canvasGroup.alpha = 0f;
        }
    }
    
    private IEnumerator FadeToAlpha(CanvasGroup canvasGroup, float targetAlpha, float duration)
    {
        if (canvasGroup == null) yield break;
        
        float startAlpha = canvasGroup.alpha;
        float elapsed = 0f;
        
        while (elapsed < duration)
        {
            if (canvasGroup == null) yield break;
            
            elapsed += Time.deltaTime;
            float t = elapsed / duration;
            canvasGroup.alpha = Mathf.Lerp(startAlpha, targetAlpha, t);
            yield return null;
        }
        
        if (canvasGroup != null)
        {
            canvasGroup.alpha = targetAlpha;
        }
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
            Debug.LogWarning("DialogueSceneController: No next scene configured!");
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

