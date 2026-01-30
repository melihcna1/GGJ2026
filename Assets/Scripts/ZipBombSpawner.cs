using UnityEngine;

public class ZipBombSpawner : MonoBehaviour
{
    public GameObject zipbombPrefab;

    [Header("Spawning")]
    public float zipbombSpawnRate = 0.1f;
    public float zipbombSpawnAmp = 0.02f;

    [SerializeField] private Camera cam;
    [SerializeField] private float spawnOffsetWorld = 1f;

    private float _elapsed;
    private float _timer;

    private void Awake()
    {
        if (cam == null)
            cam = Camera.main;
    }

    private void Update()
    {
        if (cam == null || zipbombPrefab == null)
            return;

        _elapsed += Time.deltaTime;

        float rate = Mathf.Max(0f, zipbombSpawnRate) + Mathf.Max(0f, zipbombSpawnAmp) * _elapsed;
        if (rate <= 0f)
            return;

        float interval = 1f / rate;

        _timer += Time.deltaTime;
        while (_timer >= interval)
        {
            _timer -= interval;
            SpawnZipBomb();

            rate = Mathf.Max(0f, zipbombSpawnRate) + Mathf.Max(0f, zipbombSpawnAmp) * _elapsed;
            if (rate <= 0f)
                break;
            interval = 1f / rate;
        }
    }

    private void SpawnZipBomb()
    {
        if (cam == null || zipbombPrefab == null)
            return;

        Vector2 spawnPos = GetSpawnPosition();
        Instantiate(zipbombPrefab, spawnPos, Quaternion.identity);
    }

    private Vector2 GetSpawnPosition()
    {
        float screenX = cam.orthographicSize * cam.aspect;
        float screenY = cam.orthographicSize;

        int side = Random.Range(0, 4);
        switch (side)
        {
            case 0: return new Vector2(Random.Range(-screenX, screenX), screenY + spawnOffsetWorld);
            case 1: return new Vector2(Random.Range(-screenX, screenX), -screenY - spawnOffsetWorld);
            case 2: return new Vector2(-screenX - spawnOffsetWorld, Random.Range(-screenY, screenY));
            default: return new Vector2(screenX + spawnOffsetWorld, Random.Range(-screenY, screenY));
        }
    }
}
