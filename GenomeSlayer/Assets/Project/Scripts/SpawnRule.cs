using UnityEngine;

[System.Serializable]
public class SpawnRule
{
    public EnemyData enemyData;                 

    [Min(0)] public float startTime = 0f;  // monsterSpawnStartTime
    [Min(0)] public float period = 1f;     // monsterSpawnPeriod; 0이면 1회성 스폰으로 취급
    [Min(0)] public int amount = 1;        // monsterSpawnAmount (주기마다 n마리)
    public bool isBoss = false;            // isBoss
}
