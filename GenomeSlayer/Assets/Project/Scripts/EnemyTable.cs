using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class EnemyTableData
{
    public int monsterId { get; set; }
    public string monsterName { get; set; }
    public int monsterType { get; set; }
    public float monsterHp { get; set; }
    public float monsterAttack { get; set; }
    public float monsterAttackSpeed { get; set; }
    public float monsterMoveSpeed { get; set; }
}

public class EnemyTable : DataTable
{
    public static readonly string EnemyTableId = "EnemysTable";
    //private static readonly string UnknownItemKey = "UNKNOWN_ITEM";

    private Dictionary<int, EnemyTableData> _enemys = new Dictionary<int, EnemyTableData>();

    public override void Load(string fileName)
    {
        _enemys.Clear();
        var path = string.Format(dataTablePath, fileName);
        //var path = dataTablePath + fileName;
        var textAsset = Resources.Load<TextAsset>(path);
        if (textAsset == null)
        {
            Debug.LogError($"Failed to load string table: {fileName} at path: {path}");
            return;
        }
        var records = LoadCSV<EnemyTableData>(textAsset.text);
        if (records == null || records.Count == 0)
        {
            Debug.LogWarning($"No records found in string table: {fileName}");
            return;
        }
        _enemys = records.ToDictionary(r => r.monsterId, r => r);

    }

    public EnemyTableData GetItem(int key)
    {
        if (_enemys.TryGetValue(key, out var value))
        {
            return value;
        }
        Debug.LogWarning($"Item key not found: {key}");
        return null;
        //return (UnknownItemKey, -1, -1, UnknownItemKey);
    }

}
