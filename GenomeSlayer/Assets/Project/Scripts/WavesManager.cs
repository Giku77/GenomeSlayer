using JetBrains.Annotations;
using NUnit.Framework;
using System.Collections;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.EventSystems;

public class WavesManager : MonoBehaviour
{
    public Transform spawnPoint1;

    public WavesDef[] waveDef;
    public GameObject[] enemyPrefabs;

    public readonly static int maxEnemyCount = 200;

    private int currentEnemyCount = 0;
    private float waveInterval;
    private int currentWave;

    private int spawnEnemyCount = 0;

    //public GameObject enemyPrefab;

    private Coroutine waveCoroutine;
    private List<Coroutine> returnCoroutine = new List<Coroutine>();
    private EnemyData currentEnemyData;

    private Coroutine[] waveSpawns;

    public UIManager uiManager;

    public Player player;

    private bool bossAlive;
    private bool bossSpawned = false;
    private float bossSpawnTime;
    private bool isBossWave {
        get 
        { 
            for (int i = 0; i < waveDef[currentWave].spawns.Count; i++)
            {
                if (waveDef[currentWave].spawns[i].isBoss)
                {
                    bossSpawnTime = waveDef[currentWave].spawns[i].startTime;
                    return true;
                }
            }
            return false;
        } 
    }

    private bool waveInProgress = false;


    public bool waveDone => waveInProgress;

    //private Queue<GameObject> poolEnemies = new Queue<GameObject>();
    private Dictionary<int, Queue<GameObject>> pools = new Dictionary<int, Queue<GameObject>>();
    private List<GameObject> activeEnemies = new List<GameObject>();

    private void SpawnMonsters()
    {
        if (waveSpawns != null)
            foreach (var c in waveSpawns) if (c != null) StopCoroutine(c);

        var defs = waveDef[currentWave];
        float waveDuration = defs.waveDuration;

        waveSpawns = new Coroutine[defs.spawns.Count];
        for (int i = 0; i < defs.spawns.Count; i++)
        {
            var rule = defs.spawns[i];

            if (rule.period <= 0f)
            {
                waveSpawns[i] = StartCoroutine(SpawnOneShot(rule, waveDuration));
            }
            else
            {
                waveSpawns[i] = StartCoroutine(SpawnRoutine(rule, waveDuration));
            }
        }
    }

    private IEnumerator SpawnRoutine(SpawnRule rule, float waveDuration)
    {
        float firstDelay = Mathf.Max(0f, waveDuration - rule.startTime);
        yield return new WaitForSeconds(firstDelay);

        while (waveInProgress)
        {
            for (int j = 0; j < rule.amount; j++)
            {
                if (currentEnemyCount >= maxEnemyCount) break;
                if (TryFindSpawnPoint(player.transform.position, out var spawnPos))
                {
                    var e = GetEnemy(spawnPos, Quaternion.identity, (int)rule.enemyData.enemyId);
                    if (e) currentEnemyCount++;
                }
            }
            yield return new WaitForSeconds(rule.period);
        }
    }

    void Awake()
    {
        Application.targetFrameRate = -1;
        EventBus.EnemyDied += OnEnemyDefeated;

        //foreach (var e in enemyPrefabs)
        for(int i = 0; i < enemyPrefabs.Length; i++)
        {
            var q = new Queue<GameObject>();
            var id = (int)enemyPrefabs[i].GetComponent<Enemy>().enemyData.enemyId;
            pools[id] = q;
            int prewarm = maxEnemyCount / Mathf.Max(1, enemyPrefabs.Length);
            for (int ii = 0; ii < prewarm; ii++)
            {
                var go = Instantiate(enemyPrefabs[i]);

                go.SetActive(false);
                q.Enqueue(go);
            }
        }
    }


    public GameObject GetEnemy(Vector3 pos, Quaternion rot, int id)
    {
        if (pools[id].Count == 0)
        {
            var index = 0;
            for (int i = 0; i < enemyPrefabs.Length; i++) 
            {
                var eid = (int)enemyPrefabs[i].GetComponent<Enemy>().enemyData.enemyId;
                if (eid == id)
                {
                    index = i;
                    break;
                }
            }
            GameObject e = Instantiate(enemyPrefabs[index]);
            e.SetActive(false);
            pools[id].Enqueue(e);
        }
        var enemy = pools[id].Dequeue();
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
        var id = (int)es.enemyData.enemyId;
        pools[id].Enqueue(enemy);
    }

    private void Start()
    {
        currentEnemyCount = 0;
        waveInterval = waveDef[currentWave].waveDuration;
        uiManager.UpdateWave(waveDef[currentWave].chapterNum);
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
        if (waveSpawns != null)
        {
            foreach (var c in waveSpawns)
            {
                if (c != null) StopCoroutine(c);
            }
        }
        EventBus.EnemyDied -= OnEnemyDefeated;
    }


    private IEnumerator WaveTimer()
    {
        while (waveInterval >= -1)
        {
            if (waveInProgress)
            {
                uiManager.UpdateWaveTimer(waveInterval);
                waveInterval--;

                //for(int i = 0; i < spawnEnemyCount; i++)
                //{
                //    if (currentEnemyCount < maxEnemyCount)
                //    {
                //        if (TryFindSpawnPoint(player.transform.position, out var spawnPos))
                //        {
                //            var e = GetEnemy(spawnPos, Quaternion.identity);
                //            //EventBus.RemoveObj.Add(e);
                //            currentEnemyCount++;
                //        }
                //    }
                //    else break;
                //}
            }
            yield return new WaitForSeconds(1f);
        }
    }

    private void Update()
    {
        if ((waveInterval <= -1) && waveInProgress)
        {
            //Debug.Log("Spawning Wave " + (waveDef.currentWave + 1));
            bossSpawned = false;
            ResetWaves();
            uiManager.ActiveWaveButton(true);
            uiManager.ActiveGenomButton(true);
            waveInProgress = false;
            //SpawnWave();
        }
        //if (!bossSpawned && isBossWave && waveInterval <= bossSpawnTime && waveInProgress)
        //{
        //    SpawnBoss();
        //    bossSpawned = true;
        //}
    }

    //public void SpawnBoss()
    //{
    //    if (bossEnemy == null || bossAlive) return;

    //    if (TryFindSpawnPoint(player.transform.position, out var spawnPos))
    //    {
    //        var agent = bossEnemy.GetComponent<UnityEngine.AI.NavMeshAgent>();
    //        if (agent && agent.isOnNavMesh) agent.Warp(spawnPos);
    //        else bossEnemy.transform.SetPositionAndRotation(spawnPos, Quaternion.identity);

    //        var enemy = bossEnemy.GetComponent<Enemy>();
    //        if (enemy != null)
    //        {
    //            enemy.ResetEnemy();                // 너가 Enemy에 넣은 초기화 함수 재활용
    //                                               // 필요하면 enemy.SetEnemyData(bossEnemyData);   // SO로 보스 전용 데이터 주입
    //                                               // enemy.SetBoss(true); // isBoss 플래그가 필요하면 Enemy에 추가
    //        }

    //        bossEnemy.SetActive(true);
    //        bossAlive = true;
    //    }
    //}

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
        player.Heal((int)player.maxHeal);
        //var weapon = DataTableManger.EquipmentTable.GetItem((int)WeaponIds.Watermelon_Armor);
        //player.quickSlotInventory.TryAddItem((int)WeaponIds.Watermelon_Armor, 1, weapon.equipDurability, weapon.equipQuantity);
        //player.quickSlotInventory.TryAddItem((int)WeaponIds.Katana_Pepper, 1, weapon.equipDurability, weapon.equipQuantity);

        ResetWaves();
        waveInProgress = true;
        waveInterval = waveDef[currentWave].waveDuration;
        waveCoroutine = StartCoroutine(WaveTimer());
        SpawnMonsters();
        uiManager.ActiveWaveButton(false);
        uiManager.ActiveGenomButton(false);
        uiManager.UpdateWave(waveDef[currentWave].chapterNum);
        currentWave++;
        if (currentWave >= waveDef.Length) currentWave = 0; // 엔딩 관련
        //spawnEnemyCount = currentWave * 2;

        //for (int i = 0; i < currentEnemyCount; i++)
        //{
        //    var e = Instantiate(enemyPrefab, spawnPoint1.position, Quaternion.identity);
        //    EventBus.RemoveObj.Add(e);
        //}
    }

    //private IEnumerator ReturnBossWait(float s)
    //{
    //    yield return new WaitForSeconds(s);

    //    var enemy = bossEnemy.GetComponent<Enemy>();
    //    if (enemy != null) enemy.ResetEnemy();

    //    bossEnemy.SetActive(false);
    //    bossAlive = false;

    //    // 보스는 currentEnemyCount에 포함하지 않았다면 아무 것도 안 해도 됨
    //}

    public void OnEnemyDefeated(GameObject e)
    {
        //if (bossEnemy != null && ReferenceEquals(e, bossEnemy))
        //{
        //    StartCoroutine(ReturnBossWait(3f));
        //}
        //else
        //{
            returnCoroutine.Add(StartCoroutine(ReturnEnmeyWait(e, 3f)));
        //}
    }

    private IEnumerator ReturnEnmeyWait(GameObject e, float s)
    {
        yield return new WaitForSeconds(s);
        currentEnemyCount--;
        if (currentEnemyCount < 0)
        {
            currentEnemyCount = 0;
        }
        ReturnEnemy(e);
    }

    private IEnumerator SpawnOneShot(SpawnRule rule, float waveDuration)
    {
        float delay = Mathf.Max(0f, waveDuration - rule.startTime);
        yield return new WaitForSeconds(delay);

        if (!waveInProgress) yield break;

        for (int j = 0; j < rule.amount; j++)
        {
            if (currentEnemyCount >= maxEnemyCount) break;

            if (TryFindSpawnPoint(player.transform.position, out var spawnPos))
            {
                var go = GetEnemy(spawnPos, Quaternion.identity, (int)rule.enemyData.enemyId);
                if (go) currentEnemyCount++;
            }
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
                //EventBus.UpdateSlot?.Invoke(i, string.Empty, string.Empty, -1);
            }
        }
        foreach (var enemy in activeEnemies.ToArray())
        {
            ReturnEnemy(enemy);
        }
        currentEnemyCount = 0;

        //if (bossEnemy != null)
        //{
        //    var enemy = bossEnemy.GetComponent<Enemy>();
        //    if (enemy != null) enemy.ResetEnemy();
        //    bossEnemy.SetActive(false);
        //    bossAlive = false;
        //}

        var equip = player.GetComponent<EquipItem>();
        equip.UnEquipItem();
        uiManager.ActiveFalseSlider();
    }
}
