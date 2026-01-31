using UnityEngine;

public class RecycleDummyHealth : MonoBehaviour
{
    public string dummyName;
    
    public float maxHealth = 40f;

    private float currentHealth;

    private void Awake()
    {
        currentHealth = maxHealth;
        gameObject.name = dummyName;
    }

    public void TakeDamage(float damage)
    {
        currentHealth -= damage;
        Debug.Log("RecycleDummy Health: " + currentHealth);
        if (currentHealth <= 0f)
            Destroy(gameObject);
    }
}