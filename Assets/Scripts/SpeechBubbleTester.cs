using UnityEngine;

public class SpeechBubbleTester : MonoBehaviour
{
    [SerializeField] private SpeechBubble speechBubble;

    void Start()
    {
        if (speechBubble == null)
        {
            Debug.LogError("SpeechBubbleTester: SpeechBubble reference is not assigned!");
            return;
        }

        // Wait a moment then show text
        Invoke(nameof(ShowStartMessage), 0.5f);
    }

    void ShowStartMessage()
    {
        if (speechBubble != null)
        {
            speechBubble.ShowText("You cannot die, because you were never alive.", 4f);
        }
    }

    void Update()
    {
        // Press T to show a line on demand
        if (Input.GetKeyDown(KeyCode.T) && speechBubble != null)
        {
            speechBubble.ShowText("Welcome to hell.", 3f);
        }

        // Press Y to test with a very visible message
        if (Input.GetKeyDown(KeyCode.Y) && speechBubble != null)
        {
            speechBubble.ShowText("TEST - CAN YOU SEE THIS?", 5f);
        }
    }
}

