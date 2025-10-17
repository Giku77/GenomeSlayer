using Newtonsoft.Json;
using System;
using System.IO;
using UnityEngine;

public static class SaveService
{
    static readonly string Dir = Application.persistentDataPath;
    static readonly string FileName = "savegame.json"; 
    static readonly string BackupName = "savegame.bak";

    static readonly JsonSerializerSettings JsonSettings = new()
    {
        Formatting = Formatting.None,
        NullValueHandling = NullValueHandling.Ignore,
    };

    static string PathMain => Path.Combine(Dir, FileName);
    static string PathBackup => Path.Combine(Dir, BackupName);

    public static void Save<T>(T data)
    {
        try
        {
            if (!Directory.Exists(Dir)) Directory.CreateDirectory(Dir);

            // 1) JSON 직렬화 → 2) 암호화(Base64 문자열)
            string b64 = SecureSave.EncryptJsonToBase64(data, compress: true);
            string tmpPath = PathMain + ".tmp";

            File.WriteAllText(tmpPath, b64);

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
                var text = File.ReadAllText(PathMain);

                // 암호화 판별 → 해독 시도
                if (SecureSave.LooksLikeEncrypted(text))
                {
                    if (SecureSave.TryDecryptBase64ToJson<T>(text, out var dec, compressed: true))
                    {
                        data = dec; return true;
                    }
                    // 암호화로 보이는데 실패 → 백업 시도
                }
                else
                {
                    // (구버전) 플레인 JSON → 한번 로드해서 곧바로 암호화 재저장(마이그레이션)
                    var plain = JsonConvert.DeserializeObject<T>(text, JsonSettings);
                    if (plain != null)
                    {
                        data = plain;
                        Save(data); // 즉시 암호화 저장
                        return true;
                    }
                }
            }

            if (File.Exists(PathBackup))
            {
                var btxt = File.ReadAllText(PathBackup);
                if (SecureSave.LooksLikeEncrypted(btxt))
                {
                    if (SecureSave.TryDecryptBase64ToJson<T>(btxt, out var dec, compressed: true))
                    {
                        data = dec;
                        Save(data);
                        return true;
                    }
                }
                else
                {
                    var plain = JsonConvert.DeserializeObject<T>(btxt, JsonSettings);
                    if (plain != null)
                    {
                        data = plain;
                        Save(data);
                        return true;
                    }
                }
            }
        }
        catch (Exception e)
        {
            Debug.LogWarning($"Load failed, trying backup. Err={e}");
            try
            {
                if (File.Exists(PathBackup))
                {
                    var btxt = File.ReadAllText(PathBackup);
                    if (SecureSave.LooksLikeEncrypted(btxt))
                    {
                        if (SecureSave.TryDecryptBase64ToJson<T>(btxt, out var dec, compressed: true))
                        {
                            data = dec; Save(data); return true;
                        }
                    }
                    else
                    {
                        var plain = JsonConvert.DeserializeObject<T>(btxt, JsonSettings);
                        if (plain != null) { data = plain; Save(data); return true; }
                    }
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
