using UnityEngine;

[RequireComponent(typeof(SpriteRenderer))]
public class GoodVirus : MonoBehaviour
{
    [SerializeField] private float contactDistance = 0.35f;
    [SerializeField] private string system31Tag = "System31";
    [SerializeField] private Color goodTint = new Color(0.25f, 0.9f, 0.35f, 1f);
    [SerializeField] private bool overrideMoveSpeed;
    [SerializeField] private float moveSpeed = 2.5f;

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
        ApplySpeedOverride();

        var go = GameObject.FindWithTag(system31Tag);
        if (go != null)
            _target = go.transform;

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
            GoodVirusProgress.Instance.AddProgress(1);
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
