using UnityEngine;
using UnityEditor;
using UnityEditor.SceneManagement;

/// <summary>
/// Editor utility to automatically setup enemy characters in the scene
/// This will configure Knight, Archer, Wizard, Paladin, Mage, and CamoArcher as enemies
/// </summary>
public class SetupEnemiesInScene : EditorWindow
{
    [MenuItem("Tools/Setup Enemies in Scene")]
    public static void ShowWindow()
    {
        GetWindow<SetupEnemiesInScene>("Setup Enemies");
    }

    void OnGUI()
    {
        GUILayout.Label("Setup Enemies Automatically", EditorStyles.boldLabel);
        GUILayout.Space(10);

        GUILayout.Label("This will configure these characters as enemies:");
        GUILayout.Label("• Knight");
        GUILayout.Label("• Archer");
        GUILayout.Label("• Wizard");
        GUILayout.Label("• Paladin");
        GUILayout.Label("• Mage");
        GUILayout.Label("• CamoArcher");
        GUILayout.Space(10);

        GUILayout.Label("Each will get:");
        GUILayout.Label("• Tag: Enemy");
        GUILayout.Label("• Layer: Enemy");
        GUILayout.Label("• Health component (50 HP)");
        GUILayout.Label("• EnemyController component");
        GUILayout.Label("• Rigidbody2D (if missing)");
        GUILayout.Label("• Collider2D (if missing)");
        GUILayout.Space(10);

        if (GUILayout.Button("Setup Enemies Now", GUILayout.Height(40)))
        {
            SetupEnemies();
        }

        GUILayout.Space(10);

        if (GUILayout.Button("Setup DeathKnight as Player", GUILayout.Height(40)))
        {
            SetupDeathKnightAsPlayer();
        }

        GUILayout.Space(10);

        if (GUILayout.Button("Setup Both (Recommended)", GUILayout.Height(40)))
        {
            SetupDeathKnightAsPlayer();
            SetupEnemies();
        }
    }

    static void SetupEnemies()
    {
        string[] enemyNames = { "Knight", "Archer", "Wizard", "Paladin", "Mage", "CamoArcher" };
        int successCount = 0;

        foreach (string enemyName in enemyNames)
        {
            // Find ALL GameObjects with this name
            GameObject[] allObjects = GameObject.FindObjectsByType<GameObject>(FindObjectsSortMode.None);
            bool foundOne = false;

            foreach (GameObject obj in allObjects)
            {
                if (obj.name == enemyName)
                {
                    // Check if this is the main GameObject (has PlayerController or AnimationController)
                    if (obj.GetComponent<AnimationController>() != null ||
                        obj.GetComponent<PlayerController>() != null ||
                        obj.GetComponent<Animator>() != null)
                    {
                        ConfigureAsEnemy(obj);
                        successCount++;
                        foundOne = true;
                        Debug.Log($"✓ Configured {enemyName} as enemy");
                        break; // Only configure the first valid one
                    }
                }
            }

            if (!foundOne)
            {
                Debug.LogWarning($"✗ Could not find valid {enemyName} with components in scene");
            }
        }

        // Mark scene as dirty so it can be saved
        EditorSceneManager.MarkSceneDirty(EditorSceneManager.GetActiveScene());

        EditorUtility.DisplayDialog("Setup Complete",
            $"Successfully configured {successCount} enemies!\n\nDon't forget to:\n1. Save your scene (Ctrl+S)\n2. Create 'Enemy' tag if not exists\n3. Create 'Enemy' layer if not exists",
            "OK");
    }

    static void ConfigureAsEnemy(GameObject obj)
    {
        // Set tag (will show warning if tag doesn't exist)
        try
        {
            obj.tag = "Enemy";
        }
        catch
        {
            Debug.LogWarning($"'Enemy' tag doesn't exist! Create it in Tags & Layers settings.");
        }

        // Set layer (will use layer 7 for Enemy, create it if it doesn't exist)
        obj.layer = LayerMask.NameToLayer("Enemy");
        if (obj.layer == -1)
        {
            obj.layer = 7; // Default to layer 7
            Debug.LogWarning("'Enemy' layer doesn't exist! Please create it in Tags & Layers settings.");
        }

        // Add Rigidbody2D if missing
        Rigidbody2D rb = obj.GetComponent<Rigidbody2D>();
        if (rb == null)
        {
            rb = obj.AddComponent<Rigidbody2D>();
        }
        rb.gravityScale = 0f;
        rb.linearDamping = 5f;
        rb.constraints = RigidbodyConstraints2D.FreezeRotation;

        // Add Collider if missing (CapsuleCollider2D)
        CapsuleCollider2D collider = obj.GetComponent<CapsuleCollider2D>();
        if (collider == null)
        {
            collider = obj.AddComponent<CapsuleCollider2D>();
            collider.size = new Vector2(0.5f, 1.0f);
        }

        // Add Health component if missing
        Health health = obj.GetComponent<Health>();
        if (health == null)
        {
            health = obj.AddComponent<Health>();
        }
        // Use SerializedObject to set private fields
        SerializedObject so = new SerializedObject(health);
        so.FindProperty("maxHealth").floatValue = 50f;
        so.FindProperty("isPlayer").boolValue = false;
        so.ApplyModifiedProperties();

        // Add EnemyController if missing
        EnemyController enemyController = obj.GetComponent<EnemyController>();
        if (enemyController == null)
        {
            enemyController = obj.AddComponent<EnemyController>();
        }
        // Configure EnemyController settings
        SerializedObject ecSo = new SerializedObject(enemyController);
        ecSo.FindProperty("moveSpeed").floatValue = 1.5f;
        ecSo.FindProperty("stoppingDistance").floatValue = 1.5f;
        ecSo.FindProperty("attackRange").floatValue = 1.5f;
        ecSo.FindProperty("attackCooldown").floatValue = 1.5f;
        ecSo.FindProperty("attackDamage").floatValue = 10f;
        ecSo.ApplyModifiedProperties();

        // AnimationController should already be there, just verify
        AnimationController animController = obj.GetComponent<AnimationController>();
        if (animController == null)
        {
            Debug.LogWarning($"{obj.name} is missing AnimationController!");
        }
    }

    static void SetupDeathKnightAsPlayer()
    {
        // Find ALL GameObjects named DeathKnight
        GameObject[] allObjects = GameObject.FindObjectsOfType<GameObject>();
        GameObject deathKnight = null;

        foreach (GameObject obj in allObjects)
        {
            if (obj.name == "DeathKnight")
            {
                // Check if this is the main GameObject (has components)
                if (obj.GetComponent<AnimationController>() != null ||
                    obj.GetComponent<PlayerController>() != null ||
                    obj.GetComponent<Animator>() != null)
                {
                    deathKnight = obj;
                    break;
                }
            }
        }

        if (deathKnight != null)
        {
            ConfigureAsPlayer(deathKnight);
            Debug.Log("✓ Configured DeathKnight as player");

            EditorSceneManager.MarkSceneDirty(EditorSceneManager.GetActiveScene());

            EditorUtility.DisplayDialog("Setup Complete",
                "DeathKnight configured as player!\n\nDon't forget to:\n1. Save your scene (Ctrl+S)\n2. Create 'Player1' tag if not exists\n3. Create 'Player1' layer if not exists\n4. Enable DeathKnight's SpriteRenderer\n5. Disable other characters' SpriteRenderers",
                "OK");
        }
        else
        {
            EditorUtility.DisplayDialog("Error", "Could not find DeathKnight with components in scene!", "OK");
        }
    }

    static void ConfigureAsPlayer(GameObject obj)
    {
        // Set tag
        try
        {
            obj.tag = "Player1";
        }
        catch
        {
            Debug.LogWarning($"'Player1' tag doesn't exist! Create it in Tags & Layers settings.");
        }

        // Set layer
        obj.layer = LayerMask.NameToLayer("Player1");
        if (obj.layer == -1)
        {
            obj.layer = 6; // Default to layer 6
            Debug.LogWarning("'Player1' layer doesn't exist! Please create it in Tags & Layers settings.");
        }

        // Add Rigidbody2D if missing
        Rigidbody2D rb = obj.GetComponent<Rigidbody2D>();
        if (rb == null)
        {
            rb = obj.AddComponent<Rigidbody2D>();
        }
        rb.gravityScale = 0f;
        rb.linearDamping = 5f;
        rb.constraints = RigidbodyConstraints2D.FreezeRotation;

        // Add Collider if missing
        CapsuleCollider2D collider = obj.GetComponent<CapsuleCollider2D>();
        if (collider == null)
        {
            collider = obj.AddComponent<CapsuleCollider2D>();
            collider.size = new Vector2(0.5f, 1.0f);
        }

        // Add Health component if missing
        Health health = obj.GetComponent<Health>();
        if (health == null)
        {
            health = obj.AddComponent<Health>();
        }
        // Use SerializedObject to set private fields
        SerializedObject so = new SerializedObject(health);
        so.FindProperty("maxHealth").floatValue = 100f;
        so.FindProperty("isPlayer").boolValue = true;
        so.ApplyModifiedProperties();

        // PlayerController and AnimationController should already be there
        PlayerController playerController = obj.GetComponent<PlayerController>();
        if (playerController == null)
        {
            Debug.LogWarning($"{obj.name} is missing PlayerController!");
        }

        AnimationController animController = obj.GetComponent<AnimationController>();
        if (animController == null)
        {
            Debug.LogWarning($"{obj.name} is missing AnimationController!");
        }

        // Enable SpriteRenderer
        SpriteRenderer spriteRenderer = obj.GetComponent<SpriteRenderer>();
        if (spriteRenderer != null)
        {
            spriteRenderer.enabled = true;
            Debug.Log("✓ Enabled DeathKnight SpriteRenderer");
        }
    }
}
