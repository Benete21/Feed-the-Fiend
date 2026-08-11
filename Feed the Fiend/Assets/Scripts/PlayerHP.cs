using UnityEngine;

public class PlayerHP : MonoBehaviour
{

    public float maxHealth = 100f;

    private float currentHealth;

    void Start()
    {
        currentHealth = maxHealth;
    }

    public void TakeDamage(float damage)
    {
        currentHealth -= damage;

        Debug.Log("Player Health: " + currentHealth);

        if (currentHealth <= 0)
        {
            Die();
        }
    }

    void Die()
    {
        Debug.Log("PLAYER DIED!");

        // Add your death logic here
        // Example:
        // Destroy(gameObject);
    }
}

