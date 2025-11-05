using UnityEngine;

/// <summary>
/// Improved enemy AI that moves toward the player and attacks with proper animations
/// Attacks are triggered via animation events for proper timing
/// </summary>
[RequireComponent(typeof(Rigidbody2D))]
[RequireComponent(typeof(Health))]
public class EnemyController : MonoBehaviour
{
    [Header("Movement")]
    [SerializeField] private float moveSpeed = 1.3f; // Slower than player (player is 2.0)
    [SerializeField] private float stoppingDistance = 1.5f; // How close before stopping
    [SerializeField] private float stuckCheckInterval = 1.0f; // How often to check if stuck
    [SerializeField] private float stuckThreshold = 0.05f; // Minimum movement to not be stuck

    [Header("Attack")]
    [SerializeField] private float attackRange = 1.5f;
    [SerializeField] private float attackCooldown = 1.5f;
    [SerializeField] private float attackDamage = 10f;
    
    [Header("Attack Hitbox")]
    [Tooltip("The collider used for melee attacks (should be a child object with trigger collider)")]
    [SerializeField] private Collider2D attackHitbox;
    [SerializeField] private float hitboxActiveTime = 0.2f; // How long the hitbox stays active

    [Header("References")]
    private Transform player;
    private Rigidbody2D rb;
    private Animator animator;
    private Health health;

    private float lastAttackTime = 0f;
    private float lastAngle = 0f;
    private bool isAttacking = false;
    private string currentAttackAnimationParam = null;
    
    // Stuck detection
    private Vector2 lastPosition;
    private float lastStuckCheck = 0f;
    private Vector2 unstuckDirection = Vector2.zero;
    private float unstuckTimer = 0f;

    void Start()
    {
        rb = GetComponent<Rigidbody2D>();
        animator = GetComponent<Animator>();
        health = GetComponent<Health>();
        lastPosition = transform.position;

        // Ensure player-only scripts are DESTROYED on enemies (not just disabled)
        PlayerController playerController = GetComponent<PlayerController>();
        if (playerController != null)
        {
            Debug.Log($"{gameObject.name}: Removing PlayerController component");
            Destroy(playerController);
        }

        AnimationController animationController = GetComponent<AnimationController>();
        if (animationController != null)
        {
˝            Debug.Log($"{gameObject.name}: Removing AnimationController component");
            Destroy(animationController);
        }

        // Disable attack hitbox by default
        if (attackHitbox != null)
        {
            attackHitbox.enabled = false;
        }

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
        Vector2 desiredDirection = (player.position - transform.position).normalized;

        // Update stuck detection state
        UpdateStuckState(distanceToPlayer, desiredDirection);

        Vector2 moveDirection = unstuckTimer > 0f ? unstuckDirection : desiredDirection;
        if (moveDirection.sqrMagnitude <= 0.0001f)
        {
            moveDirection = desiredDirection;
        }
        else
        {
            moveDirection = moveDirection.normalized;
        }

        float angle = Mathf.Atan2(moveDirection.y, moveDirection.x) * Mathf.Rad2Deg;
        lastAngle = SnapAngleToEightDirections(angle);

        // Update directional animations
        UpdateDirectionAnimations(lastAngle);

        bool playerInStrikeRange = IsPlayerWithinStrikeRange(distanceToPlayer);

        // Check if attack hitbox is in range and cooldown is ready
        bool canAttack = playerInStrikeRange && Time.time >= lastAttackTime + attackCooldown;

        // Check if in attack range
        if (canAttack && !isAttacking)
        {
            // Stop moving
            rb.linearVelocity = Vector2.zero;
            StartAttack();

            // Set idle animation (not running)
            if (animator != null)
            {
                animator.SetBool("isRunning", false);
            }
        }
        else if (distanceToPlayer > stoppingDistance && !isAttacking)
        {
            // Move toward player or use unstuck direction
            Vector2 desiredVelocity = moveDirection.sqrMagnitude > 0.0001f ? moveDirection * moveSpeed : Vector2.zero;
            rb.linearVelocity = desiredVelocity;

            // Set running animation
            if (animator != null)
            {
                animator.SetBool("isRunning", true);
                animator.SetBool("isAttackAttacking", false);
            }
        }
        else if (!isAttacking)
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
    /// Start the attack animation
    /// </summary>
    private void StartAttack()
    {
        isAttacking = true;
        lastAttackTime = Time.time;

        // Trigger attack animation
        if (animator != null)
        {
            animator.SetBool("isAttackAttacking", true);
            TriggerAttackAnimation();
        }

        // Enable hitbox after a short delay (mid-attack animation)
        Invoke(nameof(EnableAttackHitbox), 0.3f);
        
        // Reset attack state after animation completes
        Invoke(nameof(EndAttack), 1.0f);
    }

    /// <summary>
    /// Enable the attack hitbox during attack animation
    /// </summary>
    private void EnableAttackHitbox()
    {
        if (attackHitbox != null)
        {
            attackHitbox.enabled = true;
            Invoke(nameof(DisableAttackHitbox), hitboxActiveTime);
        }
        else
        {
            // Fallback: direct damage if no hitbox is set up
            DealDirectDamage();
        }
    }

    /// <summary>
    /// Disable the attack hitbox
    /// </summary>
    private void DisableAttackHitbox()
    {
        if (attackHitbox != null)
        {
            attackHitbox.enabled = false;
        }
    }

    /// <summary>
    /// End the attack state
    /// </summary>
    private void EndAttack()
    {
        isAttacking = false;
        
        if (animator != null)
        {
            animator.SetBool("isAttackAttacking", false);
            ResetAttackAnimationFlags();
        }
    }

    /// <summary>
    /// Direct damage fallback (used when no hitbox collider is set up)
    /// </summary>
    private void DealDirectDamage()
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
    /// Called when attack hitbox collides with player (requires hitbox setup)
    /// </summary>
    public void OnAttackHitPlayer(Collider2D collision)
    {
        Health targetHealth = collision.GetComponent<Health>();
        if (targetHealth != null && targetHealth.IsPlayer())
        {
            targetHealth.TakeDamage(attackDamage);
            Debug.Log($"{gameObject.name} hit player for {attackDamage} damage!");
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

    private void TriggerAttackAnimation()
    {
        if (animator == null) return;

        ResetAttackAnimationFlags();

        string direction = GetDirectionNameForAngle(lastAngle);
        string attackBase = Random.value < 0.5f ? "AttackAttack" : "Attack2";
        currentAttackAnimationParam = attackBase + direction;

        animator.SetBool(currentAttackAnimationParam, true);
    }

    private void ResetAttackAnimationFlags()
    {
        if (animator == null) return;

        if (!string.IsNullOrEmpty(currentAttackAnimationParam))
        {
            animator.SetBool(currentAttackAnimationParam, false);
            currentAttackAnimationParam = null;
        }

        animator.SetBool("isAttackRunning", false);
    }

    private string GetDirectionNameForAngle(float angle)
    {
        switch ((int)angle)
        {
            case 0: return "East";
            case 45: return "NorthEast";
            case 90: return "North";
            case 135: return "NorthWest";
            case 180: return "West";
            case 225: return "SouthWest";
            case 270: return "South";
            case 315: return "SouthEast";
            default: return "East";
        }
    }

    private bool IsPlayerWithinStrikeRange(float fallbackDistance)
    {
        if (player == null)
        {
            return false;
        }

        if (attackHitbox == null)
        {
            return fallbackDistance <= attackRange * 0.5f;
        }

        float reach = GetHitboxReach();
        float centerDistance = Vector2.Distance(attackHitbox.transform.position, player.position);

        return centerDistance <= reach + 0.05f;
    }

    private float GetHitboxReach()
    {
        if (attackHitbox == null)
        {
            return attackRange;
        }

        float reach = attackRange;

        if (attackHitbox is BoxCollider2D box)
        {
            Vector2 scaledSize = new Vector2(box.size.x * Mathf.Abs(box.transform.lossyScale.x), box.size.y * Mathf.Abs(box.transform.lossyScale.y));
            reach = scaledSize.magnitude * 0.5f;
        }
        else
        {
            reach = attackHitbox.bounds.extents.magnitude;
        }

        return reach;
    }

    private void UpdateStuckState(float distanceToPlayer, Vector2 desiredDirection)
    {
        if (isAttacking)
        {
            lastPosition = transform.position;
            lastStuckCheck = Time.time;
            return;
        }

        if (unstuckTimer > 0f)
        {
            unstuckTimer -= Time.deltaTime;
            if (unstuckTimer <= 0f)
            {
                unstuckTimer = 0f;
                unstuckDirection = Vector2.zero;
            }
        }

        if (Time.time < lastStuckCheck + stuckCheckInterval)
        {
            return;
        }

        Vector2 currentPosition = transform.position;
        float movedDistance = Vector2.Distance(currentPosition, lastPosition);

        // Only trigger unstuck if we're truly stuck (trying to move but not moving much)
        if (movedDistance < stuckThreshold && distanceToPlayer > stoppingDistance + 0.5f)
        {
            // Try to move perpendicular to desired direction
            Vector2 perpendicular = new Vector2(-desiredDirection.y, desiredDirection.x);
            
            if (Random.value > 0.5f)
            {
                perpendicular = -perpendicular;
            }

            unstuckDirection = perpendicular.normalized;
            unstuckTimer = 0.5f; // Shorter unstuck duration
            
            Debug.Log($"{gameObject.name} detected stuck, trying perpendicular movement");
        }

        lastPosition = currentPosition;
        lastStuckCheck = Time.time;
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
