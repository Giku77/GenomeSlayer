using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class BreedData
{
    public int breedID { get; set; }
    public string breedInfo { get; set; }
    public int breedPlant { get; set; }
    public int plantImprovedNum1 { get; set; }
    public int plantImprovedNum2 { get; set; }
    public int equipID1 { get; set; }
    public int equipID2 { get; set; }
}

public class BreedTable : DataTable
{
    public static readonly string BreedTableId = "BreedsTable";
    //private static readonly string UnknownItemKey = "UNKNOWN_ITEM";

    private Dictionary<int, BreedData> _breeds = new Dictionary<int, BreedData>();

    public override void Load(string fileName)
    {
        _breeds.Clear();
        var path = string.Format(dataTablePath, fileName);
        //var path = dataTablePath + fileName;
        var textAsset = Resources.Load<TextAsset>(path);
        if (textAsset == null)
        {
            Debug.LogError($"Failed to load string table: {fileName} at path: {path}");
            return;
        }
        var records = LoadCSV<BreedData>(textAsset.text);
        if (records == null || records.Count == 0)
        {
            Debug.LogWarning($"No records found in string table: {fileName}");
            return;
        }
        _breeds = records.ToDictionary(r => r.breedID, r => r);

    }

    public BreedData GetItem(int key)
    {
        if (_breeds.TryGetValue(key, out var value))
        {
            return value;
        }
        Debug.LogWarning($"Item key not found: {key}");
        return null;
        //return (UnknownItemKey, -1, -1, UnknownItemKey);
    }

}
