using UnityEngine;

public class System31Health : MonoBehaviour
{
    public float maxHealth = 100f;
    float currentHealth;

    public HealthBar healthBar;

    public float CurrentHealth => currentHealth;

    void Start()
    {
        currentHealth = maxHealth;
        if (healthBar != null)
        {
            healthBar.SetMaxHealth(maxHealth);
        }
    }

    public void TakeDamage(float damage)
    {
        if (damage <= 0f || currentHealth <= 0f)
            return;

        currentHealth = Mathf.Max(0f, currentHealth - damage);

        if (healthBar != null)
        {
            healthBar.SetHealth(currentHealth);
        }
        Debug.Log("System31 Health: " + currentHealth);

        if (currentHealth <= 0)
        {
            Die();
        }
    }

    public void Heal(float amount)
    {
        if (amount <= 0f || currentHealth <= 0f)
            return;

        currentHealth = Mathf.Min(maxHealth, currentHealth + amount);

        if (healthBar != null)
        {
            healthBar.SetHealth(currentHealth);
        }
    }

    void Die()
    {
        Debug.Log("System31 destroyed!");
    }
}