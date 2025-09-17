using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class GesPointData
{
    public int UpgradeId { get; set; }
    public string upgradeName { get; set; }
    public int itemId { get; set; }
    public int upgradeStat { get; set; }
    public float upgradeStatAmount { get; set; }
    public int maxUpgrade { get; set; }
    public int genomePoint1 { get; set; }
    public int genomePoint2 { get; set; }
    public int genomePoint3 { get; set; }
    public int genomePoint4 { get; set; }
    public int genomePoint5 { get; set; }
}

public class GeTable : DataTable
{
    public static readonly string GesTableId = "GesTable";
    //private static readonly string UnknownItemKey = "UNKNOWN_ITEM";

    private Dictionary<int, GesPointData> _ges = new Dictionary<int, GesPointData>();

    public override void Load(string fileName)
    {
        _ges.Clear();
        var path = string.Format(dataTablePath, fileName);
        //var path = dataTablePath + fileName;
        var textAsset = Resources.Load<TextAsset>(path);
        if (textAsset == null)
        {
            Debug.LogError($"Failed to load string table: {fileName} at path: {path}");
            return;
        }
        var records = LoadCSV<GesPointData>(textAsset.text);
        if (records == null || records.Count == 0)
        {
            Debug.LogWarning($"No records found in string table: {fileName}");
            return;
        }
        _ges = records.ToDictionary(r => r.UpgradeId, r => r);

    }

    public GesPointData GetItem(int key)
    {
        if (_ges.TryGetValue(key, out var value))
        {
            return value;
        }
        Debug.LogWarning($"Item key not found: {key}");
        return null;
        //return (UnknownItemKey, -1, -1, UnknownItemKey);
    }

    public List<GesPointData> GetAllItems()
    {
        return _ges.Values.ToList();
    }

}
