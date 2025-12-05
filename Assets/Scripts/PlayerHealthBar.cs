using UnityEngine;
using UnityEngine.UI;
using TMPro;

/// <summary>
/// Visual health bar for the player
/// Shows a nice filled bar that updates based on player health
/// </summary>
public class PlayerHealthBar : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private Health playerHealth;
    [SerializeField] private Image healthBarFill;
    [SerializeField] private TextMeshProUGUI healthText;

    [Header("Visual Settings")]
    [SerializeField] private bool showHealthText = true;
    [SerializeField] private bool showPercentage = false;
    [SerializeField] private Color fullHealthColor = new Color(0.2f, 0.8f, 0.2f, 1f); // Green
    [SerializeField] private Color midHealthColor = new Color(0.9f, 0.7f, 0.2f, 1f); // Yellow
    [SerializeField] private Color lowHealthColor = new Color(0.9f, 0.2f, 0.2f, 1f); // Red
    [SerializeField] private float lowHealthThreshold = 0.3f;
    [SerializeField] private float midHealthThreshold = 0.6f;

    [Header("Animation")]
    [SerializeField] private bool smoothTransition = true;
    [SerializeField] private float transitionSpeed = 5f;

    [Header("Bar Style")]
    [SerializeField] private BarScaleMode scaleMode = BarScaleMode.FillAmount;

    public enum BarScaleMode
    {
        FillAmount,     // Traditional filled bar (default)
        Width,          // Bar shrinks in width
        Both            // Both fill and width change
    }

    private float targetFillAmount = 1f;
    private float targetWidth = 1f;
    private Color targetColor;
    private RectTransform fillRectTransform;

    void Start()
    {
        // Auto-find player health if not assigned
        if (playerHealth == null)
        {
            GameObject player = GameObject.FindGameObjectWithTag("Player1");
            if (player != null)
            {
                playerHealth = player.GetComponent<Health>();
            }
        }

        // Get RectTransform for width scaling
        if (healthBarFill != null)
        {
            fillRectTransform = healthBarFill.GetComponent<RectTransform>();
        }

        // Set initial color
        targetColor = fullHealthColor;
        if (healthBarFill != null)
        {
            healthBarFill.color = targetColor;
        }

        // Subscribe to health changes
        if (playerHealth != null)
        {
            playerHealth.OnHealthChanged.AddListener(UpdateHealthBar);
        }
        else
        {
            Debug.LogError("PlayerHealthBar: No player health component found!");
        }

        // Delay initialization to let Health component initialize first
        Invoke(nameof(InitializeHealthBar), 0.1f);
    }

    /// <summary>
    /// Initialize the health bar after a short delay
    /// This ensures the Health component has set its currentHealth value
    /// </summary>
    private void InitializeHealthBar()
    {
        if (playerHealth != null)
        {
            UpdateHealthBar(playerHealth.GetCurrentHealth());
        }
    }

    void Update()
    {
        // Smooth transition for health bar
        if (smoothTransition && healthBarFill != null)
        {
            // Update fill amount
            if (scaleMode == BarScaleMode.FillAmount || scaleMode == BarScaleMode.Both)
            {
                healthBarFill.fillAmount = Mathf.Lerp(healthBarFill.fillAmount, targetFillAmount, Time.deltaTime * transitionSpeed);
            }

            // Update width scale
            if (scaleMode == BarScaleMode.Width || scaleMode == BarScaleMode.Both)
            {
                if (fillRectTransform != null)
                {
                    Vector3 scale = fillRectTransform.localScale;
                    scale.x = Mathf.Lerp(scale.x, targetWidth, Time.deltaTime * transitionSpeed);
                    fillRectTransform.localScale = scale;
                }
            }

            // Update color
            healthBarFill.color = Color.Lerp(healthBarFill.color, targetColor, Time.deltaTime * transitionSpeed);
        }
    }

    /// <summary>
    /// Update the health bar when health changes
    /// </summary>
    private void UpdateHealthBar(float currentHealth)
    {
        if (playerHealth == null || healthBarFill == null) return;

        float healthPercentage = playerHealth.GetHealthPercentage();
        targetFillAmount = healthPercentage;
        targetWidth = healthPercentage;

        // Update color based on health percentage
        if (healthPercentage <= lowHealthThreshold)
        {
            targetColor = lowHealthColor;
        }
        else if (healthPercentage <= midHealthThreshold)
        {
            targetColor = midHealthColor;
        }
        else
        {
            targetColor = fullHealthColor;
        }

        // Update immediately if not smooth
        if (!smoothTransition)
        {
            // Update fill amount
            if (scaleMode == BarScaleMode.FillAmount || scaleMode == BarScaleMode.Both)
            {
                healthBarFill.fillAmount = targetFillAmount;
            }

            // Update width scale
            if (scaleMode == BarScaleMode.Width || scaleMode == BarScaleMode.Both)
            {
                if (fillRectTransform != null)
                {
                    Vector3 scale = fillRectTransform.localScale;
                    scale.x = targetWidth;
                    fillRectTransform.localScale = scale;
                }
            }

            healthBarFill.color = targetColor;
        }

        // Update text
        UpdateHealthText(currentHealth);
    }

    /// <summary>
    /// Update the health text display
    /// </summary>
    private void UpdateHealthText(float currentHealth)
    {
        if (!showHealthText || healthText == null) return;

        if (showPercentage)
        {
            float percentage = playerHealth.GetHealthPercentage() * 100f;
            healthText.text = $"{Mathf.RoundToInt(percentage)}%";
        }
        else
        {
            // Show current / max
            healthText.text = $"{Mathf.RoundToInt(currentHealth)} / {Mathf.RoundToInt(playerHealth.GetMaxHealth())}";
        }
    }

    void OnDestroy()
    {
        // Unsubscribe from events
        if (playerHealth != null)
        {
            playerHealth.OnHealthChanged.RemoveListener(UpdateHealthBar);
        }
    }
}
