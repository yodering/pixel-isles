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

    [Header("Player Reference")]
    [SerializeField] private Health playerHealth;

    void Start()
    {
        // Find player if not assigned
        if (playerHealth == null)
        {
            GameObject player = GameObject.FindGameObjectWithTag("Player1");
            if (player != null)
            {
                playerHealth = player.GetComponent<Health>();
            }
        }

        // Setup instructions
        if (instructionsText != null)
        {
            instructionsText.text = "CONTROLS:\nWASD - Move\nMouse - Aim\nRight Click - Attack\nT - Spawn 1 Enemy\nY - Spawn 5 Enemies\nC - Crouch";
        }
    }

    void Update()
    {
        UpdateHealthDisplay();
        UpdateEnemyCount();
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
}
