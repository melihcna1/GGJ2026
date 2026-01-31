using UnityEngine;
public class EnemyFollow : MonoBehaviour
{
    public Transform target;

    public bool randomizeSpeed = true;
    public float speed;
    public float minSpeed = 1f;
    public float maxSpeed = 4f;
    void Start()
    {
        if (randomizeSpeed)
            speed = Random.Range(minSpeed, maxSpeed);
    }

    void Update()
    {
        if (target == null) return;

        transform.position +=
            (target.position - transform.position).normalized *
            speed * Time.deltaTime;
    }

    public void SetTarget(Transform t)
    {
        target = t;
    }
}
