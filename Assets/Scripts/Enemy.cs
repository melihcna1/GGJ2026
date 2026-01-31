using UnityEngine;

public class Enemy : MonoBehaviour
{
    [SerializeField] private int maxHealth = 10;

    [Header("Animation")]
    [SerializeField] private Animator animator;
    [SerializeField] private string lickTrigger = "islick";
    [SerializeField] private string idleTrigger = "idle";
    [SerializeField] private float minTriggerIntervalSeconds = 2f;
    [SerializeField] private float maxTriggerIntervalSeconds = 4f;

    [Header("Damage Numbers")]
    [SerializeField] private bool showDamageNumbers = true;
    [SerializeField] private Vector3 damageNumberOffset = new Vector3(0f, 1f, 0f);
    [SerializeField] private Color damageNumberColor = Color.white;
    [SerializeField] private float damageNumberTextSize = 0.18f;

    public int MaxHealth => maxHealth;
    public int CurrentHealth { get; private set; }

    private float _animTimer;
    private float _nextAnimTriggerTime;
    private int _lickTriggerHash;
    private int _idleTriggerHash;

    private void Awake()
    {
        CurrentHealth = maxHealth;

        if (animator == null)
            animator = GetComponentInChildren<Animator>();

        _lickTriggerHash = string.IsNullOrEmpty(lickTrigger) ? 0 : Animator.StringToHash(lickTrigger);
        _idleTriggerHash = string.IsNullOrEmpty(idleTrigger) ? 0 : Animator.StringToHash(idleTrigger);

        _animTimer = 0f;
        _nextAnimTriggerTime = Random.Range(
            Mathf.Max(0f, minTriggerIntervalSeconds),
            Mathf.Max(Mathf.Max(0f, minTriggerIntervalSeconds), maxTriggerIntervalSeconds)
        );
    }

    private void Update()
    {
        if (CurrentHealth <= 0)
            return;

        if (animator == null)
            return;

        _animTimer += Time.deltaTime;
        if (_animTimer < _nextAnimTriggerTime)
            return;

        _animTimer = 0f;
        _nextAnimTriggerTime = Random.Range(
            Mathf.Max(0f, minTriggerIntervalSeconds),
            Mathf.Max(Mathf.Max(0f, minTriggerIntervalSeconds), maxTriggerIntervalSeconds)
        );

        bool doLick = Random.value < 0.5f;
        int triggerHash = doLick ? _lickTriggerHash : _idleTriggerHash;
        if (triggerHash != 0)
            animator.SetTrigger(triggerHash);
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
