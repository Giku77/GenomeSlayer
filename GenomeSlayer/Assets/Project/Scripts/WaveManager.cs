using UnityEngine;

public class WaveManager : MonoBehaviour
{
    public Transform spawnPoint1;

    public WaveDef waveDef;

    private int currentEnemyCount = 0;

    public GameObject enemyPrefab;

    private void Awake()
    {
        EventBus.EnemyDied += OnEnemyDefeated;
    }

    private void Start()
    {
        waveDef.currentEnemyCount = 0;
        waveDef.currentWave = 0;
    }

    private void Update()
    {
        if(currentEnemyCount == 0)
        {
            Debug.Log("Spawning Wave " + (waveDef.currentWave + 1));
            SpawnWave();
        }
    }

    private void SpawnWave()
    {
        waveDef.currentWave++;
        currentEnemyCount = waveDef.maxEnemyCount + (waveDef.currentWave * 2);
        waveDef.currentEnemyCount = currentEnemyCount;
        for (int i = 0; i < currentEnemyCount; i++)
        {
            Instantiate(enemyPrefab, spawnPoint1.position, Quaternion.identity);
        }
    }

    public void OnEnemyDefeated()
    {
        currentEnemyCount--;
        waveDef.currentEnemyCount = currentEnemyCount;
        if (currentEnemyCount < 0)
        {
            currentEnemyCount = 0;
        }
    }
}
