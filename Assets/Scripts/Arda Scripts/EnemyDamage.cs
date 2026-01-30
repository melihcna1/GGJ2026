using UnityEngine;

public class EnemyDamage : MonoBehaviour
{
    public float damage = 20f;
    bool hasDamaged = false;

    void OnTriggerEnter2D(Collider2D other)
    {
        if (hasDamaged) return;

        if (other.CompareTag("System31"))
        {
            System31Health health = other.GetComponent<System31Health>();

            if (health != null)
            {
                health.TakeDamage(damage);
            }

            hasDamaged = true;
            Die();
        }
    }

    void Die()
    {
        Destroy(gameObject);
    }
}