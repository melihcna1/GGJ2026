using UnityEngine;

[RequireComponent(typeof(SpriteRenderer))]
public class GoodVirus : MonoBehaviour
{
    [SerializeField] private float contactDistance = 0.35f;
    [SerializeField] private string system31Tag = "System31";
    [SerializeField] private Color goodTint = new Color(0.25f, 0.9f, 0.35f, 1f);
    [SerializeField] private bool overrideMoveSpeed;
    [SerializeField] private float moveSpeed = 2.5f;
    [SerializeField] private bool grantProgressOnContact = true;
    [SerializeField] private int progressAmount = 1;
    [SerializeField] private bool healSystem31OnContact;
    [SerializeField] private float healAmount = 10f;

    private Transform _target;
    private bool _counted;

    private void Awake()
    {
        var sr = GetComponent<SpriteRenderer>();
        if (sr != null)
            sr.color = goodTint;
    }

    private void Start()
    {
        EnsureDamageable();

        var go = GameObject.FindWithTag(system31Tag);
        if (go != null)
            _target = go.transform;

        EnsureMovement();
        ApplySpeedOverride();

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

        float maxDist = Mathf.Max(0.01f, contactDistance);
        var delta = _target.position - transform.position;
        if (delta.sqrMagnitude <= maxDist * maxDist)
        {
            _counted = true;

            if (healSystem31OnContact)
            {
                var systemHealth = _target.GetComponent<System31Health>();
                if (systemHealth != null)
                    systemHealth.Heal(healAmount);
            }

            if (grantProgressOnContact && GoodVirusProgress.Instance != null)
                GoodVirusProgress.Instance.AddProgress(progressAmount);

            Destroy(gameObject);
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

        if (GetComponentInParent<EnemyHealth>() == null && GetComponentInParent<Enemy>() == null)
            gameObject.AddComponent<Enemy>();
    }

    private void EnsureMovement()
    {
        var follow = GetComponent<EnemyFollow>();
        if (follow == null)
            follow = gameObject.AddComponent<EnemyFollow>();

        if (follow != null && follow.target == null && _target != null)
            follow.target = _target;
    }

    private void ApplySpeedOverride()
    {
        if (!overrideMoveSpeed)
            return;

        var follow = GetComponent<EnemyFollow>();
        if (follow != null)
        {
            follow.randomizeSpeed = false;
            follow.speed = moveSpeed;
        }
    }
}
