using UnityEngine;

public class EnemySpawner : MonoBehaviour
{
    public GameObject enemyPrefab;
    public float spawnInterval = 2f;

    Camera cam;

    void Start()
    {
        cam = Camera.main;
        InvokeRepeating(nameof(SpawnEnemy), 1f, spawnInterval);
    }

    void SpawnEnemy()
    {
        Vector2 spawnPos = GetSpawnPosition();
        Instantiate(enemyPrefab, spawnPos, Quaternion.identity);
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