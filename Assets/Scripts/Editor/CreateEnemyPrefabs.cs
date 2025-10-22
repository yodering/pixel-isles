using UnityEngine;
using UnityEditor;
using System.IO;

/// <summary>
/// Creates enemy prefabs from the existing character GameObjects in the scene
/// This creates NEW prefabs specifically for spawning as enemies
/// </summary>
public class CreateEnemyPrefabs : EditorWindow
{
    [MenuItem("Tools/Create Enemy Prefabs from Scene")]
    public static void ShowWindow()
    {
        GetWindow<CreateEnemyPrefabs>("Create Enemy Prefabs");
    }

    void OnGUI()
    {
        GUILayout.Label("Create Enemy Prefabs", EditorStyles.boldLabel);
        GUILayout.Space(10);

        GUILayout.Label("This will create NEW enemy prefabs:");
        GUILayout.Label("• Knight_Enemy.prefab");
        GUILayout.Label("• Archer_Enemy.prefab");
        GUILayout.Label("• Wizard_Enemy.prefab");
        GUILayout.Label("• Paladin_Enemy.prefab");
        GUILayout.Label("• Mage_Enemy.prefab");
        GUILayout.Label("• CamoArcher_Enemy.prefab");
        GUILayout.Space(10);

        GUILayout.Label("These will be saved to: Assets/Prefabs/Enemies/");
        GUILayout.Space(10);

        GUILayout.Label("Each enemy prefab will have:");
        GUILayout.Label("• Tag: Enemy");
        GUILayout.Label("• Layer: Enemy");
        GUILayout.Label("• Health component (50 HP)");
        GUILayout.Label("• EnemyController (NOT PlayerController)");
        GUILayout.Label("• AnimationController");
        GUILayout.Label("• Rigidbody2D & Collider2D");
        GUILayout.Space(10);

        EditorGUILayout.HelpBox("Original prefabs will NOT be modified!", MessageType.Info);
        GUILayout.Space(10);

        if (GUILayout.Button("Create Enemy Prefabs", GUILayout.Height(40)))
        {
            CreateEnemyPrefabsFromScene();
        }
    }

    static void CreateEnemyPrefabsFromScene()
    {
        // Create Enemies folder if it doesn't exist
        string folderPath = "Assets/Prefabs/Enemies";
        if (!AssetDatabase.IsValidFolder(folderPath))
        {
            AssetDatabase.CreateFolder("Assets/Prefabs", "Enemies");
        }

        string[] enemyNames = { "Knight", "Archer", "Wizard", "Paladin", "Mage", "CamoArcher" };
        int successCount = 0;

        foreach (string enemyName in enemyNames)
        {
            // Find the GameObject in the scene with components
            GameObject[] allObjects = GameObject.FindObjectsOfType<GameObject>();
            GameObject sourceObj = null;

            foreach (GameObject obj in allObjects)
            {
                if (obj.name == enemyName &&
                    (obj.GetComponent<AnimationController>() != null ||
                     obj.GetComponent<Animator>() != null))
                {
                    sourceObj = obj;
                    break;
                }
            }

            if (sourceObj == null)
            {
                Debug.LogWarning($"Could not find {enemyName} in scene!");
                continue;
            }

            // Create a duplicate GameObject for the prefab
            GameObject enemyPrefabObj = GameObject.Instantiate(sourceObj);
            enemyPrefabObj.name = $"{enemyName}_Enemy";

            // Remove PlayerController if it exists
            PlayerController playerController = enemyPrefabObj.GetComponent<PlayerController>();
            if (playerController != null)
            {
                DestroyImmediate(playerController);
                Debug.Log($"Removed PlayerController from {enemyName}");
            }

            // Configure as enemy
            ConfigureAsEnemy(enemyPrefabObj);

            // Save as prefab
            string prefabPath = $"{folderPath}/{enemyName}_Enemy.prefab";
            PrefabUtility.SaveAsPrefabAsset(enemyPrefabObj, prefabPath);

            // Clean up the temporary GameObject
            DestroyImmediate(enemyPrefabObj);

            successCount++;
            Debug.Log($"✓ Created {prefabPath}");
        }

        AssetDatabase.Refresh();

        EditorUtility.DisplayDialog("Enemy Prefabs Created",
            $"Successfully created {successCount} enemy prefabs in:\n{folderPath}\n\nYou can now use these for spawning!\n\nNext: Assign one to the TestSpawner's 'Enemy Prefab' field.",
            "OK");
    }

    static void ConfigureAsEnemy(GameObject obj)
    {
        // Set tag
        try
        {
            obj.tag = "Enemy";
        }
        catch
        {
            Debug.LogWarning($"'Enemy' tag doesn't exist! Create it in Tags & Layers settings.");
        }

        // Set layer
        obj.layer = LayerMask.NameToLayer("Enemy");
        if (obj.layer == -1)
        {
            obj.layer = 7; // Default to layer 7
        }

        // Add/Configure Rigidbody2D
        Rigidbody2D rb = obj.GetComponent<Rigidbody2D>();
        if (rb == null)
        {
            rb = obj.AddComponent<Rigidbody2D>();
        }
        rb.gravityScale = 0f;
        rb.linearDamping = 5f;
        rb.constraints = RigidbodyConstraints2D.FreezeRotation;

        // Add/Configure Collider
        CapsuleCollider2D collider = obj.GetComponent<CapsuleCollider2D>();
        if (collider == null)
        {
            collider = obj.AddComponent<CapsuleCollider2D>();
            collider.size = new Vector2(0.5f, 1.0f);
        }

        // Add/Configure Health
        Health health = obj.GetComponent<Health>();
        if (health == null)
        {
            health = obj.AddComponent<Health>();
        }
        SerializedObject so = new SerializedObject(health);
        so.FindProperty("maxHealth").floatValue = 50f;
        so.FindProperty("isPlayer").boolValue = false;
        so.ApplyModifiedProperties();

        // Add/Configure EnemyController (NOT PlayerController!)
        EnemyController enemyController = obj.GetComponent<EnemyController>();
        if (enemyController == null)
        {
            enemyController = obj.AddComponent<EnemyController>();
        }
        SerializedObject ecSo = new SerializedObject(enemyController);
        ecSo.FindProperty("moveSpeed").floatValue = 1.5f;
        ecSo.FindProperty("stoppingDistance").floatValue = 1.5f;
        ecSo.FindProperty("attackRange").floatValue = 1.5f;
        ecSo.FindProperty("attackCooldown").floatValue = 1.5f;
        ecSo.FindProperty("attackDamage").floatValue = 10f;
        ecSo.ApplyModifiedProperties();

        // Enable SpriteRenderer
        SpriteRenderer spriteRenderer = obj.GetComponent<SpriteRenderer>();
        if (spriteRenderer != null)
        {
            spriteRenderer.enabled = true;
        }
    }
}
