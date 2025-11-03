using UnityEngine;

/// <summary>
/// Attack hitbox component for enemies
/// The collider should be positioned in front of the enemy where attacks land
/// </summary>
[RequireComponent(typeof(Collider2D))]
public class EnemyAttackHitbox : MonoBehaviour
{
    private EnemyController enemyController;
    private Collider2D hitboxCollider;

    void Start()
    {
        // Get the enemy controller from parent
        enemyController = GetComponentInParent<EnemyController>();
        
        if (enemyController == null)
        {
            Debug.LogError("EnemyAttackHitbox: No EnemyController found in parent!");
        }

        // Ensure this is a trigger collider
        hitboxCollider = GetComponent<Collider2D>();
        if (hitboxCollider != null)
        {
            hitboxCollider.isTrigger = true;
        }
    }

    /// <summary>
    /// When the hitbox collides with something, notify the enemy controller
    /// </summary>
    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (enemyController != null)
        {
            enemyController.OnAttackHitPlayer(collision);
        }
    }
}

