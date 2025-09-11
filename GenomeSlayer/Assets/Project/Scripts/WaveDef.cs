using UnityEngine;

[CreateAssetMenu(fileName = "WaveDef", menuName = "Scriptable Objects/WaveDef")]
public class WaveDef : ScriptableObject
{
    public int currentWave = 0;
    //public int totalWaves = 20;
    public float WaveInterval = 30f;
    public int currentEnemyCount = 0;
    public int maxEnemyCount = 10;
}
