using UnityEngine;

/// <summary>
/// Helper script to document and verify collision setup
/// it should be attached to a GameObject(s) to see collision debug info in console
/// </summary>
public class CollisionSetupHelper : MonoBehaviour
{
    [Header("Debug Options")]
    [SerializeField] private bool logCollisionInfo = true;
    [SerializeField] private bool checkSetupOnStart = true;

    void Start()
    {
        if (checkSetupOnStart)
        {
            CheckCollisionSetup();
        }
    }

    /// <summary>
    /// Verify that collision layers are set up correctly
    /// </summary>
    public void CheckCollisionSetup()
    {
        Debug.Log("Collision Setup Check");

        // Check if layers exist
        int enemyLayer = LayerMask.NameToLayer("Enemy");
        int playerLayer = LayerMask.NameToLayer("Player1");
        int projectileLayer = LayerMask.NameToLayer("Projectile");

        if (enemyLayer == -1)
            Debug.LogWarning("Enemy layer not found! Create layer 7: 'Enemy'");
        else
            Debug.Log($"Enemy layer found: {enemyLayer}");

        if (playerLayer == -1)
            Debug.LogWarning("Player1 layer not found! Create layer 6: 'Player1'");
        else
            Debug.Log($"Player1 layer found: {playerLayer}");

        if (projectileLayer == -1)
            Debug.LogWarning("Projectile layer not found! Create layer 8: 'Projectile'");
        else
            Debug.Log($"Projectile layer found: {projectileLayer}");

        // Check for enemies in scene
        GameObject[] enemies = GameObject.FindGameObjectsWithTag("Enemy");
        Debug.Log($"Found {enemies.Length} enemies in scene");

        foreach (GameObject enemy in enemies)
        {
            if (enemy.layer != enemyLayer)
            {
                Debug.LogWarning($"{enemy.name} is not on Enemy layer! Current layer: {LayerMask.LayerToName(enemy.layer)}");
            }

            Rigidbody2D rb = enemy.GetComponent<Rigidbody2D>();
            if (rb == null)
            {
                Debug.LogWarning($"{enemy.name} missing Rigidbody2D!");
            }

            Collider2D[] colliders = enemy.GetComponents<Collider2D>();
            bool hasBodyCollider = false;
            foreach (Collider2D col in colliders)
            {
                if (!col.isTrigger)
                {
                    hasBodyCollider = true;
                    break;
                }
            }

            if (!hasBodyCollider)
            {
                Debug.LogWarning($"{enemy.name} has no solid collider (all are triggers)");
            }
        }

        // Check for player
        GameObject player = GameObject.FindGameObjectWithTag("Player1");
        if (player == null)
        {
            Debug.LogWarning("No player found with 'Player1' tag");
        }
        else
        {
            Debug.Log($"Player found: {player.name}");
            if (player.layer != playerLayer && playerLayer != -1)
            {
                Debug.LogWarning($"Player is not on Player1 layer! Current layer: {LayerMask.LayerToName(player.layer)}");
            }
        }

        Debug.Log("End Collision Setup Check");
    }

    /// <summary>
    /// Test collision between two layers
    /// </summary>
    public static bool CanLayersCollide(int layer1, int layer2)
    {
        return !Physics2D.GetIgnoreLayerCollision(layer1, layer2);
    }

    /// <summary>
    /// Log collision matrix for debugging
    /// </summary>
    [ContextMenu("Log Collision Matrix")]
    public void LogCollisionMatrix()
    {
        Debug.Log("Physics 2D Collision Matrix");
        
        string[] layerNames = { "Default", "Player1", "Enemy", "Projectile" };
        int[] layerIndices = { 0, 6, 7, 8 };

        for (int i = 0; i < layerNames.Length; i++)
        {
            for (int j = 0; j < layerNames.Length; j++)
            {
                bool canCollide = CanLayersCollide(layerIndices[i], layerIndices[j]);
                string status;
                if (canCollide)
                {
                    status = "yes";
                }
                else
                {
                    status = "no";
                }
                Debug.Log($"{status} {layerNames[i]} <-> {layerNames[j]}: {(canCollide ? "COLLIDE" : "IGNORE")}");
            }
        }

        Debug.Log("End Collision Matrix");
    }

    void OnCollisionEnter2D(Collision2D collision)
    {
        if (logCollisionInfo)
        {
            Debug.Log($"Collision: {gameObject.name} (Layer {LayerMask.LayerToName(gameObject.layer)}) hit {collision.gameObject.name} (Layer {LayerMask.LayerToName(collision.gameObject.layer)})");
        }
    }

    void OnTriggerEnter2D(Collider2D collision)
    {
        if (logCollisionInfo)
        {
            Debug.Log($"Trigger: {gameObject.name} entered {collision.gameObject.name}");
        }
    }
}

