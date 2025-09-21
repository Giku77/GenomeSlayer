using System.Collections.Generic;

[System.Serializable]
public class StateSaveData
{
    public int genomePoint;
    public List<Entry> entries = new();
    [System.Serializable] public class Entry { public int id; public int lv; }

    public Dictionary<int, int> ToDict()
    {
        var d = new Dictionary<int, int>(entries.Count);
        foreach (var e in entries) d[e.id] = e.lv;
        return d;
    }
    public static StateSaveData FromDict(int genomePoint, Dictionary<int, int> dict)
    {
        var s = new StateSaveData { genomePoint = genomePoint };
        foreach (var kv in dict) s.entries.Add(new Entry { id = kv.Key, lv = kv.Value });
        return s;
    }
}
