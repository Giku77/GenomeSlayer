using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "StateDef", menuName = "Scriptable Objects/StateDef")]
public class StateDef : ScriptableObject
{

    public int GenomePoint;

    [System.Serializable]
    public class Row
    {
        public int id;
        public int defaultLv;

        public List<int> overrideCostCurve = new();
    }

    public List<int> sharedCostCurve = new() { 100, 200, 400, 800, 1600 };

    public List<Row> rows = new List<Row>();

    private Dictionary<int, Row> map;
    void OnEnable()
    {
        map = new Dictionary<int, Row>(rows.Count);
        foreach (var r in rows) map[r.id] = r;
    }

    public bool TryGet(int id, out Row row) => map.TryGetValue(id, out row);

    public int GetCost(int id, int nextLevel)
    {
        if (!TryGet(id, out var row)) return int.MaxValue;

        var curve = (row.overrideCostCurve != null && row.overrideCostCurve.Count > 0)
            ? row.overrideCostCurve
            : sharedCostCurve;

        if (curve == null || curve.Count == 0) return 0; 
      
        int idx = Mathf.Clamp(nextLevel - 1, 0, curve.Count - 1);
        return curve[idx];
    }

    public int MaxLevelFor(int id)
    {
        if (!TryGet(id, out var row)) return sharedCostCurve?.Count ?? 0;
        int len = (row.overrideCostCurve != null && row.overrideCostCurve.Count > 0)
            ? row.overrideCostCurve.Count
            : (sharedCostCurve?.Count ?? 0);
        return Mathf.Max(len, row.defaultLv);
    }
}
