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
    
    private Collider2D hitboxCollider;

    void Start()
    {
        // Ensure this is a trigger collider
        hitboxCollider = GetComponent<Collider2D>();
        if (hitboxCollider != null)
        {
            hitboxCollider.isTrigger = true;
            hitboxCollider.enabled = false; // Disabled by default, enabled during attacks
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
    /// When the hitbox collides with an enemy, deal damage
    /// </summary>
    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.gameObject.tag == "Enemy")
        {
            Health enemyHealth = collision.GetComponent<Health>();
            if (enemyHealth != null && !enemyHealth.IsPlayer())
            {
                enemyHealth.TakeDamage(attackDamage);
                Debug.Log($"Player hit {collision.gameObject.name} for {attackDamage} damage!");
            }
        }
    }
}

