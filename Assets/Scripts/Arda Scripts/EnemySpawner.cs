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

    [SerializeField] private Camera cam;

    private float _spawnTimer;

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
    }

    private void Update()
    {
        if (cam == null || enemyPrefab == null)
            return;

        float dt = Time.deltaTime;
        virusSpawnAmp = Mathf.Max(0f, virusSpawnAmp + Mathf.Max(0f, virusSpawnAmpRate) * dt);

        float baseInterval = Mathf.Max(0.01f, spawnInterval);
        float interval = baseInterval / (1f + virusSpawnAmp);
        interval = Mathf.Max(0.01f, interval);

        _spawnTimer += dt;
        while (_spawnTimer >= interval)
        {
            _spawnTimer -= interval;
            SpawnEnemy();

            interval = baseInterval / (1f + virusSpawnAmp);
            interval = Mathf.Max(0.01f, interval);
        }
    }

    void SpawnEnemy()
    {
        if (cam == null || enemyPrefab == null)
            return;

        Vector2 spawnPos = GetSpawnPosition();

        float healingChance = Mathf.Clamp01(healingSpawnChance);
        float goodChance = Mathf.Clamp01(goodSpawnChance);
        float roll = Random.value;

        bool spawnHealing = healingEnemyPrefab != null && roll < healingChance;
        bool spawnGood = !spawnHealing && goodEnemyPrefab != null && roll < healingChance + goodChance;

        var prefab = spawnHealing ? healingEnemyPrefab : (spawnGood ? goodEnemyPrefab : enemyPrefab);

        if (prefab == null)
            return;

        var go = Instantiate(prefab, spawnPos, Quaternion.identity);
        if ((spawnGood || spawnHealing) && go != null && go.GetComponent<GoodVirus>() == null)
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