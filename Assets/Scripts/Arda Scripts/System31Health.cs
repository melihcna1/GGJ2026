using MoreMountains.Feedbacks;
using UnityEngine;

public class System31Health : MonoBehaviour
{
    [SerializeField] private float maxHealth = 100f;
    private float currentHealth;

    public HealthBar healthBar;
    public GameObject gameOverCanvas; // ← ONLY ADDITION
    [SerializeField] private MMF_Player damageFeedback;
    [SerializeField] private MMF_Player healFeedback;
    [SerializeField] private MMF_Player deathFeedback;



    public float CurrentHealth => currentHealth;

    void Start()
    {
        currentHealth = maxHealth;
        if (healthBar != null)
            healthBar.SetMaxHealth(maxHealth);
    }

    public void TakeDamage(float damage)
    {
        if (damage <= 0f || currentHealth <= 0f)
            return;

        currentHealth = Mathf.Max(0f, currentHealth - damage);

        if (healthBar != null)
            healthBar.SetHealth(currentHealth);

        Debug.Log("System31 Health: " + currentHealth);

        if (currentHealth <= 0)
            Die();
        else 
            damageFeedback.PlayFeedbacks();
    }

    public void Heal(float amount)
    {
        if (amount <= 0f || currentHealth <= 0f)
            return;

        currentHealth = Mathf.Min(maxHealth, currentHealth + amount);
        healFeedback.PlayFeedbacks();

        if (healthBar != null)
            healthBar.SetHealth(currentHealth);
    }

    private void Die()
    {
        deathFeedback.PlayFeedbacks();
        Debug.Log("System31 destroyed!");
    
        Time.timeScale = 0f;
        if (gameOverCanvas != null)
            gameOverCanvas.SetActive(true);
        
        
        
        Destroy(gameObject);
    }
}