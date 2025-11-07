using UnityEngine;

/// <summary>
/// Singleton camera shake utility for impact feedback
/// Can be triggered from anywhere in the game
/// </summary>
public class CameraShake : MonoBehaviour
{
    public static CameraShake Instance { get; private set; }

    private Vector3 originalPosition;
    private float shakeTimeRemaining;
    private float shakePower;
    private float shakeFadeTime;
    private float shakeRotation;

    [Header("Shake Settings")]
    [SerializeField] private bool enableRotation = false;
    [SerializeField] private float maxRotation = 2f;

    void Awake()
    {
        // Singleton pattern
        if (Instance == null)
        {
            Instance = this;
        }
        else
        {
            Destroy(gameObject);
        }
    }

    void OnEnable()
    {
        originalPosition = transform.localPosition;
    }

    void LateUpdate()
    {
        if (shakeTimeRemaining > 0)
        {
            shakeTimeRemaining -= Time.deltaTime;

            float xAmount = Random.Range(-1f, 1f) * shakePower;
            float yAmount = Random.Range(-1f, 1f) * shakePower;

            transform.localPosition = originalPosition + new Vector3(xAmount, yAmount, 0);

            // Optional rotation shake
            if (enableRotation)
            {
                shakeRotation = Random.Range(-maxRotation, maxRotation) * shakePower;
                transform.localRotation = Quaternion.Euler(0, 0, shakeRotation);
            }

            // Fade out shake over time
            shakePower = Mathf.MoveTowards(shakePower, 0f, shakeFadeTime * Time.deltaTime);

            if (shakeTimeRemaining <= 0)
            {
                // Reset to original position
                transform.localPosition = originalPosition;
                if (enableRotation)
                {
                    transform.localRotation = Quaternion.identity;
                }
            }
        }
    }

    /// <summary>
    /// Trigger camera shake effect
    /// </summary>
    /// <param name="intensity">How strong the shake is (0.1 = subtle, 0.5 = strong)</param>
    /// <param name="duration">How long the shake lasts in seconds</param>
    public void Shake(float intensity, float duration)
    {
        originalPosition = transform.localPosition;
        shakePower = intensity;
        shakeTimeRemaining = duration;
        shakeFadeTime = intensity / duration;
    }

    /// <summary>
    /// Stop any ongoing shake immediately
    /// </summary>
    public void StopShake()
    {
        shakeTimeRemaining = 0;
        transform.localPosition = originalPosition;
        if (enableRotation)
        {
            transform.localRotation = Quaternion.identity;
        }
    }
}
