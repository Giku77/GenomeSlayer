using UnityEngine;

public class SettingsManager : MonoBehaviour
{
    [Header("Live")]
    public int tutorialCompleted = 1;
    public int showFPS;
    public int vibration = 1; // 진동(모바일)
    [Range(0.1f, 1f)] public float lookSensitivity = 0.32f;
    [Range(0.1f, 1f)] public float invertY = 0.1f;
    [Range(0f, 0.4f)] public float deadZone = 0.1f;

    [Header("Audio (optional)")]
    [Range(0f, 1f)] public float bgmVol = 1f;
    [Range(0f, 1f)] public float sfxVol = 1f;

    [Header("Graphics (optional)")]
    public int qualityLevel = 2;


    public void ApplyRuntime()
    {
        var camDrag = FindFirstObjectByType<CamHorizontalDrag>();
        var uiManager = FindFirstObjectByType<UIManager>();
        if (camDrag)
        {
            camDrag.dragSpeedX = lookSensitivity;
            camDrag.dragSpeedY = invertY;
        }
        if (uiManager)
        {
            uiManager.ActiveShowFPS(showFPS == 1);
            uiManager.TypingTextObject.SetActive(tutorialCompleted == 1);
            uiManager.GuidScreen.SetActive(tutorialCompleted == 1);
            if (tutorialCompleted == 1)
            {
                //uiManager.SetTypingID(1601001);
                //uiManager.ShowTypingText();
                uiManager.SettingUI.SetActive(false);
            }
        }

        Haptics.Enabled = (vibration == 1);

        //Debug.Log("bgmVol: " + bgmVol + ", sfxVol: " + sfxVol);
        AudioManager.I.SetBgmVolume(bgmVol);
        AudioManager.I.SetSfxVolume(sfxVol);

        QualitySettings.SetQualityLevel(Mathf.Clamp(qualityLevel, 0, QualitySettings.names.Length - 1));
    }

    public SettingsSaveData ExportSave()
    {
        return new SettingsSaveData
        {
            tutorial = this.tutorialCompleted,
            showFPS= this.showFPS,
            vibration = this.vibration,
            lookSensitivity = this.lookSensitivity,
            deadZone = this.deadZone,
            bgmVol = this.bgmVol,
            sfxVol = this.sfxVol,
            qualityLevel = this.qualityLevel,
        };
    }

    public void ApplySave(SettingsSaveData s)
    {
        if (s == null) return;

        tutorialCompleted = s.tutorial;
        showFPS = s.showFPS;
        vibration = s.vibration;
        lookSensitivity = Mathf.Clamp(s.lookSensitivity, 0.1f, 1f);
        invertY = Mathf.Clamp(s.invertY, 0.1f, 1f);
        deadZone = Mathf.Clamp01(s.deadZone);

        bgmVol = Mathf.Clamp01(s.bgmVol);
        sfxVol = Mathf.Clamp01(s.sfxVol);

        qualityLevel = s.qualityLevel;

        ApplyRuntime();
    }
}
