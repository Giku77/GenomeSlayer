// Assets/Editor/ScriptableObjectCreator.cs
#if UNITY_EDITOR
using UnityEditor;
using UnityEngine;
using System.IO;

public static class ScriptableObjectCreator
{
    /// <summary>
    /// 폴더를 자동 생성하고, 중복 이름이면 고유 경로로 저장해줌.
    /// folderPath 예: "Assets/GameData/Seeds"
    /// fileName  예: "Seed_Apple"
    /// </summary>
    public static T CreateAssetAt<T>(string folderPath, string fileName) where T : ScriptableObject
    {
        // 폴더 보장
        EnsureFolders(folderPath);

        var asset = ScriptableObject.CreateInstance<T>();

        string rawPath = Path.Combine(folderPath, fileName + ".asset").Replace("\\", "/");
        string uniquePath = AssetDatabase.GenerateUniqueAssetPath(rawPath);

        AssetDatabase.CreateAsset(asset, uniquePath);
        EditorUtility.SetDirty(asset);
        AssetDatabase.SaveAssets();
        AssetDatabase.Refresh();

        Selection.activeObject = asset; // 방금 만든 에셋 선택
        Debug.Log($"Created: {uniquePath}");
        return asset;
    }

    /// <summary>
    /// "Assets/A/B/C" 형태의 경로에 필요한 폴더를 순차적으로 생성
    /// </summary>
    static void EnsureFolders(string fullPath)
    {
        fullPath = fullPath.Replace("\\", "/");
        if (!fullPath.StartsWith("Assets"))
            throw new System.ArgumentException("folderPath는 'Assets'로 시작해야 합니다.");

        string[] parts = fullPath.Split('/');
        string cur = parts[0]; // "Assets"
        for (int i = 1; i < parts.Length; i++)
        {
            string next = $"{cur}/{parts[i]}";
            if (!AssetDatabase.IsValidFolder(next))
            {
                AssetDatabase.CreateFolder(cur, parts[i]);
            }
            cur = next;
        }
    }

    /// <summary>
    /// 저장 다이얼로그를 띄워 사용자가 직접 경로/이름 선택
    /// </summary>
    public static T CreateWithSaveDialog<T>(string defaultName = "NewAsset") where T : ScriptableObject
    {
        string path = EditorUtility.SaveFilePanelInProject("Save ScriptableObject", defaultName, "asset", "");
        if (string.IsNullOrEmpty(path)) return null;

        var asset = ScriptableObject.CreateInstance<T>();
        AssetDatabase.CreateAsset(asset, path);
        EditorUtility.SetDirty(asset);
        AssetDatabase.SaveAssets();
        AssetDatabase.Refresh();
        Selection.activeObject = asset;
        Debug.Log($"Created: {path}");
        return asset;
    }
}
#endif
