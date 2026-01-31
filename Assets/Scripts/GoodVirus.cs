using MoreMountains.Feedbacks;
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
    [SerializeField] private bool healSystem31OnContact;
    [SerializeField] private float healAmount = 10f;
    [SerializeField] private MMF_Player damageFeedback;
    [SerializeField] private MMF_Player deathFeedback;



    private Transform _target;
    private System31Health _system31Health;
    private bool _counted;
    private float _speed;
    private int _currentHealth;
    private float _accumulator;
    private Rigidbody2D _rb;

    private bool _subscribedToBeat;

    private void Awake()
    {
        var sr = GetComponent<SpriteRenderer>();
        if (sr != null)
            sr.color = goodTint;

        _rb = GetComponent<Rigidbody2D>();
        if (_rb != null)
        {
            _rb.collisionDetectionMode = CollisionDetectionMode2D.Continuous;
            _rb.interpolation = RigidbodyInterpolation2D.Interpolate;
        }
    }

    private void Start()
    {
        EnsureDamageable();
        _currentHealth = Mathf.Max(1, maxHealth);

        if (_rb == null)
            _rb = GetComponent<Rigidbody2D>();
        if (_rb != null)
        {
            _rb.collisionDetectionMode = CollisionDetectionMode2D.Continuous;
            _rb.interpolation = RigidbodyInterpolation2D.Interpolate;
        }

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
        _accumulator = 0f;

        TrySubscribeToBeat();
    }

    private void OnEnable()
    {
        TrySubscribeToBeat();
    }

    private void OnDisable()
    {
        UnsubscribeFromBeat();
    }

    private void Update()
    {
        if (!_subscribedToBeat)
            TrySubscribeToBeat();
    }

    private void FixedUpdate()
    {
        if (_subscribedToBeat)
            return;

        TrySubscribeToBeat();
    }

    private void TrySubscribeToBeat()
    {
        if (_subscribedToBeat)
            return;

        if (VirusRhythmClock.Instance == null)
            return;

        VirusRhythmClock.Instance.Beat += OnBeat;
        _subscribedToBeat = true;
    }

    private void UnsubscribeFromBeat()
    {
        if (!_subscribedToBeat)
            return;

        if (VirusRhythmClock.Instance != null)
            VirusRhythmClock.Instance.Beat -= OnBeat;

        _subscribedToBeat = false;
    }

    private void OnBeat()
    {
        if (_counted)
            return;

        if (_target == null)
            return;

        if (VirusRhythmClock.Instance == null)
            return;

        float dt = VirusRhythmClock.Instance.SecondsPerBeat;
        _accumulator += Mathf.Max(0f, dt);

        float interval = VirusRhythmClock.Instance.GetIntervalSeconds(goodVirusRythm);
        interval = Mathf.Max(0.0001f, interval);

        while (_accumulator + 0.000001f >= interval)
        {
            _accumulator -= interval;
            Step(interval);
        }
    }

    private void Step(float dt)
    {
        if (_counted)
            return;

        if (_target == null)
            return;

        Vector2 currentPos = _rb != null ? _rb.position : (Vector2)transform.position;
        Vector2 toTarget = (Vector2)_target.position - currentPos;
        float dist = toTarget.magnitude;
        if (dist > 0.0001f)
        {
            float moveDist = Mathf.Max(0f, _speed) * Mathf.Max(0f, dt);
            Vector2 newPos = moveDist >= dist
                ? (Vector2)_target.position
                : currentPos + (toTarget / dist) * moveDist;

            if (_rb != null)
                _rb.MovePosition(newPos);
            else
                transform.position = newPos;
        }

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

            Destroy(gameObject);
        }
    }

    public void TakeDamage(int amount)
    {
        if (amount <= 0)
            return;
        
        _currentHealth -= amount;
        if (_currentHealth <= 0)
        {
            deathFeedback.PlayFeedbacks();
            Destroy(gameObject);
        }
        else
        {
            damageFeedback.PlayFeedbacks();

        }
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
