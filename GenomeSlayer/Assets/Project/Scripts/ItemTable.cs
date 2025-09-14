using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class ItemData
{
    public int itemId { get; set; }
    public string itemName { get; set; }
    public string itemInfo { get; set; }
    public int itemType { get; set; }
    public int itemAttribute { get; set; }
    public int itemQuantity { get; set; }
    public int itemDurability { get; set; }
    public float itemDamage { get; set; }
    public float itemAttackSpeed { get; set; }
    public float itemArmor { get; set; }
    public float itemBarrier { get; set; }
    public float itemHeal { get; set; }
    public int breedStat1 { get; set; }
    public float breedStatAmount1 { get; set; }
    public int breedStat2 { get; set; }
    public float breedStatAmount2 { get; set; }
}

public class ItemTable : DataTable
{
    public static readonly string ItemTableId = "ItemsTable";
    //private static readonly string UnknownItemKey = "UNKNOWN_ITEM";

    private Dictionary<int, ItemData> _items = new Dictionary<int, ItemData>();
    //private readonly Dictionary<int, (string, int, int, string)> _items = new Dictionary<int, (string, int, int, string)>();

    public override void Load(string fileName)
    {
        _items.Clear();
        var path = string.Format(dataTablePath, fileName);
        //var path = dataTablePath + fileName;
        var textAsset = Resources.Load<TextAsset>(path);
        if (textAsset == null)
        {
            Debug.LogError($"Failed to load string table: {fileName} at path: {path}");
            return;
        }
        var records = LoadCSV<ItemData>(textAsset.text);
        if (records == null || records.Count == 0)
        {
            Debug.LogWarning($"No records found in string table: {fileName}");
            return;
        }
        _items = records.ToDictionary(r => r.itemId, r => r);
        //foreach (var record in records)
        //{
        //    if (!_items.ContainsKey(record.itemId))
        //    {
        //        //_items.Add(record.itemId, (record.Name, record.Value, record.Price, record.Des));
        //        //_items.Add(record.itemId, (record.itemName, record.itemType, record.itemAttribute, record.itemInfo));
        //    }
        //    else
        //    {
        //        Debug.LogWarning($"Duplicate key found in string table: {record.itemId}");
        //    }
        //}
    }

    public ItemData GetItem(int key)
    {
        if (_items.TryGetValue(key, out var value))
        {
            return value;
        }
        Debug.LogWarning($"Item key not found: {key}");
        return null;
        //return (UnknownItemKey, -1, -1, UnknownItemKey);
    }

}
