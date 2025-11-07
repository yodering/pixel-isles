using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// Health bar display for enemies that floats above them
/// Automatically updates based on Health component
/// </summary>
[RequireComponent(typeof(Health))]
public class EnemyHealthBar : MonoBehaviour
{
    [Header("UI References")]
    [SerializeField] private GameObject healthBarPrefab;
    [SerializeField] private Vector3 healthBarOffset = new Vector3(0, 0.05f, 0);

    private GameObject healthBarInstance;
    private Slider healthSlider;
    private Image fillImage;
    private Health health;
    private Camera mainCamera;
    private Canvas canvas;

    [Header("Visual Settings")]
    [SerializeField] private Color fullHealthColor = Color.green;
    [SerializeField] private Color mediumHealthColor = Color.yellow;
    [SerializeField] private Color lowHealthColor = Color.red;
    [SerializeField] private float mediumHealthThreshold = 0.6f;
    [SerializeField] private float lowHealthThreshold = 0.3f;

    void Start()
    {
        health = GetComponent<Health>();
        mainCamera = Camera.main;

        if (mainCamera == null)
        {
            Debug.LogWarning($"EnemyHealthBar on {gameObject.name}: No main camera found!");
        }

        // Subscribe to health changes
        if (health != null)
        {
            health.OnHealthChanged.AddListener(UpdateHealthBar);
            health.OnDeath.AddListener(OnDeath);
        }
        else
        {
            Debug.LogError($"EnemyHealthBar on {gameObject.name}: No Health component found!");
        }

        CreateHealthBar();
        Debug.Log($"EnemyHealthBar created for {gameObject.name} at enemy position {transform.position}, bar offset {healthBarOffset}");

        if (healthBarInstance != null)
        {
            Debug.Log($"Health bar world position: {healthBarInstance.transform.position}, local position: {healthBarInstance.transform.localPosition}");
        }
    }

    void CreateHealthBar()
    {
        // If no prefab is assigned, create a simple health bar
        if (healthBarPrefab == null)
        {
            CreateDefaultHealthBar();
        }
        else
        {
            healthBarInstance = Instantiate(healthBarPrefab, transform, false);
            healthBarInstance.transform.localPosition = healthBarOffset;
            healthBarInstance.transform.localRotation = Quaternion.identity;

            // Get slider component
            healthSlider = healthBarInstance.GetComponentInChildren<Slider>();
            if (healthSlider != null)
            {
                fillImage = healthSlider.fillRect?.GetComponent<Image>();
            }
        }

        // Initialize the health bar
        if (health != null)
        {
            UpdateHealthBar(health.GetCurrentHealth());
        }
    }

    void CreateDefaultHealthBar()
    {
        // Create canvas for health bar
        healthBarInstance = new GameObject("HealthBar");
        healthBarInstance.transform.SetParent(transform, false); // Use world space = false for proper local positioning
        healthBarInstance.transform.localPosition = healthBarOffset;
        healthBarInstance.transform.localRotation = Quaternion.identity;

        canvas = healthBarInstance.AddComponent<Canvas>();
        canvas.renderMode = RenderMode.WorldSpace;
        canvas.sortingOrder = 10;

        // Scale the canvas properly - use tiny values
        RectTransform canvasRect = healthBarInstance.GetComponent<RectTransform>();
        canvasRect.sizeDelta = new Vector2(1f, 0.1f); // Much smaller actual size
        canvasRect.localScale = Vector3.one; // No additional scaling

        // Create background
        GameObject background = new GameObject("Background");
        background.transform.SetParent(healthBarInstance.transform, false);
        Image bgImage = background.AddComponent<Image>();
        bgImage.color = new Color(0.2f, 0.2f, 0.2f, 0.8f);

        // Create a simple white sprite for the background
        if (bgImage.sprite == null)
        {
            bgImage.sprite = CreateWhiteSprite();
        }

        RectTransform bgRect = background.GetComponent<RectTransform>();
        bgRect.anchorMin = Vector2.zero;
        bgRect.anchorMax = Vector2.one;
        bgRect.offsetMin = Vector2.zero;
        bgRect.offsetMax = Vector2.zero;

        // Create fill
        GameObject fill = new GameObject("Fill");
        fill.transform.SetParent(background.transform, false);
        fillImage = fill.AddComponent<Image>();
        fillImage.color = fullHealthColor;

        // Create a simple white sprite for the image if it doesn't have one
        if (fillImage.sprite == null)
        {
            fillImage.sprite = CreateWhiteSprite();
        }

        // Use Filled type for proper health bar behavior
        fillImage.type = Image.Type.Filled;
        fillImage.fillMethod = Image.FillMethod.Horizontal;
        fillImage.fillOrigin = (int)Image.OriginHorizontal.Left;

        RectTransform fillRect = fill.GetComponent<RectTransform>();
        fillRect.anchorMin = Vector2.zero;
        fillRect.anchorMax = Vector2.one;
        fillRect.pivot = new Vector2(0.5f, 0.5f);
        fillRect.offsetMin = new Vector2(0.02f, 0.01f); // Small padding in world units
        fillRect.offsetMax = new Vector2(-0.02f, -0.01f); // Small padding in world units

        Debug.Log($"Created fill image with sprite: {fillImage.sprite != null}");
    }

    void LateUpdate()
    {
        // Make health bar face camera and maintain position
        if (healthBarInstance != null && mainCamera != null)
        {
            // Ensure the health bar stays at the correct offset
            healthBarInstance.transform.localPosition = healthBarOffset;
            healthBarInstance.transform.rotation = mainCamera.transform.rotation;
        }
    }

    void UpdateHealthBar(float currentHealth)
    {
        if (health == null)
        {
            Debug.LogWarning("UpdateHealthBar: health is null");
            return;
        }

        float healthPercentage = health.GetHealthPercentage();
        Debug.Log($"UpdateHealthBar for {gameObject.name}: {healthPercentage * 100}% health");

        // Update slider if using prefab
        if (healthSlider != null)
        {
            healthSlider.value = healthPercentage;
            Debug.Log("Updated slider value");
        }
        // Update fill image for default bar
        else if (fillImage != null)
        {
            // Use fillAmount instead of width scaling
            fillImage.fillAmount = healthPercentage;
            Debug.Log($"Updated fillAmount to {healthPercentage}, health: {healthPercentage * 100}%");
        }
        else
        {
            Debug.LogWarning("No slider or fillImage found!");
        }

        // Update color based on health percentage
        if (fillImage != null)
        {
            if (healthPercentage > mediumHealthThreshold)
            {
                fillImage.color = fullHealthColor;
            }
            else if (healthPercentage > lowHealthThreshold)
            {
                fillImage.color = mediumHealthColor;
            }
            else
            {
                fillImage.color = lowHealthColor;
            }
            Debug.Log($"Set color to {fillImage.color}");
        }
    }

    void OnDeath()
    {
        // Hide health bar on death
        if (healthBarInstance != null)
        {
            Destroy(healthBarInstance);
        }
    }

    void OnDestroy()
    {
        // Clean up listeners
        if (health != null)
        {
            health.OnHealthChanged.RemoveListener(UpdateHealthBar);
            health.OnDeath.RemoveListener(OnDeath);
        }
    }

    /// <summary>
    /// Create a simple white sprite for UI elements
    /// </summary>
    private Sprite CreateWhiteSprite()
    {
        Texture2D texture = new Texture2D(1, 1);
        texture.SetPixel(0, 0, Color.white);
        texture.Apply();
        return Sprite.Create(texture, new Rect(0, 0, 1, 1), new Vector2(0.5f, 0.5f), 100f);
    }
}
