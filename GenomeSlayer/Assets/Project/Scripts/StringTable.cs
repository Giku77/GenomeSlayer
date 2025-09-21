using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class StringData
{
    public int toolTipID { get; set; }
    public string toolTipText { get; set; }
    public int nextToolTip { get; set; }
    public int toolTipPos { get; set; }
    public int arrow1Pos { get; set; }
    public int arrow2Pos { get; set; }
}

public class StringTable : DataTable
{
    public static readonly string StrTableId = "StringsTable";
    //private static readonly string UnknownItemKey = "UNKNOWN_ITEM";

    private Dictionary<int, StringData> _strs = new Dictionary<int, StringData>();

    public override void Load(string fileName)
    {
        _strs.Clear();
        var path = string.Format(dataTablePath, fileName);
        //var path = dataTablePath + fileName;
        var textAsset = Resources.Load<TextAsset>(path);
        if (textAsset == null)
        {
            Debug.LogError($"Failed to load string table: {fileName} at path: {path}");
            return;
        }
        var records = LoadCSV<StringData>(textAsset.text);
        if (records == null || records.Count == 0)
        {
            Debug.LogWarning($"No records found in string table: {fileName}");
            return;
        }
        _strs = records.ToDictionary(r => r.toolTipID, r => r);

    }

    public StringData GetItem(int key)
    {
        if (_strs.TryGetValue(key, out var value))
        {
            return value;
        }
        Debug.LogWarning($"Item key not found: {key}");
        return null;
        //return (UnknownItemKey, -1, -1, UnknownItemKey);
    }

}
