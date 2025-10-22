using UnityEngine;

/// <summary>
/// Simple test spawner to manually spawn enemies for testing
/// Press T to spawn an enemy at a random position
/// </summary>
public class TestSpawner : MonoBehaviour
{
    [Header("Spawn Settings")]
    [SerializeField] private GameObject enemyPrefab;
    [SerializeField] private float spawnRadius = 10f;
    [SerializeField] private int maxEnemies = 10;

    [Header("References")]
    [SerializeField] private Transform player;

    private int currentEnemyCount = 0;

    void Update()
    {
        // Press T to spawn an enemy
        if (Input.GetKeyDown(KeyCode.T))
        {
            SpawnEnemy();
        }

        // Press Y to spawn 5 enemies
        if (Input.GetKeyDown(KeyCode.Y))
        {
            for (int i = 0; i < 5; i++)
            {
                SpawnEnemy();
            }
        }

        // Update enemy count
        currentEnemyCount = GameObject.FindGameObjectsWithTag("Enemy").Length;
    }

    /// <summary>
    /// Spawn a single enemy at a random position around the player
    /// </summary>
    public void SpawnEnemy()
    {
        if (enemyPrefab == null)
        {
            Debug.LogWarning("TestSpawner: No enemy prefab assigned!");
            return;
        }

        if (player == null)
        {
            GameObject playerObj = GameObject.FindGameObjectWithTag("Player1");
            if (playerObj != null)
            {
                player = playerObj.transform;
            }
            else
            {
                Debug.LogWarning("TestSpawner: No player found!");
                return;
            }
        }

        if (currentEnemyCount >= maxEnemies)
        {
            Debug.Log("TestSpawner: Max enemies reached!");
            return;
        }

        // Random position around player
        Vector2 randomDirection = Random.insideUnitCircle.normalized;
        Vector3 spawnPosition = player.position + new Vector3(randomDirection.x, randomDirection.y, 0) * spawnRadius;

        // Spawn the enemy
        GameObject enemy = Instantiate(enemyPrefab, spawnPosition, Quaternion.identity);
        enemy.tag = "Enemy";

        Debug.Log($"Spawned enemy at {spawnPosition}. Total enemies: {currentEnemyCount + 1}");
    }

    // Visualize spawn radius in editor
    private void OnDrawGizmosSelected()
    {
        if (player == null)
        {
            GameObject playerObj = GameObject.FindGameObjectWithTag("Player1");
            if (playerObj != null)
            {
                player = playerObj.transform;
            }
        }

        if (player != null)
        {
            Gizmos.color = Color.green;
            Gizmos.DrawWireSphere(player.position, spawnRadius);
        }
    }
}
