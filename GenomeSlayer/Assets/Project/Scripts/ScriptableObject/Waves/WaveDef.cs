using UnityEngine;

[CreateAssetMenu(fileName = "WaveDef", menuName = "Scriptable Objects/WaveDef")]
public class WaveDef : ScriptableObject
{
    public int currentWave = 0;
    public GameObject[] EnemyPrefabs;
    public float WaveInterval = 60f;
    public bool fastSpawn = false;
    public float fastSpawnTime = 20f;   
    public bool isBossWave = false;
    public GameObject bossPrefab;
    public float bossSpawnTime = 40f;
    public int maxEnemyCount = 10;
    public float spawnInterval = 1f;
    public int enemiesPerSpawn = 1;
}
