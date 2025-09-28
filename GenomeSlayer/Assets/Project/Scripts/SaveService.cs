using Newtonsoft.Json;
using System;
using System.IO;
using Unity.VisualScripting;
using UnityEngine;

public static class SaveService
{
    static readonly string Dir = Application.persistentDataPath;
    static readonly string FileName = "savegame.json";
    static readonly string BackupName = "savegame.bak";

    static readonly JsonSerializerSettings JsonSettings = new()
    {
        Formatting = Formatting.None,          // 용량 줄이기 (디버그 시 Indented 추천)
        NullValueHandling = NullValueHandling.Ignore,
        // 필요 시: ReferenceLoopHandling = ReferenceLoopHandling.Ignore
        // 다형성 필요 시: TypeNameHandling = TypeNameHandling.Auto (보안 유의)
    };

    static string PathMain => Path.Combine(Dir, FileName);
    static string PathBackup => Path.Combine(Dir, BackupName);

    public static void Save<T>(T data)
    {
        try
        {
            if (!Directory.Exists(Dir)) Directory.CreateDirectory(Dir);

            string json = JsonConvert.SerializeObject(data, JsonSettings);
            string tmpPath = PathMain + ".tmp";

 
            File.WriteAllText(tmpPath, json);

            if (File.Exists(PathMain))
            {
    
                if (File.Exists(PathBackup)) File.Delete(PathBackup);
                File.Move(PathMain, PathBackup);
            }

            File.Move(tmpPath, PathMain);
        }
        catch (Exception e)
        {
            Debug.LogError($"Save failed: {e}");
        }
    }

    public static bool TryLoad<T>(out T data)
    {
        data = default;
        try
        {
            if (File.Exists(PathMain))
            {
                string json = File.ReadAllText(PathMain);
                data = JsonConvert.DeserializeObject<T>(json, JsonSettings);
                return data != null;
            }
  
            if (File.Exists(PathBackup))
            {
                string json = File.ReadAllText(PathBackup);
                data = JsonConvert.DeserializeObject<T>(json, JsonSettings);
                return data != null;
            }
        }
        catch (Exception e)
        {
            Debug.LogWarning($"Load failed, trying backup. Err={e}");
            try
            {
                if (File.Exists(PathBackup))
                {
                    string json = File.ReadAllText(PathBackup);
                    data = JsonConvert.DeserializeObject<T>(json, JsonSettings);
                    return data != null;
                }
            }
            catch (Exception be)
            {
                Debug.LogError($"Backup load failed: {be}");
            }
        }
        return false;
    }
    public static void Delete()
    {
        try
        {
            if (File.Exists(PathMain)) File.Delete(PathMain);
            if (File.Exists(PathBackup)) File.Delete(PathBackup);
        }
        catch (Exception e)
        {
            Debug.LogError($"Settings delete failed: {e}");
        }
    }
}
