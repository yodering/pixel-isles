using UnityEngine;
using UnityEngine.Events;

/// <summary>
/// Health component for all entities (player and enemies)
/// Handles damage, death, and health management
/// </summary>
public class Health : MonoBehaviour
{
    [Header("Health Settings")]
    [SerializeField] private float maxHealth = 100f;
    [SerializeField] private float currentHealth;

    [Header("Entity Type")]
    [SerializeField] private bool isPlayer = false;

    [Header("Events")]
    public UnityEvent<float> OnHealthChanged;
    public UnityEvent OnDeath;

    private bool isDead = false;
    private Animator animator;
    private SpriteRenderer spriteRenderer;
    private float fadeOutDuration = 0.8f;
    private float fadeOutTimer = 0f;

    void Start()
    {
        currentHealth = maxHealth;
        animator = GetComponent<Animator>();
        spriteRenderer = GetComponent<SpriteRenderer>();
    }

    /// <summary>
    /// Apply damage to this entity
    /// </summary>
    public void TakeDamage(float damage)
    {
        if (isDead) return;

        currentHealth -= damage;
        currentHealth = Mathf.Max(0, currentHealth); // Clamp to 0

        OnHealthChanged?.Invoke(currentHealth);

        Debug.Log($"{gameObject.name} took {damage} damage. Health: {currentHealth}/{maxHealth}");

        // Trigger damage animation if available
        if (animator != null)
        {
            animator.SetBool("isTakeDamage", true);
            // Reset after a short delay
            Invoke(nameof(ResetDamageAnimation), 0.3f);
        }

        if (currentHealth <= 0)
        {
            Die();
        }
    }

    /// <summary>
    /// Heal this entity
    /// </summary>
    public void Heal(float amount)
    {
        if (isDead) return;

        currentHealth += amount;
        currentHealth = Mathf.Min(currentHealth, maxHealth); // Clamp to max

        OnHealthChanged?.Invoke(currentHealth);

        Debug.Log($"{gameObject.name} healed {amount}. Health: {currentHealth}/{maxHealth}");
    }

    /// <summary>
    /// Handle entity death
    /// </summary>
    private void Die()
    {
        if (isDead) return;

        isDead = true;

        Debug.Log($"{gameObject.name} died!");

        OnDeath?.Invoke();

        // Trigger death animation
        if (animator != null)
        {
            animator.SetBool("isDie", true);
        }

        // Disable movement and controls
        var controller = GetComponent<PlayerController>();
        if (controller != null)
        {
            controller.enabled = false;
        }

        var enemyController = GetComponent<EnemyController>();
        if (enemyController != null)
        {
            enemyController.enabled = false;
        }

        // Disable collision
        var collider = GetComponent<Collider2D>();
        if (collider != null)
        {
            collider.enabled = false;
        }

        // Start fade out and destroy after shorter delay
        if (!isPlayer)
        {
            fadeOutTimer = 0f;
            Destroy(gameObject, fadeOutDuration);
        }
    }

    void Update()
    {
        // Handle fade out effect when dead
        if (isDead && !isPlayer && spriteRenderer != null)
        {
            fadeOutTimer += Time.deltaTime;
            float alpha = Mathf.Lerp(1f, 0f, fadeOutTimer / fadeOutDuration);
            Color color = spriteRenderer.color;
            color.a = alpha;
            spriteRenderer.color = color;
        }
    }

    private void ResetDamageAnimation()
    {
        if (animator != null)
        {
            animator.SetBool("isTakeDamage", false);
        }
    }

    // Public getters
    public float GetCurrentHealth() => currentHealth;
    public float GetMaxHealth() => maxHealth;
    public float GetHealthPercentage() => currentHealth / maxHealth;
    public bool IsDead() => isDead;
    public bool IsPlayer() => isPlayer;
}
