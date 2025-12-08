using UnityEngine;

/// <summary>
/// Attack hitbox component for player melee attacks
/// The collider should be positioned in front of the player where attacks land
/// </summary>
[RequireComponent(typeof(Collider2D))]
public class PlayerAttackHitbox : MonoBehaviour
{
    [Header("Attack Settings")]
    [SerializeField] private float attackDamage = 15f;

    [Header("Vampirism Settings")]
    [Tooltip("Flat amount of health restored on each hit")]
    [SerializeField] private float lifeStealAmount = 10f;

    private Collider2D hitboxCollider;
    private Health playerHealth;
    private PlayerController playerController;

    void Start()
    {
        // Ensure this is a trigger collider
        hitboxCollider = GetComponent<Collider2D>();
        if (hitboxCollider != null)
        {
            hitboxCollider.isTrigger = true;
            hitboxCollider.enabled = false; // Disabled by default, enabled during attacks
        }

        // Find player's health and controller components
        GameObject player = GameObject.FindGameObjectWithTag("Player1");
        if (player != null)
        {
            playerHealth = player.GetComponent<Health>();
            playerController = player.GetComponent<PlayerController>();
        }
    }

    /// <summary>
    /// Enable the hitbox for an attack
    /// </summary>
    public void EnableHitbox(float duration = 0.2f)
    {
        if (hitboxCollider != null)
        {
            hitboxCollider.enabled = true;
            Invoke(nameof(DisableHitbox), duration);
        }
    }

    /// <summary>
    /// Disable the hitbox
    /// </summary>
    public void DisableHitbox()
    {
        if (hitboxCollider != null)
        {
            hitboxCollider.enabled = false;
        }
    }

    /// <summary>
    /// When the hitbox collides with an enemy, deal damage and restore health (vampirism)
    /// </summary>
    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.gameObject.tag == "Enemy")
        {
            Health enemyHealth = collision.GetComponent<Health>();
            if (enemyHealth != null && !enemyHealth.IsPlayer())
            {
                float currentHealth = enemyHealth.GetCurrentHealth();
                enemyHealth.TakeDamage(attackDamage);
                Debug.Log($"Player hit {collision.gameObject.name} for {attackDamage} damage!");

                // Track hit for ability unlock
                if (playerController != null)
                {
                    playerController.OnEnemyHit();
                }

                // Check if enemy died from this hit
                if (currentHealth > 0 && enemyHealth.GetCurrentHealth() <= 0)
                {
                    if (playerController != null)
                    {
                        playerController.OnEnemyKilled();
                    }
                }

                // Vampirism: Restore flat health on hit
                if (playerHealth != null && lifeStealAmount > 0)
                {
                    playerHealth.Heal(lifeStealAmount);
                    Debug.Log($"Life steal: Restored {lifeStealAmount} health");
                }
            }
        }
    }
}

