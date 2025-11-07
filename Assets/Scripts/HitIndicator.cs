using UnityEngine;

/// <summary>
/// Visual indicator for damage dealt or received
/// Creates floating damage numbers and screen flash effects
/// </summary>
public class HitIndicator : MonoBehaviour
{
    [Header("Damage Number Settings")]
    [SerializeField] private GameObject damageNumberPrefab;
    [SerializeField] private Vector3 damageNumberOffset = new Vector3(0, 1f, 0);
    [SerializeField] private Color damageNumberColor = Color.red;
    [SerializeField] private float damageNumberDuration = 1f;
    [SerializeField] private float damageNumberSpeed = 2f;

    [Header("Hit Flash Settings")]
    [SerializeField] private SpriteRenderer spriteRenderer;
    [SerializeField] private Color hitFlashColor = Color.white;
    [SerializeField] private float hitFlashDuration = 0.1f;

    [Header("Screen Shake Settings")]
    [SerializeField] private bool enableScreenShake = true;
    [SerializeField] private float screenShakeIntensity = 0.1f;
    [SerializeField] private float screenShakeDuration = 0.1f;

    private Health health;
    private Color originalColor;
    private bool isFlashing = false;
    private float lastKnownHealth;

    void Start()
    {
        health = GetComponent<Health>();

        if (spriteRenderer == null)
        {
            spriteRenderer = GetComponent<SpriteRenderer>();
        }

        if (spriteRenderer != null)
        {
            originalColor = spriteRenderer.color;
        }

        // Subscribe to health changes
        if (health != null)
        {
            lastKnownHealth = health.GetCurrentHealth();
            health.OnHealthChanged.AddListener(OnHealthChanged);
        }
    }

    void OnHealthChanged(float currentHealth)
    {
        // Calculate actual damage taken
        float damageTaken = lastKnownHealth - currentHealth;
        lastKnownHealth = currentHealth;

        // Only show indicator if damage was actually taken
        if (damageTaken <= 0) return;

        // Show damage number
        if (damageNumberPrefab != null)
        {
            ShowDamageNumber(damageTaken);
        }
        else
        {
            // Create a simple floating text if no prefab
            CreateFloatingText(damageTaken.ToString("0"));
        }

        // Flash sprite
        if (!isFlashing && spriteRenderer != null)
        {
            StartCoroutine(HitFlash());
        }

        // Screen shake for player hits
        if (health.IsPlayer() && enableScreenShake)
        {
            CameraShake.Instance?.Shake(screenShakeIntensity, screenShakeDuration);
        }
    }

    void ShowDamageNumber(float damage)
    {
        Vector3 spawnPosition = transform.position + damageNumberOffset;
        GameObject damageNumber = Instantiate(damageNumberPrefab, spawnPosition, Quaternion.identity);

        // If it has a TextMesh or TextMeshPro component, set the damage value
        TextMesh textMesh = damageNumber.GetComponent<TextMesh>();
        if (textMesh != null)
        {
            textMesh.text = damage.ToString("0");
            textMesh.color = damageNumberColor;
        }

        // Add independent animator component to the damage number itself
        DamageNumberAnimator animator = damageNumber.AddComponent<DamageNumberAnimator>();
        animator.Initialize(damageNumberDuration, damageNumberSpeed);
    }

    void CreateFloatingText(string text)
    {
        // Create a simple 3D text object
        GameObject floatingText = new GameObject("DamageNumber");
        floatingText.transform.position = transform.position + damageNumberOffset;

        TextMesh textMesh = floatingText.AddComponent<TextMesh>();
        textMesh.text = text;
        textMesh.fontSize = 30;
        textMesh.color = damageNumberColor;
        textMesh.anchor = TextAnchor.MiddleCenter;
        textMesh.alignment = TextAlignment.Center;
        textMesh.characterSize = 0.1f;

        // Make it face the camera
        MeshRenderer meshRenderer = floatingText.GetComponent<MeshRenderer>();
        if (meshRenderer != null)
        {
            meshRenderer.sortingOrder = 100;
        }

        // Add independent animator component to the damage number itself
        DamageNumberAnimator animator = floatingText.AddComponent<DamageNumberAnimator>();
        animator.Initialize(damageNumberDuration, damageNumberSpeed);
    }


    System.Collections.IEnumerator HitFlash()
    {
        if (spriteRenderer == null) yield break;

        isFlashing = true;
        spriteRenderer.color = hitFlashColor;

        yield return new WaitForSeconds(hitFlashDuration);

        spriteRenderer.color = originalColor;
        isFlashing = false;
    }

    void OnDestroy()
    {
        if (health != null)
        {
            health.OnHealthChanged.RemoveListener(OnHealthChanged);
        }
    }

    /// <summary>
    /// Public method to trigger hit indicator from external sources
    /// </summary>
    public void TriggerHitEffect(float damage)
    {
        if (damageNumberPrefab != null)
        {
            ShowDamageNumber(damage);
        }
        else
        {
            CreateFloatingText(damage.ToString("0"));
        }

        if (!isFlashing)
        {
            StartCoroutine(HitFlash());
        }
    }
}
