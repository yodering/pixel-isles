using UnityEngine;

[RequireComponent(typeof(Rigidbody2D))]
[RequireComponent(typeof(Health))]
public class EnemyController : MonoBehaviour
{
    [Header("Movement")]
    [SerializeField] private float moveSpeed = 1.3f;
    [SerializeField] private float stuckCheckInterval = 0.3f;
    [SerializeField] private float stuckThreshold = 0.02f;
    [SerializeField] private float obstacleDetectionDistance = 1.0f;
    [SerializeField] private LayerMask obstacleLayer;

    [Header("Attack")]
    [SerializeField] private float attackRange = 1.5f;
    [SerializeField] private float attackCooldown = 1.5f;
    [SerializeField] private float attackDamage = 10f;
    [SerializeField] private Collider2D attackHitbox;
    [SerializeField] private float hitboxActiveTime = 0.2f;

    [Header("References")]
    private Transform player;
    private Rigidbody2D rb;
    private Animator animator;
    private Health health;

    private float lastAttackTime = 0f;
    private float lastAngle = 0f;
    private bool isAttacking = false;
    private string currentAttackAnimationParam = null;
    private Vector2 lastPosition;
    private float lastStuckCheck = 0f;
    private Vector2 unstuckDirection = Vector2.zero;
    private float unstuckTimer = 0f;
    private int consecutiveStuckFrames = 0;
    private const int maxStuckFrames = 3;
    private Vector2 desiredVelocity = Vector2.zero;

    void Start()
    {
        rb = GetComponent<Rigidbody2D>();
        animator = GetComponent<Animator>();
        health = GetComponent<Health>();
        lastPosition = transform.position;

        PlayerController playerController = GetComponent<PlayerController>();
        if (playerController != null) Destroy(playerController);

        AnimationController animationController = GetComponent<AnimationController>();
        if (animationController != null) Destroy(animationController);

        if (attackHitbox != null) attackHitbox.enabled = false;

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
        if (player == null || health.IsDead())
        {
            desiredVelocity = Vector2.zero;
            return;
        }

        float distanceToPlayer = Vector2.Distance(transform.position, player.position);
        Vector2 desiredDirection = (player.position - transform.position).normalized;
        Vector2 moveDirection = GetNavigationDirection(desiredDirection, distanceToPlayer);

        UpdateStuckState(distanceToPlayer, moveDirection);

        if (unstuckTimer > 0f) moveDirection = unstuckDirection;
        if (moveDirection.sqrMagnitude > 0.0001f) moveDirection = moveDirection.normalized;

        float angle = Mathf.Atan2(moveDirection.y, moveDirection.x) * Mathf.Rad2Deg;
        lastAngle = SnapAngleToEightDirections(angle);
        UpdateDirectionAnimations(lastAngle);

        bool playerInStrikeRange = IsPlayerWithinStrikeRange(distanceToPlayer);
        bool canAttack = playerInStrikeRange && Time.time >= lastAttackTime + attackCooldown;

        if (canAttack && !isAttacking)
        {
            desiredVelocity = Vector2.zero;
            StartAttack();
            if (animator != null) animator.SetBool("isRunning", false);
        }
        else if (!isAttacking)
        {
            desiredVelocity = moveDirection.sqrMagnitude > 0.0001f ? moveDirection * moveSpeed : Vector2.zero;
        }
        
        if (animator != null && !isAttacking)
        {
            bool isActuallyMoving = desiredVelocity.magnitude > 0.1f;
            animator.SetBool("isRunning", isActuallyMoving);
            animator.SetBool("isAttackAttacking", false);
            
            if (isActuallyMoving)
            {
                SetMovementAnimation(lastAngle);
            }
            else
            {
                ResetMovementAnimations();
            }
        }
    }

    void FixedUpdate()
    {
        if (rb != null) rb.linearVelocity = desiredVelocity;
    }

    private void StartAttack()
    {
        isAttacking = true;
        lastAttackTime = Time.time;
        if (animator != null)
        {
            animator.SetBool("isAttackAttacking", true);
            TriggerAttackAnimation();
        }
        Invoke(nameof(EnableAttackHitbox), 0.3f);
        Invoke(nameof(EndAttack), 1.0f);
    }

    private void EnableAttackHitbox()
    {
        if (attackHitbox != null)
        {
            attackHitbox.enabled = true;
            Invoke(nameof(DisableAttackHitbox), hitboxActiveTime);
        }
        else DealDirectDamage();
    }

    private void DisableAttackHitbox()
    {
        if (attackHitbox != null) attackHitbox.enabled = false;
    }

    private void EndAttack()
    {
        isAttacking = false;
        if (animator != null)
        {
            animator.SetBool("isAttackAttacking", false);
            ResetAttackAnimationFlags();
        }
    }

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

    public void OnAttackHitPlayer(Collider2D collision)
    {
        Health targetHealth = collision.GetComponent<Health>();
        if (targetHealth != null && targetHealth.IsPlayer())
        {
            targetHealth.TakeDamage(attackDamage);
            Debug.Log($"{gameObject.name} hit player for {attackDamage} damage!");
        }
    }

    private float SnapAngleToEightDirections(float angle)
    {
        if (angle < 0) angle += 360;
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

    private void UpdateDirectionAnimations(float angle)
    {
        if (animator == null) return;
        animator.SetBool("isNorth", false);
        animator.SetBool("isSouth", false);
        animator.SetBool("isEast", false);
        animator.SetBool("isWest", false);
        animator.SetBool("isNorthEast", false);
        animator.SetBool("isNorthWest", false);
        animator.SetBool("isSouthEast", false);
        animator.SetBool("isSouthWest", false);
        if (angle == 0) animator.SetBool("isEast", true);
        else if (angle == 45) animator.SetBool("isNorthEast", true);
        else if (angle == 90) animator.SetBool("isNorth", true);
        else if (angle == 135) animator.SetBool("isNorthWest", true);
        else if (angle == 180) animator.SetBool("isWest", true);
        else if (angle == 225) animator.SetBool("isSouthWest", true);
        else if (angle == 270) animator.SetBool("isSouth", true);
        else if (angle == 315) animator.SetBool("isSouthEast", true);
    }

    private void SetMovementAnimation(float angle)
    {
        if (animator == null) return;
        string direction = GetDirectionNameForAngle(angle);
        string animationKey = "Move" + direction;
        ResetMovementAnimations();
        animator.SetBool(animationKey, true);
    }

    private void ResetMovementAnimations()
    {
        if (animator == null) return;
        string[] directions = new string[] { "North", "South", "East", "West", "NorthEast", "NorthWest", "SouthEast", "SouthWest" };
        foreach (string direction in directions) animator.SetBool("Move" + direction, false);
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
        if (player == null) return false;
        if (attackHitbox == null) return fallbackDistance <= attackRange * 0.5f;
        float reach = GetHitboxReach();
        float centerDistance = Vector2.Distance(attackHitbox.transform.position, player.position);
        return centerDistance <= reach + 0.05f;
    }

    private float GetHitboxReach()
    {
        if (attackHitbox == null) return attackRange;
        float reach = attackRange;
        if (attackHitbox is BoxCollider2D box)
        {
            Vector2 scaledSize = new Vector2(box.size.x * Mathf.Abs(box.transform.lossyScale.x), box.size.y * Mathf.Abs(box.transform.lossyScale.y));
            reach = scaledSize.magnitude * 0.5f;
        }
        else reach = attackHitbox.bounds.extents.magnitude;
        return reach;
    }

    private Vector2 GetNavigationDirection(Vector2 desiredDirection, float distanceToPlayer)
    {
        RaycastHit2D directHit = Physics2D.Raycast(transform.position, desiredDirection, obstacleDetectionDistance, obstacleLayer);
        if (directHit.collider == null) return desiredDirection;
        float[] angleOffsets = { 45f, -45f, 90f, -90f, 135f, -135f };
        foreach (float offset in angleOffsets)
        {
            float angleRad = Mathf.Atan2(desiredDirection.y, desiredDirection.x) + (offset * Mathf.Deg2Rad);
            Vector2 testDirection = new Vector2(Mathf.Cos(angleRad), Mathf.Sin(angleRad));
            RaycastHit2D testHit = Physics2D.Raycast(transform.position, testDirection, obstacleDetectionDistance, obstacleLayer);
            if (testHit.collider == null) return testDirection;
        }
        return desiredDirection;
    }

    private void UpdateStuckState(float distanceToPlayer, Vector2 moveDirection)
    {
        if (isAttacking)
        {
            lastPosition = transform.position;
            lastStuckCheck = Time.time;
            consecutiveStuckFrames = 0;
            return;
        }
        if (unstuckTimer > 0f)
        {
            unstuckTimer -= Time.deltaTime;
            if (unstuckTimer <= 0f)
            {
                unstuckTimer = 0f;
                unstuckDirection = Vector2.zero;
                consecutiveStuckFrames = 0;
            }
        }
        if (Time.time < lastStuckCheck + stuckCheckInterval) return;

        Vector2 currentPosition = transform.position;
        float movedDistance = Vector2.Distance(currentPosition, lastPosition);
        bool tryingToMove = desiredVelocity.magnitude > 0.1f;

        if (movedDistance < stuckThreshold && tryingToMove)
        {
            consecutiveStuckFrames++;
            if (consecutiveStuckFrames >= maxStuckFrames)
            {
                Vector2[] escapeDirections = new Vector2[]
                {
                    new Vector2(-moveDirection.y, moveDirection.x),
                    new Vector2(moveDirection.y, -moveDirection.x),
                    -moveDirection,
                    Random.insideUnitCircle.normalized
                };
                foreach (Vector2 escapeDir in escapeDirections)
                {
                    RaycastHit2D escapeCheck = Physics2D.Raycast(transform.position, escapeDir, obstacleDetectionDistance * 0.5f, obstacleLayer);
                    if (escapeCheck.collider == null)
                    {
                        unstuckDirection = escapeDir.normalized;
                        unstuckTimer = 0.4f;
                        consecutiveStuckFrames = 0;
                        Debug.Log($"{gameObject.name} detected stuck, escaping");
                        break;
                    }
                }
            }
        }
        else consecutiveStuckFrames = 0;

        lastPosition = currentPosition;
        lastStuckCheck = Time.time;
    }

    private void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(transform.position, attackRange);

        if (player != null)
        {
            Vector2 directionToPlayer = (player.position - transform.position).normalized;
            Gizmos.color = Color.cyan;
            Gizmos.DrawRay(transform.position, directionToPlayer * obstacleDetectionDistance);
        }
    }
}
