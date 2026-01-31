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
    [SerializeField] private DifficultyCurveController difficulty;
    public float virusSpawnAmp = 0f;
    public float virusSpawnAmpRate = 0f;
    [SerializeField] private float virusSpawnSpeedMultiplierMin = 1f;
    [SerializeField] private float virusSpawnSpeedMultiplierMax = 3f;

    public float goodVirusSpawnInterval = 5f;
    public float goodVirusSpawnAmp = 0f;
    public float goodVirusSpawnAmpRate = 0f;
    [SerializeField] private float goodVirusSpawnSpeedMultiplierMin = 1f;
    [SerializeField] private float goodVirusSpawnSpeedMultiplierMax = 2f;

    public float healingVirusSpawnInterval = 10f;
    public float healingVirusSpawnAmp = 0f;
    public float healingVirusSpawnAmpRate = 0f;
    [SerializeField] private float healingVirusSpawnSpeedMultiplierMin = 1f;
    [SerializeField] private float healingVirusSpawnSpeedMultiplierMax = 1.5f;

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

        float d01 = difficulty != null ? difficulty.GetValue01() : 0f;
        float d = difficulty != null ? difficulty.GetValue() : 1f;
        d = Mathf.Max(0.01f, d);

        float virusSpeedMult = Mathf.Lerp(virusSpawnSpeedMultiplierMin, virusSpawnSpeedMultiplierMax, d01) * d;
        float goodSpeedMult = Mathf.Lerp(goodVirusSpawnSpeedMultiplierMin, goodVirusSpawnSpeedMultiplierMax, d01) * d;
        float healingSpeedMult = Mathf.Lerp(healingVirusSpawnSpeedMultiplierMin, healingVirusSpawnSpeedMultiplierMax, d01) * d;

        TickSpawnLoop(ref _spawnTimer, spawnInterval, virusSpeedMult, enemyPrefab, false);
        TickSpawnLoop(ref _goodSpawnTimer, goodVirusSpawnInterval, goodSpeedMult, goodEnemyPrefab, true);
        TickSpawnLoop(ref _healingSpawnTimer, healingVirusSpawnInterval, healingSpeedMult, healingEnemyPrefab, true);
    }

    private void TickSpawnLoop(ref float timer, float baseInterval, float speedMultiplier, GameObject prefab, bool ensureGoodVirusComponent)
    {
        if (prefab == null)
            return;

        float dt = Time.deltaTime;

        float safeBaseInterval = Mathf.Max(0.01f, baseInterval);
        float safeSpeedMultiplier = Mathf.Max(0.01f, speedMultiplier);
        float interval = safeBaseInterval / safeSpeedMultiplier;
        interval = Mathf.Max(0.01f, interval);

        timer += dt;
        while (timer >= interval)
        {
            timer -= interval;
            Spawn(prefab, ensureGoodVirusComponent);

            interval = safeBaseInterval / safeSpeedMultiplier;
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