using UnityEngine;

[RequireComponent(typeof(EnemyFollow))]
public class EnemyTargetSelector : MonoBehaviour
{
    public float scanRadius = 6f;
    public float scanInterval = 0.5f;

    private EnemyFollow follow;
    private float timer;

    private void Awake()
    {
        follow = GetComponent<EnemyFollow>();
    }

    private void Update()
    {
        timer -= Time.deltaTime;
        if (timer <= 0f)
        {
            timer = scanInterval;
            SelectTarget();
        }
    }

    void SelectTarget()
    {
        Transform closest = null;
        float bestDist = float.MaxValue;

        foreach (var d in GameObject.FindGameObjectsWithTag("RecycleDummy"))
        {
            float dist = Vector2.Distance(transform.position, d.transform.position);
            if (dist < bestDist && dist <= scanRadius)
            {
                bestDist = dist;
                closest = d.transform;
            }
        }

        if (closest == null)
        {
            var sys = GameObject.FindWithTag("System31");
            if (sys != null)
                closest = sys.transform;
        }

        if (closest != null)
            follow.SetTarget(closest);
    }
}