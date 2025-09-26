using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "Waves_XX", menuName = "Scriptable Objects/WavesDef")]
public class WavesDef : ScriptableObject
{
    [Header("Identity")]
    public int waveID;          
    public int chapterNum;        

    [Header("Timeline")]
    [Min(1)] public float waveDuration = 180f; 

    [Header("Spawns")]
    public List<SpawnRule> spawns = new();

    // 특정 시점까지의 총 스폰 예상치(디버그/밸런싱용)
    public int EstimateTotalSpawns()
    {
        int total = 0;
        foreach (var s in spawns)
        {
            if (s.period <= 0f)
            {
                total += s.amount; // 1회성
            }
            else
            {
                float len = Mathf.Max(0f, waveDuration - s.startTime);
                if (len >= 0f)
                {
                    int ticks = 1 + Mathf.FloorToInt(len / s.period);
                    total += ticks * Mathf.Max(0, s.amount);
                }
            }
        }
        return total;
    }

#if UNITY_EDITOR
    private void OnValidate()
    {
        // 보스 규칙은 보통 1회성이므로 자동 보정(원치 않으면 주석 처리)
        foreach (var s in spawns)
        {
            if (s.isBoss && s.period != 0f) s.period = 0f;
            if (s.amount < 1) s.amount = 1;
        }
    }
#endif
}
