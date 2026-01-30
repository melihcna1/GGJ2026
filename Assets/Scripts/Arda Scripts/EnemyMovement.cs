using System;
using UnityEngine;
using Random = UnityEngine.Random;

public class EnemyFollow : MonoBehaviour
{
    public Transform target;
    public float speed ;

    private void Start()
    {
        target = GameObject.FindWithTag("System31").transform;
        speed = Random.Range(1f, 4f);
    }

    void Update()
    {
        if (target == null) return;

        Vector3 direction = (target.position - transform.position).normalized;
        transform.position += direction * speed * Time.deltaTime;
    }
}