using System.Collections;
using System.Runtime.InteropServices;
using UnityEngine;
using UnityEngine.EventSystems;

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

    public bool waveDone => waveInProgress;

    private void Awake()
    {
        EventBus.EnemyDied += OnEnemyDefeated;
    }

    private void Start()
    {
        waveDef.currentEnemyCount = 0;
        waveDef.currentWave = 0;
        waveDef.WaveInterval = 60f;
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
            ResetWaves();
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
        waveDef.WaveInterval = 60f;
        waveCoroutine = StartCoroutine(WaveTimer());
        uiManager.ActiveWaveButton(false);
        waveDef.currentWave++;
        uiManager.UpdateWave(waveDef.currentWave);
        currentEnemyCount = waveDef.maxEnemyCount + (waveDef.currentWave * 2);
        waveDef.currentEnemyCount = currentEnemyCount;
        for (int i = 0; i < currentEnemyCount; i++)
        {
            var e = Instantiate(enemyPrefab, spawnPoint1.position, Quaternion.identity);
            EventBus.RemoveObj.Add(e);
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

    public void ResetWaves()
    {
        foreach (var obj in EventBus.RemoveObj)
        {
            if (obj != null)
                Destroy(obj);
        }
        var inv = player.quickSlotInventory;
        for (int i = 0; i < 7; i++)
        {
            var slot = inv.GetSlot(i);
            if (slot != null && !slot.IsEmpty)
            {
                inv.RemoveItem(i);
                EventBus.UpdateSlot?.Invoke(i, string.Empty, string.Empty);
            }
        }
        var equip = player.GetComponent<EquipItem>();
        equip.UnEquipItem();
    }
}
