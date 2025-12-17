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
    [SerializeField] private float dialogueDisplayDuration = 3f;
    [SerializeField] private float timeBetweenLines = 0.3f;
    [SerializeField] private float boxFadeInDuration = 0.5f;
    [SerializeField] private float boxSlideDistance = 50f;

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
            skipHintText.fontSize = 16; // Smaller font
            skipHintText.gameObject.SetActive(true);
        }
    }

    private IEnumerator DisplayDialogueSequence()
    {
        yield return new WaitForSeconds(0.5f); // Initial delay
        
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

        // Wait a bit before auto-transitioning
        yield return new WaitForSeconds(2f);
        TransitionToNextScene();
    }

    private IEnumerator DisplayDialogueLine(DialogueLine line)
    {
        isDisplaying = true;

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
            rectTransform.sizeDelta = new Vector2(800, 150);

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
            speakerRect.anchoredPosition = new Vector2(20, -10);
            speakerRect.sizeDelta = new Vector2(760, 30);

            TextMeshProUGUI speakerText = speakerObj.AddComponent<TextMeshProUGUI>();
            speakerText.text = line.speaker + ":";
            speakerText.fontSize = 20;
            speakerText.fontStyle = FontStyles.Bold;
            speakerText.color = line.speaker == "Voice" ? new Color(1f, 0.8f, 0.2f) : new Color(0.8f, 0.9f, 1f);

            // Dialogue text
            GameObject textObj = new GameObject("Text");
            textObj.transform.SetParent(box.transform);
            RectTransform textRect = textObj.AddComponent<RectTransform>();
            textRect.anchorMin = new Vector2(0, 0);
            textRect.anchorMax = new Vector2(1, 1);
            textRect.offsetMin = new Vector2(20, 15);
            textRect.offsetMax = new Vector2(-20, -45);

            TextMeshProUGUI dialogueText = textObj.AddComponent<TextMeshProUGUI>();
            dialogueText.text = line.text;
            dialogueText.fontSize = 18;
            dialogueText.color = new Color(0.95f, 0.95f, 0.95f);
            dialogueText.alignment = TextAlignmentOptions.TopLeft;
        }

        // Position box (stack downward from top-left)
        RectTransform boxRect = box.GetComponent<RectTransform>();
        if (boxRect != null)
        {
            boxRect.anchorMin = new Vector2(0f, 1f); // Top-left anchor
            boxRect.anchorMax = new Vector2(0f, 1f);
            boxRect.pivot = new Vector2(0, 1f); // Pivot at top-left
            float yOffset = -50f - (currentDialogueIndex * 170f); // Stack boxes downward from top
            boxRect.anchoredPosition = new Vector2(50f - boxSlideDistance, yOffset);
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

