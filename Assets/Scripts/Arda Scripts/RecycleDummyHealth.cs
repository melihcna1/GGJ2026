using UnityEngine;
using UnityEngine.UI;

public class RecycleDummyHealth : MonoBehaviour
{
    public string dummyName;
    
    public float maxHealth = 40f;

    private float currentHealth;

    [Header("UI")]
    [SerializeField] private GameObject healthBarRoot;
    [SerializeField] private Image healthBarFill;
    [SerializeField] private bool hideWhenFull = true;

    private void Awake()
    {
        currentHealth = Mathf.Max(0f, maxHealth);
        gameObject.name = dummyName;
        UpdateHealthUI();
    }

    public void TakeDamage(float damage)
    {
        currentHealth -= Mathf.Max(0f, damage);
        currentHealth = Mathf.Clamp(currentHealth, 0f, Mathf.Max(0f, maxHealth));
        Debug.Log("RecycleDummy Health: " + currentHealth);
        UpdateHealthUI();
        if (currentHealth <= 0f)
            Destroy(gameObject);
    }

    private void UpdateHealthUI()
    {
        if (healthBarFill != null)
        {
            float denom = Mathf.Max(0.0001f, maxHealth);
            healthBarFill.fillAmount = Mathf.Clamp01(currentHealth / denom);
        }

        if (healthBarRoot != null)
            healthBarRoot.SetActive(!(hideWhenFull && currentHealth >= maxHealth));
    }
}