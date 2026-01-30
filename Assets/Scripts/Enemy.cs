using UnityEngine;

public class Enemy : MonoBehaviour
{
    [SerializeField] private int maxHealth = 10;

    [Header("Damage Numbers")]
    [SerializeField] private bool showDamageNumbers = true;
    [SerializeField] private Vector3 damageNumberOffset = new Vector3(0f, 1f, 0f);
    [SerializeField] private Color damageNumberColor = Color.white;
    [SerializeField] private float damageNumberTextSize = 0.18f;

    public int MaxHealth => maxHealth;
    public int CurrentHealth { get; private set; }

    private void Awake()
    {
        CurrentHealth = maxHealth;
    }

    public void TakeDamage(int amount)
    {
        if (amount <= 0 || CurrentHealth <= 0)
            return;

        CurrentHealth = Mathf.Max(0, CurrentHealth - amount);

        if (showDamageNumbers)
            SpawnDamageNumber(amount);

        if (CurrentHealth == 0)
            Die();
    }

    public void Heal(int amount)
    {
        if (amount <= 0 || CurrentHealth <= 0)
            return;

        CurrentHealth = Mathf.Min(maxHealth, CurrentHealth + amount);
    }

    private void Die()
    {
        gameObject.SetActive(false);
    }

    private void SpawnDamageNumber(int amount)
    {
        var go = new GameObject("DamageNumber");
        go.transform.position = transform.position + damageNumberOffset;

        var text = go.AddComponent<TextMesh>();
        text.anchor = TextAnchor.MiddleCenter;
        text.alignment = TextAlignment.Center;
        text.fontSize = 64;
        text.characterSize = damageNumberTextSize;
        text.color = damageNumberColor;
        text.text = amount.ToString();

        go.AddComponent<MeshRenderer>();

        var dn = go.AddComponent<DamageNumber>();
        dn.Initialize(amount, damageNumberColor, damageNumberTextSize);
    }
}
