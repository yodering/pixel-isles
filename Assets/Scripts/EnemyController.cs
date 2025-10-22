using UnityEngine;

/// <summary>
/// Simple enemy AI that moves toward the player and attacks
/// </summary>
[RequireComponent(typeof(Rigidbody2D))]
[RequireComponent(typeof(Health))]
public class EnemyController : MonoBehaviour
{
    [Header("Movement")]
    [SerializeField] private float moveSpeed = 1.5f;
    [SerializeField] private float stoppingDistance = 1.5f; // How close before stopping

    [Header("Attack")]
    [SerializeField] private float attackRange = 1.5f;
    [SerializeField] private float attackCooldown = 1.5f;
    [SerializeField] private float attackDamage = 10f;

    [Header("References")]
    private Transform player;
    private Rigidbody2D rb;
    private Animator animator;
    private Health health;

    private float lastAttackTime = 0f;
    private float lastAngle = 0f;

    void Start()
    {
        rb = GetComponent<Rigidbody2D>();
        animator = GetComponent<Animator>();
        health = GetComponent<Health>();

        // Find the player
        GameObject playerObject = GameObject.FindGameObjectWithTag("Player1");
        if (playerObject != null)
        {
            player = playerObject.transform;
        }
        else
        {
            Debug.LogWarning("EnemyController: No player found with 'Player1' tag!");
        }
    }

    void Update()
    {
        if (player == null || health.IsDead()) return;

        float distanceToPlayer = Vector2.Distance(transform.position, player.position);

        // Calculate direction to player
        Vector2 directionToPlayer = (player.position - transform.position).normalized;
        float angle = Mathf.Atan2(directionToPlayer.y, directionToPlayer.x) * Mathf.Rad2Deg;
        lastAngle = SnapAngleToEightDirections(angle);

        // Update directional animations
        UpdateDirectionAnimations(lastAngle);

        // Check if in attack range
        if (distanceToPlayer <= attackRange)
        {
            // Stop moving
            rb.linearVelocity = Vector2.zero;

            // Attack if cooldown is ready
            if (Time.time >= lastAttackTime + attackCooldown)
            {
                AttackPlayer();
                lastAttackTime = Time.time;
            }

            // Set attack animation
            if (animator != null)
            {
                animator.SetBool("isRunning", false);
                animator.SetBool("isAttackAttacking", true);
            }
        }
        else if (distanceToPlayer > stoppingDistance)
        {
            // Move toward player
            rb.linearVelocity = directionToPlayer * moveSpeed;

            // Set running animation
            if (animator != null)
            {
                animator.SetBool("isRunning", true);
                animator.SetBool("isAttackAttacking", false);
            }
        }
        else
        {
            // Stop moving but don't attack (too far)
            rb.linearVelocity = Vector2.zero;

            if (animator != null)
            {
                animator.SetBool("isRunning", false);
                animator.SetBool("isAttackAttacking", false);
            }
        }
    }

    /// <summary>
    /// Attack the player (melee)
    /// </summary>
    private void AttackPlayer()
    {
        if (player == null) return;

        float distanceToPlayer = Vector2.Distance(transform.position, player.position);

        if (distanceToPlayer <= attackRange)
        {
            Health playerHealth = player.GetComponent<Health>();
            if (playerHealth != null)
            {
                playerHealth.TakeDamage(attackDamage);
                Debug.Log($"{gameObject.name} attacked player for {attackDamage} damage!");
            }
        }
    }

    /// <summary>
    /// Snap angle to 8 cardinal directions (same as player)
    /// </summary>
    private float SnapAngleToEightDirections(float angle)
    {
        // Normalize angle to 0-360
        if (angle < 0) angle += 360;

        // Snap to nearest 45-degree increment
        float[] directions = { 0, 45, 90, 135, 180, 225, 270, 315 };
        float closestAngle = 0;
        float minDifference = 360;

        foreach (float dir in directions)
        {
            float difference = Mathf.Abs(Mathf.DeltaAngle(angle, dir));
            if (difference < minDifference)
            {
                minDifference = difference;
                closestAngle = dir;
            }
        }

        return closestAngle;
    }

    /// <summary>
    /// Update animator direction booleans
    /// </summary>
    private void UpdateDirectionAnimations(float angle)
    {
        if (animator == null) return;

        // Reset all directions
        animator.SetBool("isNorth", false);
        animator.SetBool("isSouth", false);
        animator.SetBool("isEast", false);
        animator.SetBool("isWest", false);
        animator.SetBool("isNorthEast", false);
        animator.SetBool("isNorthWest", false);
        animator.SetBool("isSouthEast", false);
        animator.SetBool("isSouthWest", false);

        // Set appropriate direction
        if (angle == 0) animator.SetBool("isEast", true);
        else if (angle == 45) animator.SetBool("isNorthEast", true);
        else if (angle == 90) animator.SetBool("isNorth", true);
        else if (angle == 135) animator.SetBool("isNorthWest", true);
        else if (angle == 180) animator.SetBool("isWest", true);
        else if (angle == 225) animator.SetBool("isSouthWest", true);
        else if (angle == 270) animator.SetBool("isSouth", true);
        else if (angle == 315) animator.SetBool("isSouthEast", true);
    }

    // Visualize attack range in editor
    private void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(transform.position, attackRange);

        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(transform.position, stoppingDistance);
    }
}
