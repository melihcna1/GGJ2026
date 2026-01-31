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

 
     private float _accumulator;
     private Rigidbody2D _rb;

     private bool _subscribedToBeat;

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
        Vector2 currentPos = _rb != null ? _rb.position : (Vector2)transform.position;
        Vector2 toTarget = (Vector2)target.position - currentPos;
        float dist = toTarget.magnitude;
        if (dist <= 0.0001f)
            return;

        float moveDist = Mathf.Max(0f, speed) * Mathf.Max(0f, dt);
        Vector2 newPos = moveDist >= dist
            ? target.position
            : currentPos + (toTarget / dist) * moveDist;

        if (_rb != null)
            _rb.MovePosition(newPos);
        else
            transform.position = newPos;

        enemyBeatFeedback.PlayFeedbacks();
    }

    public void SetTarget(Transform t)
    {
        target = t;
    }
}
