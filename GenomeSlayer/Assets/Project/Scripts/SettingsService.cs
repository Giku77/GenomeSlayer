using System;
using System.IO;
using Newtonsoft.Json;
using UnityEngine;

public static class SettingsService
{
    static readonly string Dir = Application.persistentDataPath;
    static readonly string FileName = "settings.json";
    static readonly string Backup = "settings.bak";

    static readonly JsonSerializerSettings JsonSettings = new()
    {
        Formatting = Formatting.Indented,
        NullValueHandling = NullValueHandling.Ignore
    };

    static string MainPath => Path.Combine(Dir, FileName);
    static string BackupPath => Path.Combine(Dir, Backup);

    public static void Save(SettingsSaveData s)
    {
        try
        {
            if (!Directory.Exists(Dir)) Directory.CreateDirectory(Dir);
            var json = JsonConvert.SerializeObject(s, JsonSettings);
            var tmp = MainPath + ".tmp";
            File.WriteAllText(tmp, json);
            if (File.Exists(MainPath))
            {
                if (File.Exists(BackupPath)) File.Delete(BackupPath);
                File.Move(MainPath, BackupPath);
            }
            File.Move(tmp, MainPath);
        }
        catch (Exception e)
        {
            Debug.LogError($"Settings save failed: {e}");
        }
    }

    public static bool TryLoad(out SettingsSaveData s)
    {
        s = null;
        try
        {
            if (File.Exists(MainPath))
            {
                s = JsonConvert.DeserializeObject<SettingsSaveData>(File.ReadAllText(MainPath), JsonSettings);
                if (s != null) return true;
            }
            if (File.Exists(BackupPath))
            {
                s = JsonConvert.DeserializeObject<SettingsSaveData>(File.ReadAllText(BackupPath), JsonSettings);
                if (s != null) return true;
            }
        }
        catch (Exception e)
        {
            Debug.LogWarning($"Settings load failed: {e}");
        }
        return false;
    }

    public static void Delete()
    {
        try
        {
            if (File.Exists(MainPath)) File.Delete(MainPath);
            if (File.Exists(BackupPath)) File.Delete(BackupPath);
        }
        catch (Exception e)
        {
            Debug.LogError($"Settings delete failed: {e}");
        }
    }
}
