using UnityEngine;
using UnityEngine.UI;

public class PlayerHP : MonoBehaviour
{
    [Header("Health")]
    public float maxHealth = 100f;

    private float currentHealth;

    [Header("UI")]
    [SerializeField] private Slider healthBar;

    private void Start()
    {
        currentHealth = maxHealth;

        // Set health bar to full health
        if (healthBar != null)
        {
            healthBar.maxValue = maxHealth;
            healthBar.value = currentHealth;
        }
    }

    public void TakeDamage(float damage)
    {
        currentHealth -= damage;

        // Prevent health from going below zero
        currentHealth = Mathf.Clamp(currentHealth, 0f, maxHealth);

        Debug.Log("Player Health: " + currentHealth);

        // Update UI
        if (healthBar != null)
        {
            healthBar.value = currentHealth;
        }

        if (currentHealth <= 0)
        {
            Die();
        }
    }

    private void Die()
    {
        Debug.Log("PLAYER DIED!");
    }

    public float GetCurrentHealth()
    {
        return currentHealth;
    }
}
