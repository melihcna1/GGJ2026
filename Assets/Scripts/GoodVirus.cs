using UnityEngine;

[RequireComponent(typeof(SpriteRenderer))]
public class GoodVirus : MonoBehaviour
{
    [SerializeField] private float contactDistance = 0.35f;
    [SerializeField] private string system31Tag = "System31";
    [SerializeField] private Color goodTint = new Color(0.25f, 0.9f, 0.35f, 1f);
    [SerializeField] private bool overrideMoveSpeed;
    [SerializeField] private float moveSpeed = 2.5f;
    [SerializeField] private bool randomizeSpeed = true;
    [SerializeField] private float minSpeed = 1f;
    [SerializeField] private float maxSpeed = 4f;
    [SerializeField] private float goodVirusRythm = 1f;
    [SerializeField] private int maxHealth = 1;
    [SerializeField] private bool grantProgressOnContact = true;
    [SerializeField] private int progressAmount = 1;
    [SerializeField] private bool healSystem31OnContact;
    [SerializeField] private float healAmount = 10f;

    private Transform _target;
    private System31Health _system31Health;
    private bool _counted;
    private float _speed;
    private int _currentHealth;
    private float _lastStepTime;

    private void Awake()
    {
        var sr = GetComponent<SpriteRenderer>();
        if (sr != null)
            sr.color = goodTint;
    }

    private void Start()
    {
        EnsureDamageable();
        _currentHealth = Mathf.Max(1, maxHealth);

        RemoveEnemyDependencies();

        var go = GameObject.FindWithTag(system31Tag);
        if (go != null)
            _target = go.transform;

        if (_target == null)
        {
            _system31Health = FindFirstObjectByType<System31Health>();
            if (_system31Health != null)
                _target = _system31Health.transform;
        }
        else
        {
            _system31Health = _target.GetComponent<System31Health>();
            if (_system31Health == null)
                _system31Health = _target.GetComponentInParent<System31Health>();
            if (_system31Health == null)
                _system31Health = _target.GetComponentInChildren<System31Health>();
        }

        ApplySpeedOverride();
        _lastStepTime = Time.time;

        if (grantProgressOnContact)
        {
            if (GoodVirusProgress.Instance == null)
            {
                var mgr = new GameObject("GoodVirusProgress");
                mgr.AddComponent<GoodVirusProgress>();
            }

            if (FindFirstObjectByType<GoodVirusProgressUI>() == null)
            {
                var ui = new GameObject("GoodVirusProgressUI");
                ui.AddComponent<GoodVirusProgressUI>();
            }
        }
    }

    private void Update()
    {
        if (_counted)
            return;

        if (_target == null)
            return;

        float interval = VirusRhythmClock.Instance != null
            ? VirusRhythmClock.Instance.GetIntervalSeconds(goodVirusRythm)
            : Mathf.Max(0.0001f, 1f / Mathf.Max(0.0001f, goodVirusRythm));

        if (Time.time - _lastStepTime < interval)
            return;

        float dt = Time.time - _lastStepTime;
        _lastStepTime = Time.time;

        var direction = (_target.position - transform.position).normalized;
        transform.position += direction * _speed * dt;

        float maxDist = Mathf.Max(0.01f, contactDistance);
        var delta = _target.position - transform.position;
        if (delta.sqrMagnitude <= maxDist * maxDist)
        {
            _counted = true;

            if (healSystem31OnContact)
            {
                if (_system31Health == null && _target != null)
                {
                    _system31Health = _target.GetComponent<System31Health>();
                    if (_system31Health == null)
                        _system31Health = _target.GetComponentInParent<System31Health>();
                    if (_system31Health == null)
                        _system31Health = _target.GetComponentInChildren<System31Health>();
                }

                if (_system31Health != null)
                    _system31Health.Heal(healAmount);
            }

            if (grantProgressOnContact && GoodVirusProgress.Instance != null)
                GoodVirusProgress.Instance.AddProgress(progressAmount);

            Destroy(gameObject);
        }
    }

    public void TakeDamage(int amount)
    {
        if (amount <= 0)
            return;

        _currentHealth -= amount;
        if (_currentHealth <= 0)
            Destroy(gameObject);
    }

    private void EnsureDamageable()
    {
        if (GetComponentInChildren<Collider2D>() == null && GetComponentInParent<Collider2D>() == null)
            gameObject.AddComponent<BoxCollider2D>();

        if (GetComponentInChildren<Rigidbody2D>() == null && GetComponentInParent<Rigidbody2D>() == null)
        {
            var rb = gameObject.AddComponent<Rigidbody2D>();
            rb.gravityScale = 0f;
        }
    }

    private void RemoveEnemyDependencies()
    {
        var follow = GetComponent<EnemyFollow>();
        if (follow != null)
            Destroy(follow);

        var enemy = GetComponent<Enemy>();
        if (enemy != null)
            Destroy(enemy);

        var enemyHealth = GetComponent<EnemyHealth>();
        if (enemyHealth != null)
            Destroy(enemyHealth);
    }

    private void ApplySpeedOverride()
    {
        if (overrideMoveSpeed)
        {
            _speed = Mathf.Max(0f, moveSpeed);
            return;
        }

        if (randomizeSpeed)
            _speed = Random.Range(minSpeed, maxSpeed);
        else
            _speed = Mathf.Max(0f, moveSpeed);
    }
}
