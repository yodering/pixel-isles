using UnityEngine;
using TMPro;

/// <summary>
/// Simple UI manager to display player health and enemy count
/// </summary>
public class SimpleUI : MonoBehaviour
{
    [Header("UI References")]
    [SerializeField] private TextMeshProUGUI healthText;
    [SerializeField] private TextMeshProUGUI enemyCountText;
    [SerializeField] private TextMeshProUGUI instructionsText;
    [SerializeField] private TextMeshProUGUI abilitiesText;

    [Header("Player Reference")]
    [SerializeField] private Health playerHealth;
    [SerializeField] private PlayerController playerController;

    void Start()
    {
        // Find player if not assigned
        if (playerHealth == null || playerController == null)
        {
            GameObject player = GameObject.FindGameObjectWithTag("Player1");
            if (player != null)
            {
                if (playerHealth == null)
                    playerHealth = player.GetComponent<Health>();
                if (playerController == null)
                    playerController = player.GetComponent<PlayerController>();
            }
        }

        // Setup instructions
        if (instructionsText != null)
        {
            instructionsText.text = "CONTROLS:\nWASD - Move | Mouse - Aim\n[E] Sword | [Q] Projectile | [F] AOE\nC - Crouch";
        }
    }

    void Update()
    {
        UpdateHealthDisplay();
        UpdateEnemyCount();
        UpdateAbilitiesDisplay();
    }

    /// <summary>
    /// Update the health text display
    /// </summary>
    private void UpdateHealthDisplay()
    {
        if (healthText == null || playerHealth == null) return;

        float currentHP = playerHealth.GetCurrentHealth();
        float maxHP = playerHealth.GetMaxHealth();
        float percentage = playerHealth.GetHealthPercentage() * 100f;

        healthText.text = $"HP: {currentHP:F0}/{maxHP:F0} ({percentage:F0}%)";

        // Color code based on health
        if (percentage > 60f)
            healthText.color = Color.green;
        else if (percentage > 30f)
            healthText.color = Color.yellow;
        else
            healthText.color = Color.red;
    }

    /// <summary>
    /// Update the enemy count display
    /// </summary>
    private void UpdateEnemyCount()
    {
        if (enemyCountText == null) return;

        int enemyCount = GameObject.FindGameObjectsWithTag("Enemy").Length;
        enemyCountText.text = $"Enemies: {enemyCount}";
    }

    /// <summary>
    /// Update the abilities status display
    /// </summary>
    private void UpdateAbilitiesDisplay()
    {
        if (abilitiesText == null || playerController == null) return;

        // Only show if player is ranged character
        if (!playerController.isRanged)
        {
            abilitiesText.text = "";
            return;
        }

        int hitCount = playerController.GetHitCount();
        int killCount = playerController.GetKillCount();
        bool qUnlocked = playerController.IsProjectileUnlocked();
        bool fUnlocked = playerController.IsAoEUnlocked();

        // Build ability status text
        string abilities = "=== ABILITIES ===\n";
        abilities += "[E] Sword Attack (Always Available)\n";

        // Q - Projectile ability
        if (qUnlocked)
        {
            abilities += "<color=#00FF00>[Q] Projectile UNLOCKED</color>\n";
        }
        else
        {
            abilities += $"<color=#888888>[Q] Projectile LOCKED ({hitCount}/2 hits)</color>\n";
        }

        // F - AOE ability
        if (fUnlocked)
        {
            abilities += "<color=#00FF00>[F] AOE Attack UNLOCKED</color>";
        }
        else
        {
            abilities += $"<color=#888888>[F] AOE Attack LOCKED ({killCount}/5 kills)</color>";
        }

        abilitiesText.text = abilities;
    }
}
