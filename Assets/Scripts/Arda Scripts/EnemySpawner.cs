using UnityEngine;

public class EnemySpawner : MonoBehaviour
{
    public GameObject enemyPrefab;
    public GameObject goodEnemyPrefab;
    public GameObject healingEnemyPrefab;
    [Range(0f, 1f)] public float goodSpawnChance = 0.2f;
    [Range(0f, 1f)] public float healingSpawnChance = 0.1f;
    public float spawnInterval = 2f;

    [Header("Difficulty")]
    public float virusSpawnAmp = 0f;
    public float virusSpawnAmpRate = 0f;

    public float goodVirusSpawnInterval = 5f;
    public float goodVirusSpawnAmp = 0f;
    public float goodVirusSpawnAmpRate = 0f;

    public float healingVirusSpawnInterval = 10f;
    public float healingVirusSpawnAmp = 0f;
    public float healingVirusSpawnAmpRate = 0f;

    [SerializeField] private Camera cam;

    private float _spawnTimer;
    private float _goodSpawnTimer;
    private float _healingSpawnTimer;

    private void Awake()
    {
        if (cam == null)
            cam = Camera.main;
    }

    void Start()
    {
        if (cam == null)
        {
            enabled = false;

            return;
        }

        if (enemyPrefab == null)
        {
            enabled = false;
            return;
        }

        if (spawnInterval <= 0f)
            spawnInterval = 0.25f;

        _spawnTimer = 1f;
        _goodSpawnTimer = 1f;
        _healingSpawnTimer = 1f;
    }

    private void Update()
    {
        if (cam == null || enemyPrefab == null)
            return;

        float dt = Time.deltaTime;
        virusSpawnAmp = Mathf.Max(0f, virusSpawnAmp + Mathf.Max(0f, virusSpawnAmpRate) * dt);
        goodVirusSpawnAmp = Mathf.Max(0f, goodVirusSpawnAmp + Mathf.Max(0f, goodVirusSpawnAmpRate) * dt);
        healingVirusSpawnAmp = Mathf.Max(0f, healingVirusSpawnAmp + Mathf.Max(0f, healingVirusSpawnAmpRate) * dt);

        TickSpawnLoop(ref _spawnTimer, spawnInterval, virusSpawnAmp, enemyPrefab, false);
        TickSpawnLoop(ref _goodSpawnTimer, goodVirusSpawnInterval, goodVirusSpawnAmp, goodEnemyPrefab, true);
        TickSpawnLoop(ref _healingSpawnTimer, healingVirusSpawnInterval, healingVirusSpawnAmp, healingEnemyPrefab, true);
    }

    private void TickSpawnLoop(ref float timer, float baseInterval, float amp, GameObject prefab, bool ensureGoodVirusComponent)
    {
        if (prefab == null)
            return;

        float dt = Time.deltaTime;

        float safeBaseInterval = Mathf.Max(0.01f, baseInterval);
        float interval = safeBaseInterval / (1f + Mathf.Max(0f, amp));
        interval = Mathf.Max(0.01f, interval);

        timer += dt;
        while (timer >= interval)
        {
            timer -= interval;
            Spawn(prefab, ensureGoodVirusComponent);

            interval = safeBaseInterval / (1f + Mathf.Max(0f, amp));
            interval = Mathf.Max(0.01f, interval);
        }
    }

    private void Spawn(GameObject prefab, bool ensureGoodVirusComponent)
    {
        if (cam == null || prefab == null)
            return;

        Vector2 spawnPos = GetSpawnPosition();
        var go = Instantiate(prefab, spawnPos, Quaternion.identity);
        if (ensureGoodVirusComponent && go != null && go.GetComponent<GoodVirus>() == null)
            go.AddComponent<GoodVirus>();
    }

    Vector2 GetSpawnPosition()
    {
        float screenX = cam.orthographicSize * cam.aspect;
        float screenY = cam.orthographicSize;

        int side = Random.Range(0, 4); // 0=top,1=bottom,2=left,3=right

        switch (side)
        {
            case 0: return new Vector2(Random.Range(-screenX, screenX), screenY + 1f);
            case 1: return new Vector2(Random.Range(-screenX, screenX), -screenY - 1f);
            case 2: return new Vector2(-screenX - 1f, Random.Range(-screenY, screenY));
            default: return new Vector2(screenX + 1f, Random.Range(-screenY, screenY));
        }
    }
}