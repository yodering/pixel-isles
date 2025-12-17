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
    [SerializeField] private bool usePresetWaves = true; // Use built-in 3-wave setup

    [Header("Spawn Areas")]
    [SerializeField] private BoxCollider2D[] spawnAreas;
    [SerializeField] private bool autoFindSpawnAreas = true;

    [Header("Speech Bubble Integration")]
    [SerializeField] private SpeechBubble speechBubble;
    [SerializeField] private float speechDuration = 4f;
    [Tooltip("Message shown only once when entering this scene/environment")]
    [SerializeField] private string environmentMessage = "Where am I...\nWHAT THE HELL?!";
    [SerializeField] private bool showEnvironmentMessageOnStart = true;
    [SerializeField] private string victoryMessage = "Finally...\nIt's over.";

    [Header("Scene Transition")]
    [SerializeField] private bool autoTransitionOnComplete = true;
    [SerializeField] private float transitionDelay = 3f;
    [SerializeField] private string nextSceneName = ""; // Leave empty to load next scene in build order

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
    private bool hasShownEnvironmentMessage = false;

    void Start()
    {
        if (autoFindSpawnAreas && (spawnAreas == null || spawnAreas.Length == 0))
        {
            FindSpawnAreas();
        }

        if (speechBubble == null)
        {
            speechBubble = FindAnyObjectByType<SpeechBubble>();
        }

        if (usePresetWaves)
        {
            SetupPresetWaves();
        }

        // Show environment-specific message once on scene start
        if (showEnvironmentMessageOnStart && !string.IsNullOrEmpty(environmentMessage))
        {
            ShowEnvironmentMessage();
        }

        if (autoStartFirstWave && waves.Count > 0)
        {
            StartNextWave();
        }
    }

    /// <summary>
    /// Setup the 3 preset waves: 2, 3, 5 enemies
    /// Uses prefabs from existing wave configuration
    /// </summary>
    private void SetupPresetWaves()
    {
        GameObject[] enemyPrefabs = FindEnemyPrefabs();
        if (enemyPrefabs.Length == 0)
        {
            Debug.LogWarning("WaveSpawner: No enemy prefabs found! Please assign prefabs in Inspector waves.");
            return;
        }

        waves.Clear();
        Wave wave1 = new Wave();
        wave1.waveName = "Wave_1";
        wave1.delayBeforeWave = 5f; // Longer delay for first wave to let player explore movement
        wave1.enemies.Add(new EnemySpawnInfo { enemyPrefab = enemyPrefabs[0], count = 2, spawnInterval = 0.8f });
        waves.Add(wave1);

        // Wave 2: 3 enemies (mix types if available)
        Wave wave2 = new Wave();
        wave2.waveName = "Wave_2";
        wave2.delayBeforeWave = 3f;
        if (enemyPrefabs.Length >= 2)
        {
            wave2.enemies.Add(new EnemySpawnInfo { enemyPrefab = enemyPrefabs[0], count = 2, spawnInterval = 0.6f });
            wave2.enemies.Add(new EnemySpawnInfo { enemyPrefab = enemyPrefabs[1], count = 1, spawnInterval = 0.6f });
        }
        else
        {
            wave2.enemies.Add(new EnemySpawnInfo { enemyPrefab = enemyPrefabs[0], count = 3, spawnInterval = 0.6f });
        }
        waves.Add(wave2);

        // Wave 3: 5 enemies (mix all types)
        Wave wave3 = new Wave();
        wave3.waveName = "Wave_3";
        wave3.delayBeforeWave = 3f;
        if (enemyPrefabs.Length >= 3)
        {
            wave3.enemies.Add(new EnemySpawnInfo { enemyPrefab = enemyPrefabs[0], count = 2, spawnInterval = 0.5f });
            wave3.enemies.Add(new EnemySpawnInfo { enemyPrefab = enemyPrefabs[1], count = 2, spawnInterval = 0.5f });
            wave3.enemies.Add(new EnemySpawnInfo { enemyPrefab = enemyPrefabs[2], count = 1, spawnInterval = 0.5f });
        }
        else if (enemyPrefabs.Length >= 2)
        {
            wave3.enemies.Add(new EnemySpawnInfo { enemyPrefab = enemyPrefabs[0], count = 3, spawnInterval = 0.5f });
            wave3.enemies.Add(new EnemySpawnInfo { enemyPrefab = enemyPrefabs[1], count = 2, spawnInterval = 0.5f });
        }
        else
        {
            wave3.enemies.Add(new EnemySpawnInfo { enemyPrefab = enemyPrefabs[0], count = 5, spawnInterval = 0.5f });
        }
        waves.Add(wave3);

        if (showDebugLogs) Debug.Log($"WaveSpawner: Setup {waves.Count} preset waves with {enemyPrefabs.Length} enemy types");
    }

    /// <summary>
    /// Find enemy prefabs from the current wave configuration
    /// Must be called BEFORE clearing waves!
    /// </summary>
    private GameObject[] FindEnemyPrefabs()
    {
        List<GameObject> prefabs = new List<GameObject>();
        
        if (waves != null && waves.Count > 0)
        {
            foreach (Wave wave in waves)
            {
                if (wave.enemies != null)
                {
                    foreach (EnemySpawnInfo enemyInfo in wave.enemies)
                    {
                        if (enemyInfo.enemyPrefab != null && !prefabs.Contains(enemyInfo.enemyPrefab))
                        {
                            prefabs.Add(enemyInfo.enemyPrefab);
                        }
                    }
                }
            }
        }
        if (prefabs.Count == 0)
        {
            string[] prefabNames = { "Knight", "Paladin", "Mage", "DarkLord" };
            foreach (string prefabName in prefabNames)
            {
                GameObject prefab = Resources.Load<GameObject>($"Prefabs/Enemies/{prefabName}");
                if (prefab != null && !prefabs.Contains(prefab))
                {
                    prefabs.Add(prefab);
                }
            }
        }

        return prefabs.ToArray();
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
                ShowVictoryMessage();
                OnAllWavesComplete?.Invoke();

                // Transition to next scene if enabled
                if (autoTransitionOnComplete)
                {
                    StartCoroutine(TransitionToNextScene());
                }
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

        // Set sorting layer to "Player" for all sprite renderers
        SpriteRenderer[] renderers = enemy.GetComponentsInChildren<SpriteRenderer>();
        foreach (SpriteRenderer renderer in renderers)
        {
            renderer.sortingLayerName = "Player";
        }

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

    /// <summary>
    /// Show environment-specific message once when entering the scene
    /// </summary>
    private void ShowEnvironmentMessage()
    {
        if (speechBubble == null || hasShownEnvironmentMessage) return;

        if (!string.IsNullOrEmpty(environmentMessage))
        {
            speechBubble.ShowText(environmentMessage, speechDuration);
            hasShownEnvironmentMessage = true;
            if (showDebugLogs) Debug.Log($"WaveSpawner: Showing environment message: {environmentMessage}");
        }
    }

    /// <summary>
    /// Show victory message when all waves complete
    /// </summary>
    private void ShowVictoryMessage()
    {
        if (speechBubble != null && !string.IsNullOrEmpty(victoryMessage))
        {
            speechBubble.ShowText(victoryMessage, speechDuration * 1.5f);
        }
    }

    /// <summary>
    /// Transition to next scene after delay
    /// </summary>
    private IEnumerator TransitionToNextScene()
    {
        if (showDebugLogs) Debug.Log($"WaveSpawner: Transitioning to next scene in {transitionDelay} seconds...");

        yield return new WaitForSeconds(transitionDelay);

        // Check if player is still alive before transitioning
        Health playerHealth = FindPlayerHealth();
        if (playerHealth != null && playerHealth.IsDead())
        {
            if (showDebugLogs) Debug.Log("WaveSpawner: Player is dead - cancelling scene transition");
            yield break;
        }

        SceneLoader sceneLoader = FindAnyObjectByType<SceneLoader>();

        // Always transition even if SceneLoader is missing (e.g. when play starts from a non-loading scene).
        if (!string.IsNullOrEmpty(nextSceneName))
        {
            if (showDebugLogs) Debug.Log($"WaveSpawner: Loading scene '{nextSceneName}'");
            if (sceneLoader != null) sceneLoader.LoadScene(nextSceneName);
            else UnityEngine.SceneManagement.SceneManager.LoadScene(nextSceneName);
            yield break;
        }

        // Load next scene in build order
        int nextSceneIndex = UnityEngine.SceneManagement.SceneManager.GetActiveScene().buildIndex + 1;
        if (showDebugLogs) Debug.Log($"WaveSpawner: Loading scene index {nextSceneIndex}");

        // Guard: avoid silent failure if the build index is out of range.
        if (nextSceneIndex < 0 || nextSceneIndex >= UnityEngine.SceneManagement.SceneManager.sceneCountInBuildSettings)
        {
            Debug.LogWarning($"WaveSpawner: Next scene index {nextSceneIndex} is out of range (Scenes In Build: {UnityEngine.SceneManagement.SceneManager.sceneCountInBuildSettings}).");
            yield break;
        }

        if (sceneLoader != null) sceneLoader.LoadSceneByIndex(nextSceneIndex);
        else UnityEngine.SceneManagement.SceneManager.LoadScene(nextSceneIndex);
    }

    /// <summary>
    /// Find the player's Health component
    /// </summary>
    private Health FindPlayerHealth()
    {
        Health[] allHealthComponents = FindObjectsByType<Health>(FindObjectsSortMode.None);
        foreach (Health health in allHealthComponents)
        {
            if (health.IsPlayer())
            {
                return health;
            }
        }
        return null;
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
