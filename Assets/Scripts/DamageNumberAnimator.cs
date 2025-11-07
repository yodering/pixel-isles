using UnityEngine;

/// <summary>
/// Independent component that animates and destroys damage numbers
/// Runs independently from the entity that spawned it, ensuring it always completes
/// </summary>
public class DamageNumberAnimator : MonoBehaviour
{
    private float duration = 1f;
    private float speed = 2f;
    private float elapsed = 0f;
    private Vector3 startPosition;
    private TextMesh textMesh;

    public void Initialize(float animDuration, float moveSpeed)
    {
        duration = animDuration;
        speed = moveSpeed;
        startPosition = transform.position;
        textMesh = GetComponent<TextMesh>();

        // Safety: destroy after duration even if animation fails
        Destroy(gameObject, duration + 0.5f);
    }

    void Update()
    {
        elapsed += Time.deltaTime;
        float progress = elapsed / duration;

        if (progress >= 1f)
        {
            // Animation complete, destroy immediately
            Destroy(gameObject);
            return;
        }

        // Move upward
        transform.position = startPosition + Vector3.up * (speed * elapsed);

        // Fade out
        if (textMesh != null)
        {
            Color color = textMesh.color;
            color.a = 1f - progress;
            textMesh.color = color;
        }

        // Scale up slightly then down
        float scale = 1f + Mathf.Sin(progress * Mathf.PI) * 0.3f;
        transform.localScale = Vector3.one * scale;

        // Face camera
        if (Camera.main != null)
        {
            transform.rotation = Camera.main.transform.rotation;
        }
    }
}
