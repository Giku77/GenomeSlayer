using System.Collections;
using UnityEditor.Experimental.GraphView;
using UnityEngine;

public class WaveManager : MonoBehaviour
{
    public Transform spawnPoint1;

    public WaveDef waveDef;

    private int currentEnemyCount = 0;

    public GameObject enemyPrefab;

    private Coroutine waveCoroutine;

    public UIManager uiManager;

    public Player player;

    private bool waveInProgress = false;

    private void Awake()
    {
        EventBus.EnemyDied += OnEnemyDefeated;
    }

    private void Start()
    {
        waveDef.currentEnemyCount = 0;
        waveDef.currentWave = 0;
        waveDef.WaveInterval = 30f;
        uiManager.UpdateWave(waveDef.currentWave);
    }

    private void OnDisable()
    {
        if (waveCoroutine != null)
        {
            StopCoroutine(waveCoroutine);
        }
    }


    private IEnumerator WaveTimer()
    {
        while (waveDef.WaveInterval >= -1)
        {
            if (waveInProgress)
            {
                uiManager.UpdateWaveTimer(waveDef.WaveInterval);
                waveDef.WaveInterval--;
            }
            yield return new WaitForSeconds(1f);
        }
    }

    private void Update()
    {
        //if(currentEnemyCount == 0)
        if (waveDef.WaveInterval <= -1 || waveDef.currentWave == 0)
        {
            //Debug.Log("Spawning Wave " + (waveDef.currentWave + 1));
            uiManager.ActiveWaveButton(true);
            waveInProgress = false;
            //SpawnWave();
        }
    }

    public void SpawnWave()
    {
        if (waveCoroutine != null)
        {
            StopCoroutine(waveCoroutine);
        }
        player.Heal(1000);
        waveInProgress = true;
        waveDef.WaveInterval = 30f;
        waveCoroutine = StartCoroutine(WaveTimer());
        uiManager.ActiveWaveButton(false);
        waveDef.currentWave++;
        uiManager.UpdateWave(waveDef.currentWave);
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
