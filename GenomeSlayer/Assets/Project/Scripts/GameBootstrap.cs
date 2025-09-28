using UnityEngine;

public class GameBootstrap : MonoBehaviour
{
    public SettingsManager settings;
    public StateManager stateManager;
    public WavesManager wavesManager;

    public bool skipAutoSaveOnce;

    private void Awake()
    {
        if (SettingsService.TryLoad(out var set))
            settings.ApplySave(set);
        else
            settings.ApplyRuntime();

        if (SaveService.TryLoad<SaveRoot>(out var save))
        {
            stateManager.ApplySave(save.state, suppressEvents: true);
            wavesManager.ApplySave(save.waves);
        }
        else
        {
            // 첫 실행: 기본값으로 플레이 시작
            //wavesManager.ApplySave(null);
        }
    }

    public void SaveNow()
    {
        if (skipAutoSaveOnce) { skipAutoSaveOnce = false; return; }
        SettingsService.Save(settings.ExportSave());
        var root = new SaveRoot
        {
            version = 1,
            savedAtIso = System.DateTimeOffset.Now.ToString("o"),
            state = stateManager.ExportSave(),
            waves = wavesManager.ExportSave(),
        };
        SaveService.Save(root);
        Debug.Log("Saved.");
    }

    // 앱 나갔다 들어올 때 자동 세이브 (모바일 권장)
    private void OnApplicationPause(bool pause)
    {
        if (pause) SaveNow();
    }

    private void OnApplicationFocus(bool focus)
    {
        if (!focus) SaveNow();
    }

    private void OnApplicationQuit()
    {
        SaveNow();
    }
    public void OnSensitivityChanged(float v)
    {
        settings.lookSensitivity = v;
        settings.ApplyRuntime();
    }
}
