using System.Collections;
using UnityEngine;
using UnityEngine.UI;

public class PlayerHP : MonoBehaviour
{
    [Header("Health")]
    public float maxHealth = 100f;

    private float currentHealth;

    [Header("UI")]
    [SerializeField] private Slider healthBar;

    [Header("Stun Particles")]
    [SerializeField] private ParticleSystem stunParticles;
    [SerializeField] private float stunDuration = 5f;

    private bool isStunned = false;

    [Header("Knockback")]
    [SerializeField] private float knockbackForce = 5f;

    private Rigidbody rb;
    private Waiter_Controls waiter;


    private void Start()
    {
        currentHealth = maxHealth;

        rb = GetComponent<Rigidbody>();

        waiter = GetComponent<Waiter_Controls>();

        if (healthBar != null)
        {
            healthBar.maxValue = maxHealth;
            healthBar.value = currentHealth;
        }

        // Make sure particles are off at the beginning
        if (stunParticles != null)
        {
            stunParticles.Stop();
        }
    }


    public void TakeDamage(float damage)
    {
        // Don't take damage while stunned
        if (isStunned)
            return;

        currentHealth -= damage;

        currentHealth = Mathf.Clamp(
            currentHealth,
            0f,
            maxHealth
        );

        Debug.Log("Player Health: " + currentHealth);

        if (healthBar != null)
        {
            healthBar.value = currentHealth;
        }

        if (currentHealth <= 0f)
        {
            Die();
        }
    }


    public void Knockback(Vector3 direction)
    {
        if (rb == null)
            return;

        // Only knock the player horizontally
        direction.y = 0f;

        if (direction == Vector3.zero)
            return;

        direction.Normalize();

        rb.AddForce(
            direction * knockbackForce,
            ForceMode.Impulse
        );
    }


    private void Die()
    {
        if (isStunned)
            return;

        Debug.Log("PLAYER STUNNED!");

        isStunned = true;

        // Disable player movement
        DisablePlayerMovement();

        // Play stun particles
        if (stunParticles != null)
        {
            stunParticles.Play();
        }

        // Stop player physics movement
        if (rb != null)
        {
            rb.linearVelocity = Vector3.zero;
        }

        // Start stun timer
        StartCoroutine(StunRoutine());
    }


    private IEnumerator StunRoutine()
    {
        Debug.Log(
            "Player is stunned for " +
            stunDuration +
            " seconds."
        );

        yield return new WaitForSeconds(stunDuration);

        RecoverFromStun();
    }


    private void RecoverFromStun()
    {
        Debug.Log("PLAYER RECOVERED!");

        isStunned = false;

        // Restore health
        currentHealth = maxHealth;

        if (healthBar != null)
        {
            healthBar.value = currentHealth;
        }

        // Stop stun particles
        if (stunParticles != null)
        {
            stunParticles.Stop();
        }

        // Enable player movement
        EnablePlayerMovement();
    }


    private void DisablePlayerMovement()
    {
        if (waiter != null)
        {
            waiter.enabled = false;
        }
    }


    private void EnablePlayerMovement()
    {
        if (waiter != null)
        {
            waiter.enabled = true;
        }
    }


    public float GetCurrentHealth()
    {
        return currentHealth;
    }


    public bool IsStunned()
    {
        return isStunned;
    }
}