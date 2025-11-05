using UnityEngine;
using System.Collections.Generic;

/// <summary>
/// Improved spawner that spawns enemies within designated spawn area colliders
/// Press T to spawn an enemy at a random position within spawn areas
/// </summary>
public class TestSpawner : MonoBehaviour
{
    [Header("Spawn Settings")]
    [SerializeField] private GameObject enemyPrefab;
    [SerializeField] private int maxEnemies = 5;
    
    [Header("Spawn Areas")]
    [Tooltip("Assign BoxCollider2D objects marked as triggers that define spawn areas")]
    [SerializeField] private BoxCollider2D[] spawnAreas;
    
    [Header("Auto-Find Spawn Areas")]
    [Tooltip("If true, automatically finds all GameObjects with 'spawn' in their name")]
    [SerializeField] private bool autoFindSpawnAreas = true;

    [Header("References")]
    [SerializeField] private Transform player;

    private int currentEnemyCount = 0;

    void Start()
    {
        // Auto-find spawn areas if enabled
        if (autoFindSpawnAreas && (spawnAreas == null || spawnAreas.Length == 0))
        {
            FindSpawnAreas();
        }
    }

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
    /// Automatically find all spawn area colliders in the scene
    /// </summary>
    private void FindSpawnAreas()
    {
        List<BoxCollider2D> foundAreas = new List<BoxCollider2D>();
        
        // Find all GameObjects with "spawn" in their name
        GameObject[] allObjects = GameObject.FindObjectsByType<GameObject>(FindObjectsSortMode.None);
        foreach (GameObject obj in allObjects)
        {
            if (obj.name.ToLower().Contains("spawn"))
            {
                BoxCollider2D collider = obj.GetComponent<BoxCollider2D>();
                if (collider != null && collider.isTrigger)
                {
                    foundAreas.Add(collider);
                }
            }
        }
        
        spawnAreas = foundAreas.ToArray();
        Debug.Log($"TestSpawner: Found {spawnAreas.Length} spawn areas");
    }

    /// <summary>
    /// Spawn a single enemy at a random position within spawn areas
    /// </summary>
    public void SpawnEnemy()
    {
        if (enemyPrefab == null)
        {
            Debug.LogWarning("TestSpawner: No enemy prefab assigned!");
            return;
        }

        if (currentEnemyCount >= maxEnemies)
        {
            Debug.Log("TestSpawner: Max enemies reached!");
            return;
        }

        if (spawnAreas == null || spawnAreas.Length == 0)
        {
            Debug.LogWarning("TestSpawner: No spawn areas defined! Add BoxCollider2D components to GameObjects named 'spawn1', 'spawn2', etc.");
            return;
        }

        // Pick a random spawn area
        BoxCollider2D spawnArea = spawnAreas[Random.Range(0, spawnAreas.Length)];
        
        // Get random position within the spawn area bounds
        Vector3 spawnPosition = GetRandomPositionInBounds(spawnArea);

        // Spawn the enemy
        GameObject enemy = Instantiate(enemyPrefab, spawnPosition, Quaternion.identity);
        enemy.tag = "Enemy";

        Debug.Log($"Spawned enemy {currentEnemyCount + 1}/{maxEnemies} at ({spawnPosition.x:F1}, {spawnPosition.y:F1}) via {spawnArea.gameObject.name}");
    }

    /// <summary>
    /// Get a random position within a BoxCollider2D bounds
    /// </summary>
    private Vector3 GetRandomPositionInBounds(BoxCollider2D collider)
    {
        Bounds bounds = collider.bounds;
        
        float randomX = Random.Range(bounds.min.x, bounds.max.x);
        float randomY = Random.Range(bounds.min.y, bounds.max.y);
        
        return new Vector3(randomX, randomY, 0);
    }

    // Visualize spawn areas in editor
    private void OnDrawGizmosSelected()
    {
        if (spawnAreas != null && spawnAreas.Length > 0)
        {
            Gizmos.color = Color.green;
            foreach (BoxCollider2D area in spawnAreas)
            {
                if (area != null)
                {
                    Bounds bounds = area.bounds;
                    Gizmos.DrawWireCube(bounds.center, bounds.size);
                }
            }
        }
    }
}
