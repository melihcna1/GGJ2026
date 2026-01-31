using UnityEngine;

public class EnemyHealth : MonoBehaviour
{
    public float maxHealth = 100f;

    public float currentHealth = 20;

    public HealthBar healthBar;

    private bool _hasScored;

    void Start()
    {
        currentHealth = maxHealth;
        _hasScored = false;
        healthBar.SetMaxHealth(maxHealth);

    }

    void DamageTick()
    {
        TakeDamage(50f);
    }

    public void TakeDamage(float damage)
    {
        currentHealth -= damage;
        healthBar.SetHealth(currentHealth);

        if (currentHealth <= 0)
        {
            Die();
        }
    }

    void Die()
    {
        CancelInvoke();   

        if (!_hasScored)
        {
            _hasScored = true;
            ComboManager.EnsureInstance().RegisterKill();
        }

        Destroy(gameObject);
    }
}