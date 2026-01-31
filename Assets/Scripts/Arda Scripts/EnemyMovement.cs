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
    void Start()
    {
        if (randomizeSpeed)
            speed = Random.Range(minSpeed, maxSpeed);

         _lastStepTime = Time.time;
    }

    void Update()
    {
        if (target == null) return;

         float interval = VirusRhythmClock.Instance != null
             ? VirusRhythmClock.Instance.GetIntervalSeconds(entityRythm)
             : Mathf.Max(0.0001f, 1f / Mathf.Max(0.0001f, entityRythm));

         if (Time.time - _lastStepTime < interval)
             return;

         float dt = Time.time - _lastStepTime;
         _lastStepTime = Time.time;

         transform.position +=
             (target.position - transform.position).normalized *
             speed * dt;
    }

    public void SetTarget(Transform t)
    {
        target = t;
    }
}
