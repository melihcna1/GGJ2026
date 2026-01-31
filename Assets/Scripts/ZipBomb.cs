using MoreMountains.Feedbacks;
using UnityEngine;

public class ZipBomb : MonoBehaviour
{
    [Header("Explosion")]
    [SerializeField] private float zipbombDamage = 20f;
    [SerializeField] private Animator zipbombExplosion;
    [SerializeField] private string explosionTrigger = "Explode";
    [SerializeField] private float destroyAfterExplodeSeconds = 0.75f;
    [SerializeField] private MMF_Player ExplosionFeedback;


    [Header("Movement")]
    [SerializeField] private float roamSpeed = 2.5f;
    [SerializeField] private float directionChangeInterval = 0.75f;
    [SerializeField] private float zipBombRythm = 1f;

    [Header("Lifetime")]
    [SerializeField] private float minLifetimeSeconds = 2f;
    [SerializeField] private float offscreenMarginViewport = 0.15f;

    [Header("Player")]
    [SerializeField] private string system31Tag = "System31";

    private Camera _cam;
    private Rigidbody2D _rb;
    private Vector2 _moveDir;
    private float _dirTimer;
    private float _lifetime;
    private bool _exploded;
    private float _lastStepTime;

    private void Awake()
    {
        _cam = Camera.main;
        _rb = GetComponent<Rigidbody2D>();
        PickNewDirection();
        _lastStepTime = Time.time;
    }

    private void Update()
    {
        if (_exploded)
            return;

        _lifetime += Time.deltaTime;

        _dirTimer -= Time.deltaTime;
        if (_dirTimer <= 0f)
            PickNewDirection();

        if (_cam != null && _lifetime >= minLifetimeSeconds)
        {
            var vp = _cam.WorldToViewportPoint(transform.position);
            if (vp.x < -offscreenMarginViewport || vp.x > 1f + offscreenMarginViewport ||
                vp.y < -offscreenMarginViewport || vp.y > 1f + offscreenMarginViewport)
            {
                Destroy(gameObject);
            }
        }
    }

    private void FixedUpdate()
    {
        if (_exploded)
            return;

        if (_rb != null)
            _rb.linearVelocity = Vector2.zero;

        if (VirusRhythmClock.Instance == null)
            return;

        float interval = VirusRhythmClock.Instance.GetIntervalSeconds(zipBombRythm);

        if (Time.time - _lastStepTime < interval)
            return;

        float dt = Time.time - _lastStepTime;
        _lastStepTime = Time.time;

        var vel = _moveDir * Mathf.Max(0f, roamSpeed);
        var delta = vel * dt;
        if (_rb != null)
            _rb.MovePosition(_rb.position + delta);
        else
            transform.position += (Vector3)delta;
    }

    private void PickNewDirection()
    {
        _dirTimer = Mathf.Max(0.05f, directionChangeInterval);
        _moveDir = Random.insideUnitCircle;
        if (_moveDir.sqrMagnitude < 0.0001f)
            _moveDir = Vector2.right;
        _moveDir.Normalize();
    }

    public void Explode()
    {
        if (_exploded)
            return;

        _exploded = true;

        if (_rb != null)
            _rb.linearVelocity = Vector2.zero;

        if (zipbombExplosion != null)
        {
            ExplosionFeedback.PlayFeedbacks();
            zipbombExplosion.SetTrigger(explosionTrigger);
        }
            

        var player = GameObject.FindWithTag(system31Tag);
        if (player != null)
        {
            var health = player.GetComponent<System31Health>();
            if (health == null)
                health = player.GetComponentInParent<System31Health>();
            if (health == null)
                health = player.GetComponentInChildren<System31Health>();

            if (health != null)
                health.TakeDamage(zipbombDamage);
        }

        Destroy(gameObject, Mathf.Max(0.01f, destroyAfterExplodeSeconds));
    }
}
