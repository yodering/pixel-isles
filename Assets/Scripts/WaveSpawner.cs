using UnityEngine;
using UnityEngine.Events;
using System.Collections;
using System.Collections.Generic;

/// <summary>
/// Advanced wave-based enemy spawning system
/// Supports multiple enemy types, configurable waves, and spawn areas
/// </summary>
public class WaveSpawner : MonoBehaviour
{
    [System.Serializable]
    public class EnemySpawnInfo
    {
        public GameObject enemyPrefab;
        public int count = 1;
        [Tooltip("Delay in seconds between spawning each of this enemy type")]
        public float spawnInterval = 0.5f;
    }

    [System.Serializable]
    public class Wave
    {
        public string waveName = "Wave";
        public List<EnemySpawnInfo> enemies = new List<EnemySpawnInfo>();
        [Tooltip("Delay in seconds before this wave starts after previous wave is cleared")]
        public float delayBeforeWave = 3f;
    }

    [Header("Wave Configuration")]
    [SerializeField] private List<Wave> waves = new List<Wave>();
    [SerializeField] private bool autoStartFirstWave = true;
    [SerializeField] private bool loopWaves = false;

    [Header("Spawn Areas")]
    [SerializeField] private BoxCollider2D[] spawnAreas;
    [SerializeField] private bool autoFindSpawnAreas = true;

    [Header("Events")]
    public UnityEvent<int> OnWaveStart; // Passes wave number
    public UnityEvent<int> OnWaveComplete; // Passes wave number
    public UnityEvent OnAllWavesComplete;
    public UnityEvent<int, int> OnEnemyCountChanged; // Passes (current, total)

    [Header("Debug")]
    [SerializeField] private bool showDebugLogs = true;

    private int currentWaveIndex = -1;
    private bool isSpawningWave = false;
    private List<GameObject> activeEnemies = new List<GameObject>();
    private int totalEnemiesInCurrentWave = 0;

    void Start()
    {
        if (autoFindSpawnAreas && (spawnAreas == null || spawnAreas.Length == 0))
        {
            FindSpawnAreas();
        }

        if (autoStartFirstWave && waves.Count > 0)
        {
            StartNextWave();
        }
    }

    void Update()
    {
        // Clean up null references (destroyed enemies)
        activeEnemies.RemoveAll(enemy => enemy == null);

        // Check if wave is complete
        if (!isSpawningWave && currentWaveIndex >= 0 && activeEnemies.Count == 0 && totalEnemiesInCurrentWave > 0)
        {
            OnWaveCompleted();
        }

        // Debug controls
        if (showDebugLogs)
        {
            if (Input.GetKeyDown(KeyCode.N))
            {
                StartNextWave();
            }
        }
    }

    /// <summary>
    /// Start the next wave in sequence
    /// </summary>
    public void StartNextWave()
    {
        if (isSpawningWave)
        {
            if (showDebugLogs) Debug.LogWarning("WaveSpawner: Cannot start next wave while currently spawning!");
            return;
        }

        currentWaveIndex++;

        // Check if all waves are complete
        if (currentWaveIndex >= waves.Count)
        {
            if (loopWaves)
            {
                currentWaveIndex = 0;
                if (showDebugLogs) Debug.Log("WaveSpawner: Looping back to first wave");
            }
            else
            {
                if (showDebugLogs) Debug.Log("WaveSpawner: All waves complete!");
                OnAllWavesComplete?.Invoke();
                return;
            }
        }

        Wave currentWave = waves[currentWaveIndex];
        StartCoroutine(SpawnWave(currentWave, currentWaveIndex));
    }

    /// <summary>
    /// Restart from the first wave
    /// </summary>
    public void RestartWaves()
    {
        StopAllCoroutines();

        // Clear all active enemies
        foreach (GameObject enemy in activeEnemies)
        {
            if (enemy != null) Destroy(enemy);
        }
        activeEnemies.Clear();

        currentWaveIndex = -1;
        isSpawningWave = false;
        totalEnemiesInCurrentWave = 0;

        StartNextWave();
    }

    /// <summary>
    /// Coroutine to spawn a complete wave
    /// </summary>
    private IEnumerator SpawnWave(Wave wave, int waveNumber)
    {
        isSpawningWave = true;

        // Wait for delay before wave
        if (waveNumber > 0 && wave.delayBeforeWave > 0)
        {
            if (showDebugLogs) Debug.Log($"WaveSpawner: Waiting {wave.delayBeforeWave}s before {wave.waveName}...");
            yield return new WaitForSeconds(wave.delayBeforeWave);
        }

        // Calculate total enemies in this wave
        totalEnemiesInCurrentWave = 0;
        foreach (EnemySpawnInfo enemyInfo in wave.enemies)
        {
            totalEnemiesInCurrentWave += enemyInfo.count;
        }

        if (showDebugLogs) Debug.Log($"WaveSpawner: Starting {wave.waveName} (Wave {waveNumber + 1}) - {totalEnemiesInCurrentWave} enemies");
        OnWaveStart?.Invoke(waveNumber + 1);

        // Spawn each enemy type in the wave
        foreach (EnemySpawnInfo enemyInfo in wave.enemies)
        {
            if (enemyInfo.enemyPrefab == null)
            {
                Debug.LogWarning($"WaveSpawner: Null enemy prefab in {wave.waveName}");
                continue;
            }

            for (int i = 0; i < enemyInfo.count; i++)
            {
                SpawnEnemy(enemyInfo.enemyPrefab);
                OnEnemyCountChanged?.Invoke(activeEnemies.Count, totalEnemiesInCurrentWave);

                // Wait between spawning each enemy
                if (i < enemyInfo.count - 1 && enemyInfo.spawnInterval > 0)
                {
                    yield return new WaitForSeconds(enemyInfo.spawnInterval);
                }
            }
        }

        isSpawningWave = false;
        if (showDebugLogs) Debug.Log($"WaveSpawner: Finished spawning {wave.waveName}");
    }

    /// <summary>
    /// Spawn a single enemy at a random spawn point
    /// </summary>
    private void SpawnEnemy(GameObject enemyPrefab)
    {
        if (spawnAreas == null || spawnAreas.Length == 0)
        {
            Debug.LogWarning("WaveSpawner: No spawn areas defined!");
            return;
        }

        // Pick random spawn area
        BoxCollider2D spawnArea = spawnAreas[Random.Range(0, spawnAreas.Length)];
        Vector3 spawnPosition = GetRandomPositionInBounds(spawnArea);

        // Spawn enemy
        GameObject enemy = Instantiate(enemyPrefab, spawnPosition, Quaternion.identity);
        enemy.tag = "Enemy";
        activeEnemies.Add(enemy);

        if (showDebugLogs) Debug.Log($"WaveSpawner: Spawned {enemyPrefab.name} at {spawnPosition}");
    }

    /// <summary>
    /// Called when wave is completed (all enemies defeated)
    /// </summary>
    private void OnWaveCompleted()
    {
        if (showDebugLogs) Debug.Log($"WaveSpawner: Wave {currentWaveIndex + 1} completed!");
        OnWaveComplete?.Invoke(currentWaveIndex + 1);

        totalEnemiesInCurrentWave = 0;

        // Auto-start next wave
        StartNextWave();
    }

    /// <summary>
    /// Find all spawn area colliders in scene
    /// </summary>
    private void FindSpawnAreas()
    {
        List<BoxCollider2D> foundAreas = new List<BoxCollider2D>();

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
        if (showDebugLogs) Debug.Log($"WaveSpawner: Found {spawnAreas.Length} spawn areas");
    }

    /// <summary>
    /// Get random position within BoxCollider2D bounds
    /// </summary>
    private Vector3 GetRandomPositionInBounds(BoxCollider2D collider)
    {
        Bounds bounds = collider.bounds;
        float randomX = Random.Range(bounds.min.x, bounds.max.x);
        float randomY = Random.Range(bounds.min.y, bounds.max.y);
        return new Vector3(randomX, randomY, 0);
    }

    // Public getters
    public int GetCurrentWaveNumber() => currentWaveIndex + 1;
    public int GetTotalWaves() => waves.Count;
    public int GetActiveEnemyCount() => activeEnemies.Count;
    public bool IsSpawningWave() => isSpawningWave;

    // Visualize spawn areas
    private void OnDrawGizmosSelected()
    {
        if (spawnAreas != null && spawnAreas.Length > 0)
        {
            Gizmos.color = Color.cyan;
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
