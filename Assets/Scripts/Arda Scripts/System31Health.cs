using System.Collections;
using MoreMountains.Feedbacks;
using UnityEngine;

public class System31Health : MonoBehaviour
{
    [SerializeField] private float maxHealth = 100f;
    private float currentHealth;

    [Header("Animation")]
    [SerializeField] private Animator system31Animator;
    [SerializeField] private string dieParameterName = "isdie";
    [SerializeField] private float deathFreezeDelay = 0.25f;
    private bool _died;

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

        ComboManager.EnsureInstance().ResetCombo();

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
        if (_died)
            return;

        _died = true;

        if (system31Animator == null)
            system31Animator = GetComponentInChildren<Animator>();

        TrySetDieAnimation();

        StartCoroutine(DieRoutine());
    }

    private IEnumerator DieRoutine()
    {
        deathFeedback.PlayFeedbacks();
        Debug.Log("System31 destroyed!");

        float delay = Mathf.Max(0f, deathFreezeDelay);
        if (delay > 0f)
            yield return new WaitForSecondsRealtime(delay);

        Time.timeScale = 0f;
        if (gameOverCanvas != null)
            gameOverCanvas.SetActive(true);

        Destroy(gameObject);
    }

    private void TrySetDieAnimation()
    {
        if (system31Animator == null)
            return;

        if (string.IsNullOrWhiteSpace(dieParameterName))
            return;

        AnimatorControllerParameterType? foundType = null;
        var parameters = system31Animator.parameters;
        for (int i = 0; i < parameters.Length; i++)
        {
            if (parameters[i].name == dieParameterName)
            {
                foundType = parameters[i].type;
                break;
            }
        }

        if (foundType == AnimatorControllerParameterType.Trigger)
            system31Animator.SetTrigger(dieParameterName);
        else if (foundType == AnimatorControllerParameterType.Bool)
            system31Animator.SetBool(dieParameterName, true);
    }
}