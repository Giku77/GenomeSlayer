using System;
using System.Collections.Generic;
using UnityEngine;

public class StateManager : MonoBehaviour
{
    [SerializeField] private StateDef def;
    private Dictionary<int, int> levels = new Dictionary<int, int>();

    public int GenomePoint { get; private set; }

    public int RowCount => def != null ? def.rows.Count : 0;
    public StateDef.Row GetRow(int index) => def.rows[index];

    public event Action<int, int> OnLevelChanged;
    public event Action<int> OnGenomePointChanged;

    public enum LevelUpResult
    {
        Ok,
        NotEnoughPoint,
        ReachedMaxLevel,
        InvalidIndex
    }

    public StateSaveData ExportSave()
    {
        var dict = levels != null ? new Dictionary<int, int>(levels)
                                  : new Dictionary<int, int>();

        // for (int i = 0; i < RowCount; i++)
        // {
        //     var row = def.rows[i];
        //     int lv = GetLevel(row.id);
        //     dict[row.id] = lv;
        // }

        return StateSaveData.FromDict(GenomePoint, dict);
    }

    public void ApplySave(StateSaveData save, bool suppressEvents = true)
    {
        if (save == null) return;

        var gpHandlers = OnGenomePointChanged;
        var lvHandlers = OnLevelChanged;
        if (suppressEvents)
        {
            OnGenomePointChanged = null;
            OnLevelChanged = null;
        }

        try
        {
            levels = save.ToDict();               
            GenomePoint = save.genomePoint;      
            if (def != null && levels != null)
            {
                var tmp = new Dictionary<int, int>();
                foreach (var row in def.rows)
                {
                    int storedLv = 0;
                    if (levels.TryGetValue(row.id, out var lv))
                        storedLv = Mathf.Clamp(lv, 0, def.MaxLevelFor(row.id));
                    else
                        storedLv = row.defaultLv; 
                    tmp[row.id] = storedLv;
                }
                levels = tmp;
            }

            if (!suppressEvents)
            {
                OnGenomePointChanged?.Invoke(GenomePoint);
                if (levels != null)
                    foreach (var kv in levels) OnLevelChanged?.Invoke(kv.Key, kv.Value);
            }
        }
        finally
        {
            if (suppressEvents)
            {
                OnGenomePointChanged = gpHandlers;
                OnLevelChanged = lvHandlers;
                OnGenomePointChanged?.Invoke(GenomePoint);
                if (levels != null)
                    foreach (var kv in levels) OnLevelChanged?.Invoke(kv.Key, kv.Value);
            }
        }
    }

    void Awake()
    {
        GenomePoint = def.GenomePoint;
        OnGenomePointChanged?.Invoke(GenomePoint);
        //levels = new Dictionary<int, int>();
        // TODO: JSON 로드 연결
    }

    public LevelUpResult TryLevelUpByIndexResult(int index, int? overrideCost = null)
    {
        if (index < 0 || index >= RowCount) return LevelUpResult.InvalidIndex;

        var row = def.rows[index];
        int curLv = GetLevel(row.id);
        int maxLv = def.MaxLevelFor(row.id);
        if (curLv >= maxLv) return LevelUpResult.ReachedMaxLevel;

        int cost = overrideCost ?? GetNextCostByIndex(index);
        if (GenomePoint < cost) return LevelUpResult.NotEnoughPoint;

        GenomePoint -= cost;
        int newLv = curLv + 1;
        levels[row.id] = newLv;

        OnGenomePointChanged?.Invoke(GenomePoint);
        OnLevelChanged?.Invoke(row.id, newLv);
        // SaveJson(); 필요 시
        return LevelUpResult.Ok;
    }

    public int GetLevel(int id)
    {
        return levels != null && levels.TryGetValue(id, out var lv) ? lv
             : (def.TryGet(id, out var row) ? row.defaultLv : 0);
    }

    public int GetLevelByIndex(int index)
    {
        var id = def.rows[index].id;
        return GetLevel(id);
    }

    public int GetNextCostByIndex(int index)
    {
        var row = def.rows[index];
        int curLv = GetLevel(row.id);
        int nextLv = curLv + 1;
        return def.GetCost(row.id, nextLv);
    }

    public bool TryLevelUpByIndex(int index, int? overrideCost = null)
    {
        var row = def.rows[index];

        // 레벨 상한
        int maxLv = def.MaxLevelFor(row.id);
        int curLv = GetLevel(row.id);
        if (curLv >= maxLv) return false;

        int cost = overrideCost ?? GetNextCostByIndex(index);
        if (GenomePoint < cost) return false;

        GenomePoint -= cost;
        int newLv = curLv + 1;
        levels[row.id] = newLv;

        OnGenomePointChanged?.Invoke(GenomePoint);
        OnLevelChanged?.Invoke(row.id, newLv);
        // TODO: SaveJson()
        return true;
    }

    public void AddGenomePoint(int amount)
    {
        GenomePoint += amount;
        OnGenomePointChanged?.Invoke(GenomePoint);
    }

    public float GetUpgradeStatAmount(int id)
    {
        var a = DataTableManger.GeTable.GetItem(id).upgradeStatAmount;
        var lv = GetLevel(id);
        return a * lv;
    }

}
