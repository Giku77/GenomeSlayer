using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class EquipItemData
{
    public int equipID { get; set; }
    public string equipName { get; set; }
    public string equipInfo { get; set; }
    public int equipType { get; set; }
    public int equipAttribute { get; set; }
    public int equipQuantity { get; set; }
    public int equipDurability { get; set; }
    public float equipAttack { get; set; }
    public float equipAttackSpeed { get; set; }
    public float equipArmor { get; set; }
    public float equipBarrier { get; set; }
    public int debuffToEnemy { get; set; }
    public int buffToPlayer { get; set; }
    public int equipImprovedNum { get; set; }
    public int improvedSeedID { get; set; }
    public int seedID { get; set; }
    public int regressedSeedID { get; set; }

}

public class EquipmentTable : DataTable
{
    public static readonly string EquipTableId = "EquipmentsTable";
    //private static readonly string UnknownItemKey = "UNKNOWN_ITEM";

    private Dictionary<int, EquipItemData> _equips = new Dictionary<int, EquipItemData>();

    public override void Load(string fileName)
    {
        _equips.Clear();
        var path = string.Format(dataTablePath, fileName);
        //var path = dataTablePath + fileName;
        var textAsset = Resources.Load<TextAsset>(path);
        if (textAsset == null)
        {
            Debug.LogError($"Failed to load string table: {fileName} at path: {path}");
            return;
        }
        var records = LoadCSV<EquipItemData>(textAsset.text);
        if (records == null || records.Count == 0)
        {
            Debug.LogWarning($"No records found in string table: {fileName}");
            return;
        }
        _equips = records.ToDictionary(r => r.equipID, r => r);

    }

    public EquipItemData GetItem(int key)
    {
        if (_equips.TryGetValue(key, out var value))
        {
            return value;
        }
        Debug.LogWarning($"Item key not found: {key}");
        return null;
        //return (UnknownItemKey, -1, -1, UnknownItemKey);
    }

}
