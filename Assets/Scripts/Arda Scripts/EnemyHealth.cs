using MoreMountains.Feedbacks;
using UnityEngine;

public class EnemyHealth : MonoBehaviour
{
    public float maxHealth = 100f;

    public float currentHealth = 20;

    public HealthBar healthBar;
    
    [SerializeField] private MMF_Player damageFeedback;
    [SerializeField] private MMF_Player deathFeedback;



    private bool _hasScored;

    void Start()
    {
        currentHealth = maxHealth;
        _hasScored = false;
        healthBar.SetMaxHealth(maxHealth);

    }

    private void DamageTick()
    {
        TakeDamage(50f);
    }

    public void TakeDamage(float damage)
    {
        damageFeedback.PlayFeedbacks();
        currentHealth -= damage;
        healthBar.SetHealth(currentHealth);
        

        if (currentHealth <= 0)
        {
            Die();
        }
    }

    void Die()
    {
        deathFeedback.PlayFeedbacks();
        CancelInvoke();   

        if (!_hasScored)
        {
            _hasScored = true;
            if (ScoreManager.Instance != null)
                ScoreManager.Instance.AddKillScore();
        }

        Destroy(gameObject);
    }
}