using UnityEngine;

public class EnemyAttack : MonoBehaviour
{
    public float damage = 10f;

    private void OnCollisionEnter2D(Collision2D col)
    {
        if (col.collider.CompareTag("RecycleDummy"))
        {
            col.collider.GetComponent<RecycleDummyHealth>()?.TakeDamage(damage);
            Destroy(gameObject);
        }
        else if (col.collider.CompareTag("System31"))
        {
            col.collider.GetComponent<System31Health>()?.TakeDamage(damage);
            Destroy(gameObject);
        }
    }
}