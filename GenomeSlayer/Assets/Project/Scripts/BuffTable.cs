using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class BuffData
{
    public int buffId { get; set; }
    public string buffName { get; set; }
    public int buffTo { get; set; }
    public int buffStat { get; set; }
    public float buffStatPercent { get; set; }
    public float buffStatFixed { get; set; }
    public float buffPeriod { get; set; }
    public bool isBuffOverlap { get; set; }
}

public class BuffTable : DataTable
{
    public static readonly string BuffTableId = "BuffsTable";
    //private static readonly string UnknownItemKey = "UNKNOWN_ITEM";

    private Dictionary<int, BuffData> _buffs = new Dictionary<int, BuffData>();

    public override void Load(string fileName)
    {
        _buffs.Clear();
        var path = string.Format(dataTablePath, fileName);
        //var path = dataTablePath + fileName;
        var textAsset = Resources.Load<TextAsset>(path);
        if (textAsset == null)
        {
            Debug.LogError($"Failed to load string table: {fileName} at path: {path}");
            return;
        }
        var records = LoadCSV<BuffData>(textAsset.text);
        if (records == null || records.Count == 0)
        {
            Debug.LogWarning($"No records found in string table: {fileName}");
            return;
        }
        _buffs = records.ToDictionary(r => r.buffId, r => r);

    }

    public BuffData GetItem(int key)
    {
        if (_buffs.TryGetValue(key, out var value))
        {
            return value;
        }
        Debug.LogWarning($"Item key not found: {key}");
        return null;
        //return (UnknownItemKey, -1, -1, UnknownItemKey);
    }

}
