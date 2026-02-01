using MoreMountains.Feedbacks;
using UnityEngine;
public class EnemyFollow : MonoBehaviour
{
    public Transform target;

    public bool randomizeSpeed = true;
    public float speed;
    public float minSpeed = 1f;
    public float maxSpeed = 4f;
 
     [SerializeField] private float entityRythm = 1f;
     [SerializeField] private MMF_Player enemyBeatFeedback;

     [Header("Hop-Through Protection")]
     [Tooltip("Radius around System31 to check if enemy hops through it")]
     [SerializeField] private float hitDetectionRadius = 1.5f;
     [SerializeField] private float damage = 20f;
 
     private float _accumulator;
     private Rigidbody2D _rb;

     private bool _subscribedToBeat;
     private bool _hasDealtDamage;

     private void Awake()
     {
         _rb = GetComponent<Rigidbody2D>();
         if (_rb != null)
         {
             _rb.collisionDetectionMode = CollisionDetectionMode2D.Continuous;
             _rb.interpolation = RigidbodyInterpolation2D.Interpolate;
         }
     }

     private void OnEnable()
     {
         TrySubscribeToBeat();
     }

     private void OnDisable()
     {
         UnsubscribeFromBeat();
     }
    void Start()
    {
        if (randomizeSpeed)
            speed = Random.Range(minSpeed, maxSpeed);

         _accumulator = 0f;
         _hasDealtDamage = false;

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
        if (target == null)
            return;

        if (VirusRhythmClock.Instance == null)
            return;

        float dt = VirusRhythmClock.Instance.SecondsPerBeat;
        _accumulator += Mathf.Max(0f, dt);

        float interval = VirusRhythmClock.Instance.GetIntervalSeconds(entityRythm);
        interval = Mathf.Max(0.0001f, interval);

        while (_accumulator + 0.000001f >= interval)
        {
            _accumulator -= interval;
            Step(interval);
        }
    }

    private void Step(float dt)
    {
        if (_hasDealtDamage)
            return;

        Vector2 currentPos = _rb != null ? _rb.position : (Vector2)transform.position;
        Vector2 toTarget = (Vector2)target.position - currentPos;
        float dist = toTarget.magnitude;
        if (dist <= 0.0001f)
        {
            // Already at target - deal damage and destroy
            DealDamageToSystem31();
            return;
        }

        float moveDist = Mathf.Max(0f, speed) * Mathf.Max(0f, dt);
        Vector2 newPos = moveDist >= dist
            ? target.position
            : currentPos + (toTarget / dist) * moveDist;

        // Check if this hop would pass through or reach System31
        // Use line-segment to circle intersection to detect if we hop through the target
        if (WouldHopThroughTarget(currentPos, newPos, (Vector2)target.position, hitDetectionRadius))
        {
            // Snap to the target and deal damage
            if (_rb != null)
                _rb.MovePosition(target.position);
            else
                transform.position = target.position;

            enemyBeatFeedback.PlayFeedbacks();
            DealDamageToSystem31();
            return;
        }

        if (_rb != null)
            _rb.MovePosition(newPos);
        else
            transform.position = newPos;

        enemyBeatFeedback.PlayFeedbacks();
    }

    /// <summary>
    /// Check if a line segment from start to end passes within radius of the target point.
    /// This catches cases where the enemy hops over the System31 without triggering collision.
    /// </summary>
    private bool WouldHopThroughTarget(Vector2 start, Vector2 end, Vector2 targetPos, float radius)
    {
        // Vector from start to end
        Vector2 lineDir = end - start;
        float lineLengthSq = lineDir.sqrMagnitude;

        // Vector from start to target
        Vector2 toTarget = targetPos - start;

        // If the line has no length, just check distance from start to target
        if (lineLengthSq < 0.0001f)
            return toTarget.magnitude <= radius;

        // Project target onto the line segment, clamping to [0, 1]
        float t = Mathf.Clamp01(Vector2.Dot(toTarget, lineDir) / lineLengthSq);

        // Find the closest point on the line segment to the target
        Vector2 closestPoint = start + t * lineDir;

        // Check if this closest point is within the hit radius
        float distSq = (targetPos - closestPoint).sqrMagnitude;
        return distSq <= radius * radius;
    }

    private void DealDamageToSystem31()
    {
        if (_hasDealtDamage)
            return;

        _hasDealtDamage = true;

        if (target != null)
        {
            var health = target.GetComponent<System31Health>();
            if (health == null)
                health = target.GetComponentInParent<System31Health>();
            if (health == null)
                health = target.GetComponentInChildren<System31Health>();

            if (health != null)
                health.TakeDamage(damage);
        }

        Destroy(gameObject);
    }

    public void SetTarget(Transform t)
    {
        target = t;
    }
}
