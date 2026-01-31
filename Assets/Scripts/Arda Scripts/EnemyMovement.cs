using UnityEngine;
public class EnemyFollow : MonoBehaviour
{
    public Transform target;

    public bool randomizeSpeed = true;
    public float speed;
    public float minSpeed = 1f;
    public float maxSpeed = 4f;
 
     [SerializeField] private float entityRythm = 1f;
 
     private float _lastStepTime;
     private Rigidbody2D _rb;

     private void Awake()
     {
         _rb = GetComponent<Rigidbody2D>();
         if (_rb != null)
         {
             _rb.collisionDetectionMode = CollisionDetectionMode2D.Continuous;
             _rb.interpolation = RigidbodyInterpolation2D.Interpolate;
         }
     }
    void Start()
    {
        if (randomizeSpeed)
            speed = Random.Range(minSpeed, maxSpeed);

         _lastStepTime = Time.time;
    }

    void FixedUpdate()
    {
        if (target == null) return;

         float interval = VirusRhythmClock.Instance != null
             ? VirusRhythmClock.Instance.GetIntervalSeconds(entityRythm)
             : Mathf.Max(0.0001f, 1f / Mathf.Max(0.0001f, entityRythm));

         if (Time.time - _lastStepTime < interval)
             return;

         float dt = Time.time - _lastStepTime;
         _lastStepTime = Time.time;

         Vector2 currentPos = _rb != null ? _rb.position : (Vector2)transform.position;
         Vector2 toTarget = (Vector2)target.position - currentPos;
         float dist = toTarget.magnitude;
         if (dist <= 0.0001f)
             return;

         float moveDist = Mathf.Max(0f, speed) * dt;
         Vector2 newPos = moveDist >= dist
             ? (Vector2)target.position
             : currentPos + (toTarget / dist) * moveDist;

         if (_rb != null)
             _rb.MovePosition(newPos);
         else
             transform.position = newPos;
    }

    public void SetTarget(Transform t)
    {
        target = t;
    }
}
