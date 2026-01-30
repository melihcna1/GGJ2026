using System;
using UnityEngine;
using Random = UnityEngine.Random;

public class EnemyFollow : MonoBehaviour
{
    public Transform target;
    public float speed;
    public bool randomizeSpeed = true;
    public float minSpeed = 1f;
    public float maxSpeed = 4f;

    private void Start()
    {
        var targetGo = GameObject.FindWithTag("System31");
        if (targetGo != null)
            target = targetGo.transform;

        if (randomizeSpeed)
            speed = Random.Range(minSpeed, maxSpeed);
    }

    void Update()
    {
        if (target == null) return;

        Vector3 direction = (target.position - transform.position).normalized;
        transform.position += direction * speed * Time.deltaTime;
    }
}