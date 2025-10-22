using UnityEngine;

/// <summary>
/// Component for projectiles and attacks that deal damage
/// Handles collision detection and damage application
/// </summary>
public class DamageDealer : MonoBehaviour
{
    [Header("Damage Settings")]
    [SerializeField] private float damage = 10f;
    [SerializeField] private bool destroyOnHit = true;

    [Header("Ownership")]
    [SerializeField] private bool isPlayerProjectile = true;

    [Header("Lifetime")]
    [SerializeField] private float lifetime = 5f; // Auto-destroy after 5 seconds

    private void Start()
    {
        // Destroy projectile after lifetime to prevent clutter
        Destroy(gameObject, lifetime);
    }

    /// <summary>
    /// Detect collision with entities
    /// </summary>
    private void OnTriggerEnter2D(Collider2D other)
    {
        // Get health component from the thing we hit
        Health targetHealth = other.GetComponent<Health>();

        if (targetHealth == null) return;

        // Don't allow friendly fire
        if (isPlayerProjectile && targetHealth.IsPlayer()) return;
        if (!isPlayerProjectile && !targetHealth.IsPlayer()) return;

        // Apply damage
        targetHealth.TakeDamage(damage);

        Debug.Log($"Projectile hit {other.gameObject.name} for {damage} damage!");

        // Destroy projectile on hit
        if (destroyOnHit)
        {
            Destroy(gameObject);
        }
    }

    // Public setters for spawning
    public void SetDamage(float newDamage) => damage = newDamage;
    public void SetIsPlayerProjectile(bool isPlayer) => isPlayerProjectile = isPlayer;
    public void SetDestroyOnHit(bool shouldDestroy) => destroyOnHit = shouldDestroy;
}
