using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class PlantData
{
    public int plantId { get; set; }
    public string plantName { get; set; }
    public int requireWater { get; set; }
    public int requireFertilizer { get; set; }
    public int adultPlant { get; set; }
    public float plantExpireTime { get; set; }
    public int plantBuffId { get; set; }
    public int plantItemId { get; set; }
}

public class PlantTable : DataTable
{
    public static readonly string BuffTableId = "PlantsTable";
    //private static readonly string UnknownItemKey = "UNKNOWN_ITEM";

    private Dictionary<int, PlantData> _plants = new Dictionary<int, PlantData>();

    public override void Load(string fileName)
    {
        _plants.Clear();
        var path = string.Format(dataTablePath, fileName);
        //var path = dataTablePath + fileName;
        var textAsset = Resources.Load<TextAsset>(path);
        if (textAsset == null)
        {
            Debug.LogError($"Failed to load string table: {fileName} at path: {path}");
            return;
        }
        var records = LoadCSV<PlantData>(textAsset.text);
        if (records == null || records.Count == 0)
        {
            Debug.LogWarning($"No records found in string table: {fileName}");
            return;
        }
        _plants = records.ToDictionary(r => r.plantId, r => r);

    }

    public PlantData GetItem(int key)
    {
        if (_plants.TryGetValue(key, out var value))
        {
            return value;
        }
        Debug.LogWarning($"Item key not found: {key}");
        return null;
        //return (UnknownItemKey, -1, -1, UnknownItemKey);
    }

}
