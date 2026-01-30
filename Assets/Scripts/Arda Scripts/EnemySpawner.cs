using UnityEngine;

public class EnemySpawner : MonoBehaviour
{
    public GameObject enemyPrefab;
    public GameObject goodEnemyPrefab;
    [Range(0f, 1f)] public float goodSpawnChance = 0.2f;
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
        bool spawnGood = Random.value < Mathf.Clamp01(goodSpawnChance);
        var prefab = spawnGood && goodEnemyPrefab != null ? goodEnemyPrefab : enemyPrefab;

        var go = Instantiate(prefab, spawnPos, Quaternion.identity);
        if (spawnGood && go != null && go.GetComponent<GoodVirus>() == null)
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