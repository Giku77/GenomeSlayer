using JetBrains.Annotations;
using NUnit.Framework;
using System.Collections;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using UnityEngine;
using UnityEngine.EventSystems;

public class WaveManager : MonoBehaviour
{
    public Transform spawnPoint1;

    public WaveDef waveDef;

    private int currentEnemyCount = 0;

    private int spawnEnemyCount = 0;

    public GameObject enemyPrefab;

    private Coroutine waveCoroutine;
    private List<Coroutine> returnCoroutine = new List<Coroutine>();

    public UIManager uiManager;

    public Player player;

    private bool waveInProgress = false;

    public bool waveDone => waveInProgress;

    private Queue<GameObject> poolEnemies = new Queue<GameObject>();
    private List<GameObject> activeEnemies = new List<GameObject>();

    private void Awake()
    {
        Application.targetFrameRate = -1;
        EventBus.EnemyDied += OnEnemyDefeated;
        for(int i = 0; i < waveDef.maxEnemyCount; i++)
        {
            var e = Instantiate(enemyPrefab, spawnPoint1.position, Quaternion.identity);
            e.SetActive(false);
            poolEnemies.Enqueue(e);
            //EventBus.RemoveObj.Add(e);
        }
    }

    public GameObject GetEnemy(Vector3 pos, Quaternion rot)
    {
        if (poolEnemies.Count == 0)
        {
            GameObject e = Instantiate(enemyPrefab);
            e.SetActive(false);
            poolEnemies.Enqueue(e);
        }

        var enemy = poolEnemies.Dequeue();
        enemy.transform.SetPositionAndRotation(pos, rot);
        enemy.SetActive(true);
        activeEnemies.Add(enemy);
        return enemy;
    }

    public void ReturnEnemy(GameObject enemy)
    {
        enemy.SetActive(false);
        var es = enemy.GetComponent<Enemy>();
        es.ResetEnemy();
        activeEnemies.Remove(enemy);
        poolEnemies.Enqueue(enemy);
    }

    private void Start()
    {
        waveDef.currentEnemyCount = 0;
        waveDef.currentWave = 0;
        waveDef.WaveInterval = 600f;
        uiManager.UpdateWave(waveDef.currentWave);
    }

    private void OnDisable()
    {
        if (waveCoroutine != null)
        {
            StopCoroutine(waveCoroutine);
        }
        if (returnCoroutine.Count != 0)
        {
            foreach(var c in returnCoroutine)
            {
                StopCoroutine(c);
            }
        }
        EventBus.EnemyDied -= OnEnemyDefeated;
    }


    private IEnumerator WaveTimer()
    {
        while (waveDef.WaveInterval >= -1)
        {
            if (waveInProgress)
            {
                uiManager.UpdateWaveTimer(waveDef.WaveInterval);
                waveDef.WaveInterval--;

                for(int i = 0; i < spawnEnemyCount; i++)
                {
                    if (currentEnemyCount < waveDef.maxEnemyCount)
                    {
                        if (TryFindSpawnPoint(player.transform.position, out var spawnPos))
                        {
                            var e = GetEnemy(spawnPos, Quaternion.identity);
                            //EventBus.RemoveObj.Add(e);
                            currentEnemyCount++;
                            waveDef.currentEnemyCount = currentEnemyCount;
                        }
                    }
                    else break;
                }
            }
            yield return new WaitForSeconds(1f);
        }
    }

    private void Update()
    {
        if ((waveDef.WaveInterval <= -1 || waveDef.currentWave == 0))
        {
            //Debug.Log("Spawning Wave " + (waveDef.currentWave + 1));
            ResetWaves();
            uiManager.ActiveWaveButton(true);
            uiManager.ActiveGenomButton(true);
            waveInProgress = false;
            //SpawnWave();
        }
    }

    Vector3 RandomPointOnRing(Vector3 center, float rMin, float rMax, float y = 0f)
    {
        float r = Random.Range(rMin, rMax);
        float ang = Random.Range(0f, Mathf.PI * 2f);
        return new Vector3(center.x + r * Mathf.Cos(ang), y, center.z + r * Mathf.Sin(ang));
    }

    bool TryFindSpawnPoint(Vector3 playerPos, out Vector3 spawnPos, float rMin = 15f, float rMax = 25f, int attempts = 8)
    {
        for (int i = 0; i < attempts; i++)
        {
            var p = RandomPointOnRing(playerPos, rMin, rMax, playerPos.y);
            if (UnityEngine.AI.NavMesh.SamplePosition(p, out var hit, 3f, UnityEngine.AI.NavMesh.AllAreas))
            {
                var path = new UnityEngine.AI.NavMeshPath();
                if (UnityEngine.AI.NavMesh.CalculatePath(hit.position, playerPos, UnityEngine.AI.NavMesh.AllAreas, path)
                    && path.status == UnityEngine.AI.NavMeshPathStatus.PathComplete)
                {
                    spawnPos = hit.position;
                    return true;
                }
            }
        }
        spawnPos = default;
        return false;
    }


    public void SpawnWave()
    {
        if (waveCoroutine != null)
        {
            StopCoroutine(waveCoroutine);
        }
        player.Heal(1000);
        var weapon = DataTableManger.EquipmentTable.GetItem((int)WeaponIds.Watermelon_Armor);
        player.quickSlotInventory.TryAddItem((int)WeaponIds.Watermelon_Armor, 1, weapon.equipDurability, weapon.equipQuantity);
        waveInProgress = true;
        waveDef.WaveInterval = 600f;
        waveCoroutine = StartCoroutine(WaveTimer());
        uiManager.ActiveWaveButton(false);
        uiManager.ActiveGenomButton(false);
        waveDef.currentWave++;
        uiManager.UpdateWave(waveDef.currentWave);
        spawnEnemyCount = waveDef.currentWave * 2;

        //for (int i = 0; i < currentEnemyCount; i++)
        //{
        //    var e = Instantiate(enemyPrefab, spawnPoint1.position, Quaternion.identity);
        //    EventBus.RemoveObj.Add(e);
        //}
    }

    public void OnEnemyDefeated(GameObject e)
    {
        returnCoroutine.Add(StartCoroutine(ReturnEnmeyWait(e, 3f)));
    }

    private IEnumerator ReturnEnmeyWait(GameObject e, float s)
    {
        yield return new WaitForSeconds(s);
        currentEnemyCount--;
        waveDef.currentEnemyCount = currentEnemyCount;
        if (currentEnemyCount < 0)
        {
            currentEnemyCount = 0;
        }
        ReturnEnemy(e);
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
                //EventBus.UpdateSlot?.Invoke(i, string.Empty, string.Empty, -1);
            }
        }
        foreach (var enemy in activeEnemies.ToArray())
        {
            ReturnEnemy(enemy);
        }
        currentEnemyCount = 0;

        var equip = player.GetComponent<EquipItem>();
        equip.UnEquipItem();
        uiManager.ActiveFalseSlider();
    }
}
