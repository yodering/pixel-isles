using UnityEngine;
using System.Collections.Generic;

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
    
    [Header("Teleport When Stuck")]
    [SerializeField] private bool teleportWhenStuck = true;
    [SerializeField] private float stuckTimeBeforeTeleport = 2.5f; // Seconds stuck before teleporting
    [SerializeField] private float teleportMinDistance = 1.2f; // Min distance from player (closer!)
    [SerializeField] private float teleportMaxDistance = 2.5f; // Max distance from player (closer!)
    [SerializeField] private float teleportStunDuration = 3.0f; // Can't attack after teleporting
    [SerializeField] private GameObject teleportEffectPrefab; // Optional particle effect

    [Header("Advanced Pathfinding")]
    [SerializeField] private int raycastDirections = 16; // Number of directions to check for obstacles
    [SerializeField] private float wallAvoidanceWeight = 2.0f; // How strongly to avoid walls
    [SerializeField] private bool useAStarPathfinding = true; // Enable A* pathfinding for complex navigation
    [SerializeField] private float pathfindingDistance = 5.0f; // Use A* when player is farther than this
    [SerializeField] private float waypointReachedDistance = 0.5f; // Distance to consider waypoint reached

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
    private Vector2 desiredVelocity = Vector2.zero;
    private float totalStuckTime = 0f; // Track total time stuck for teleport
    private float teleportStunTimer = 0f; // Can't attack while stunned after teleport
    private bool isStunnedFromTeleport = false;
    private SpriteRenderer spriteRenderer; // Cache for visual effects

    // A* Pathfinding variables
    private List<Vector3> currentPath;
    private int currentWaypointIndex = 0;
    private float lastPathUpdateTime = 0f;

    void Start()
    {
        rb = GetComponent<Rigidbody2D>();
        animator = GetComponent<Animator>();
        health = GetComponent<Health>();
        lastPosition = transform.position;
        
        spriteRenderer = GetComponent<SpriteRenderer>();
        if (spriteRenderer == null) spriteRenderer = GetComponentInChildren<SpriteRenderer>();

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

        // Handle teleport stun timer
        if (isStunnedFromTeleport)
        {
            teleportStunTimer -= Time.deltaTime;
            if (teleportStunTimer <= 0f)
            {
                isStunnedFromTeleport = false;
                teleportStunTimer = 0f;
                // Restore normal color
                if (spriteRenderer != null) spriteRenderer.color = Color.white;
            }
        }

        float distanceToPlayer = Vector2.Distance(transform.position, player.position);
        Vector2 desiredDirection = (player.position - transform.position).normalized;

        // Determine navigation direction based on distance and pathfinding availability
        Vector2 moveDirection;
        if (useAStarPathfinding && distanceToPlayer > pathfindingDistance && AStarPathfinding.Instance != null)
        {
            moveDirection = GetPathfindingDirection(distanceToPlayer);
        }
        else
        {
            moveDirection = GetNavigationDirection(desiredDirection, distanceToPlayer);
        }

        UpdateStuckState(distanceToPlayer, moveDirection);

        if (moveDirection.sqrMagnitude > 0.0001f) moveDirection = moveDirection.normalized;

        float angle = Mathf.Atan2(moveDirection.y, moveDirection.x) * Mathf.Rad2Deg;
        lastAngle = SnapAngleToEightDirections(angle);
        UpdateDirectionAnimations(lastAngle);

        bool playerInStrikeRange = IsPlayerWithinStrikeRange(distanceToPlayer);
        // Can't attack while stunned from teleport!
        bool canAttack = playerInStrikeRange && Time.time >= lastAttackTime + attackCooldown && !isStunnedFromTeleport;

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
        // Multi-directional obstacle detection with weighted vector field approach
        Vector2 finalDirection = Vector2.zero;
        float totalWeight = 0f;

        // Cast rays in multiple directions to build a complete view of obstacles
        for (int i = 0; i < raycastDirections; i++)
        {
            float angle = (360f / raycastDirections) * i;
            float angleRad = angle * Mathf.Deg2Rad;
            Vector2 rayDirection = new Vector2(Mathf.Cos(angleRad), Mathf.Sin(angleRad));

            // Cast ray to detect obstacles
            RaycastHit2D hit = Physics2D.Raycast(transform.position, rayDirection, obstacleDetectionDistance, obstacleLayer);

            float weight = 0f;

            if (hit.collider == null)
            {
                // No obstacle - direction is safe
                // Weight by alignment with desired direction
                float alignment = Vector2.Dot(rayDirection, desiredDirection);
                weight = Mathf.Max(0, alignment); // Only positive alignment

                // Increase weight if aligned with desired direction
                if (alignment > 0.5f)
                {
                    weight *= 1.5f;
                }
            }
            else
            {
                // Obstacle detected - penalize this direction based on distance
                float distanceRatio = hit.distance / obstacleDetectionDistance;

                // Apply strong negative weight for nearby obstacles
                float obstacleAlignment = Vector2.Dot(rayDirection, desiredDirection);
                if (obstacleAlignment > 0)
                {
                    // Obstacle is in our desired direction - strongly avoid
                    weight = -wallAvoidanceWeight * (1f - distanceRatio);
                }
                else
                {
                    // Obstacle is not in desired direction - less critical
                    weight = -0.5f * wallAvoidanceWeight * (1f - distanceRatio);
                }
            }

            // Accumulate weighted direction
            finalDirection += rayDirection * weight;
            totalWeight += Mathf.Abs(weight);
        }

        // Normalize the final direction
        if (totalWeight > 0.01f && finalDirection.sqrMagnitude > 0.01f)
        {
            finalDirection = finalDirection.normalized;
        }
        else
        {
            // Fallback to desired direction if no clear path
            finalDirection = desiredDirection;
        }

        return finalDirection;
    }

    private void UpdateStuckState(float distanceToPlayer, Vector2 moveDirection)
    {
        if (isAttacking)
        {
            lastPosition = transform.position;
            lastStuckCheck = Time.time;
            totalStuckTime = 0f;
            return;
        }

        if (Time.time < lastStuckCheck + stuckCheckInterval) return;

        Vector2 currentPosition = transform.position;
        float movedDistance = Vector2.Distance(currentPosition, lastPosition);
        bool tryingToMove = desiredVelocity.magnitude > 0.1f;

        if (movedDistance < stuckThreshold && tryingToMove)
        {
            // Accumulate stuck time
            totalStuckTime += stuckCheckInterval;
            
            // Teleport after being stuck for the threshold time
            if (teleportWhenStuck && player != null && totalStuckTime >= stuckTimeBeforeTeleport)
            {
                Debug.Log($"{gameObject.name} stuck for {totalStuckTime:F1}s, teleporting near player!");
                TeleportNearPlayer();
                totalStuckTime = 0f;
            }
        }
        else
        {
            // Moving successfully - reset stuck time
            totalStuckTime = 0f;
        }

        lastPosition = currentPosition;
        lastStuckCheck = Time.time;
    }

    private void TeleportNearPlayer()
    {
        if (player == null) return;

        Vector2 playerPos = player.position;
        Vector2 teleportPos = Vector2.zero;
        bool foundValidPosition = false;

        // Try multiple angles at different distances
        float[] angles = { 180f, 150f, 210f, 120f, 240f, 90f, 270f, 60f, 300f, 45f, 315f, 30f, 330f, 0f };
        float[] distances = { teleportMinDistance, (teleportMinDistance + teleportMaxDistance) / 2f, teleportMaxDistance };
        
        // Get player's facing direction
        Vector2 playerFacing = Vector2.right;
        Rigidbody2D playerRb = player.GetComponent<Rigidbody2D>();
        if (playerRb != null && playerRb.linearVelocity.magnitude > 0.1f)
        {
            playerFacing = playerRb.linearVelocity.normalized;
        }
        float baseAngle = Mathf.Atan2(playerFacing.y, playerFacing.x) * Mathf.Rad2Deg;

        // Try systematic positions
        foreach (float dist in distances)
        {
            foreach (float angleOffset in angles)
            {
                float finalAngle = (baseAngle + angleOffset) * Mathf.Deg2Rad;
                Vector2 offset = new Vector2(Mathf.Cos(finalAngle), Mathf.Sin(finalAngle)) * dist;
                Vector2 candidatePos = playerPos + offset;

                if (IsValidTeleportPosition(candidatePos))
                {
                    teleportPos = candidatePos;
                    foundValidPosition = true;
                    break;
                }
            }
            if (foundValidPosition) break;
        }

        // Last resort: try many random positions
        if (!foundValidPosition)
        {
            for (int i = 0; i < 30; i++)
            {
                float randomAngle = Random.Range(0f, 360f) * Mathf.Deg2Rad;
                float distance = Random.Range(teleportMinDistance, teleportMaxDistance);
                Vector2 offset = new Vector2(Mathf.Cos(randomAngle), Mathf.Sin(randomAngle)) * distance;
                Vector2 candidatePos = playerPos + offset;

                if (IsValidTeleportPosition(candidatePos))
                {
                    teleportPos = candidatePos;
                    foundValidPosition = true;
                    break;
                }
            }
        }

        if (foundValidPosition)
        {
            // Spawn teleport effect at old position
            if (teleportEffectPrefab != null)
            {
                Instantiate(teleportEffectPrefab, transform.position, Quaternion.identity);
            }

            transform.position = teleportPos;
            lastPosition = teleportPos;
            if (teleportEffectPrefab != null)
            {
                Instantiate(teleportEffectPrefab, teleportPos, Quaternion.identity);
            }

            isStunnedFromTeleport = true;
            teleportStunTimer = teleportStunDuration;
            
            if (spriteRenderer != null)
            {
                spriteRenderer.color = new Color(0.5f, 0.8f, 1f, 1f);
            }
            
            StartCoroutine(TeleportStunVisualEffect());

            Debug.Log($"{gameObject.name} teleported near player to {teleportPos}, stunned for {teleportStunDuration}s");
        }
        else
        {
            Debug.LogWarning($"{gameObject.name} could not find valid teleport position inside boundaries!");
        }
    }

    private bool IsValidTeleportPosition(Vector2 position)
    {
        Collider2D obstacle = Physics2D.OverlapCircle(position, 0.4f, obstacleLayer);
        if (obstacle != null) return false;

        EdgeCollider2D[] edgeColliders = FindObjectsByType<EdgeCollider2D>(FindObjectsSortMode.None);
        if (edgeColliders.Length == 0) return true;
        foreach (EdgeCollider2D edge in edgeColliders)
        {
            if (IsPointInsideEdgeCollider(position, edge))
            {
                return true;
            }
        }

        return false;
    }

    /// <summary>
    /// Check if a point is inside the polygon formed by an EdgeCollider2D
    /// Uses the ray casting algorithm (count edge crossings)
    /// </summary>
    private bool IsPointInsideEdgeCollider(Vector2 point, EdgeCollider2D edge)
    {
        if (edge == null) return false;

        // Get world-space points of the edge collider
        Vector2[] localPoints = edge.points;
        Vector2[] worldPoints = new Vector2[localPoints.Length];
        
        for (int i = 0; i < localPoints.Length; i++)
        {
            worldPoints[i] = edge.transform.TransformPoint(localPoints[i]);
        }

        // Ray casting algorithm: count how many times a horizontal ray crosses the polygon
        int crossings = 0;
        int pointCount = worldPoints.Length;

        for (int i = 0; i < pointCount; i++)
        {
            Vector2 p1 = worldPoints[i];
            Vector2 p2 = worldPoints[(i + 1) % pointCount];

            // Check if the ray from 'point' going right crosses this edge
            if (RayCrossesEdge(point, p1, p2))
            {
                crossings++;
            }
        }

        // Odd number of crossings = inside, even = outside
        return (crossings % 2) == 1;
    }

    /// <summary>
    /// Check if a horizontal ray from 'point' going right crosses the edge p1-p2
    /// </summary>
    private bool RayCrossesEdge(Vector2 point, Vector2 p1, Vector2 p2)
    {
        // Ensure p1 is below p2 for consistent checking
        if (p1.y > p2.y)
        {
            Vector2 temp = p1;
            p1 = p2;
            p2 = temp;
        }

        // Point is outside the vertical range of this edge
        if (point.y <= p1.y || point.y > p2.y)
        {
            return false;
        }

        // Calculate x-coordinate where the ray intersects the edge
        float slope = (p2.x - p1.x) / (p2.y - p1.y);
        float intersectX = p1.x + (point.y - p1.y) * slope;

        // Ray crosses if intersection is to the right of the point
        return point.x < intersectX;
    }

    /// <summary>
    /// Visual effect while stunned after teleporting - pulsing cyan color
    /// </summary>
    private System.Collections.IEnumerator TeleportStunVisualEffect()
    {
        if (spriteRenderer == null) yield break;

        Color stunColor1 = new Color(0.5f, 0.8f, 1f, 1f);
        Color stunColor2 = new Color(0.7f, 0.9f, 1f, 1f);
        float pulseSpeed = 3f;
        
        while (isStunnedFromTeleport && teleportStunTimer > 0)
        {
            float t = (Mathf.Sin(Time.time * pulseSpeed) + 1f) / 2f;
            spriteRenderer.color = Color.Lerp(stunColor1, stunColor2, t);
            yield return null;
        }
        
        spriteRenderer.color = Color.white;
    }

    private Vector2 GetPathfindingDirection(float distanceToPlayer)
    {
        if (AStarPathfinding.Instance == null)
        {
            return (player.position - transform.position).normalized;
        }

        // Update path periodically
        float pathUpdateInterval = AStarPathfinding.Instance.GetPathUpdateInterval();
        if (currentPath == null || Time.time >= lastPathUpdateTime + pathUpdateInterval)
        {
            currentPath = AStarPathfinding.Instance.FindPath(transform.position, player.position);
            currentWaypointIndex = 0;
            lastPathUpdateTime = Time.time;

            // If no path found, fall back to direct navigation
            if (currentPath == null || currentPath.Count == 0)
            {
                Vector2 desiredDirection = (player.position - transform.position).normalized;
                return GetNavigationDirection(desiredDirection, distanceToPlayer);
            }
        }

        // Follow the path
        if (currentPath != null && currentPath.Count > 0)
        {
            // Check if we've reached the current waypoint
            if (currentWaypointIndex < currentPath.Count)
            {
                Vector3 currentWaypoint = currentPath[currentWaypointIndex];
                float distanceToWaypoint = Vector2.Distance(transform.position, currentWaypoint);

                if (distanceToWaypoint < waypointReachedDistance)
                {
                    currentWaypointIndex++;
                }

                // If we still have waypoints, move towards the current one
                if (currentWaypointIndex < currentPath.Count)
                {
                    Vector2 directionToWaypoint = (currentPath[currentWaypointIndex] - transform.position).normalized;

                    // Use local obstacle avoidance to smooth the path
                    return GetNavigationDirection(directionToWaypoint, distanceToPlayer);
                }
            }
        }

        // Fallback to direct navigation if path is complete or invalid
        Vector2 fallbackDirection = (player.position - transform.position).normalized;
        return GetNavigationDirection(fallbackDirection, distanceToPlayer);
    }

    private void OnDrawGizmos()
    {
        // Draw attack range
        Gizmos.color = new Color(1, 0, 0, 0.3f);
        Gizmos.DrawWireSphere(transform.position, attackRange);

        if (player != null)
        {
            // Draw direct line to player
            Gizmos.color = new Color(1, 1, 0, 0.5f);
            Gizmos.DrawLine(transform.position, player.position);

            // Draw multi-directional raycasts for obstacle detection
            for (int i = 0; i < raycastDirections; i++)
            {
                float angle = (360f / raycastDirections) * i;
                float angleRad = angle * Mathf.Deg2Rad;
                Vector2 rayDirection = new Vector2(Mathf.Cos(angleRad), Mathf.Sin(angleRad));

                RaycastHit2D hit = Physics2D.Raycast(transform.position, rayDirection, obstacleDetectionDistance, obstacleLayer);

                if (hit.collider == null)
                {
                    // Green for clear paths
                    Gizmos.color = new Color(0, 1, 0, 0.2f);
                    Gizmos.DrawRay(transform.position, rayDirection * obstacleDetectionDistance);
                }
                else
                {
                    // Red for blocked paths
                    Gizmos.color = new Color(1, 0, 0, 0.5f);
                    Gizmos.DrawRay(transform.position, rayDirection * hit.distance);
                    Gizmos.DrawWireSphere(hit.point, 0.1f);
                }
            }

            // Draw current movement direction
            if (desiredVelocity.magnitude > 0.1f)
            {
                Gizmos.color = Color.magenta;
                Gizmos.DrawRay(transform.position, desiredVelocity.normalized * obstacleDetectionDistance);
            }
        }

        // Draw current A* path
        if (currentPath != null && currentPath.Count > 0)
        {
            Gizmos.color = Color.cyan;
            for (int i = 0; i < currentPath.Count - 1; i++)
            {
                Gizmos.DrawLine(currentPath[i], currentPath[i + 1]);
                Gizmos.DrawWireSphere(currentPath[i], 0.15f);
            }

            // Highlight current waypoint in yellow
            if (currentWaypointIndex < currentPath.Count)
            {
                Gizmos.color = Color.yellow;
                Gizmos.DrawWireSphere(currentPath[currentWaypointIndex], 0.25f);
                Gizmos.DrawLine(transform.position, currentPath[currentWaypointIndex]);
            }
        }

    }
}

