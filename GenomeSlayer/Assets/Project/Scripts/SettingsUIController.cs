using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class SettingsUIController : MonoBehaviour
{
    [Header("Refs")]
    [SerializeField] SettingsManager settings;
    [SerializeField] Slider tutorialCompletedTgl;
    [SerializeField] Slider showFpsTgl;
    [SerializeField] Slider vibrationTgl;
    [SerializeField] Slider sensitivitySld;
    [SerializeField] Slider invertYTgl;
    //[SerializeField] Slider deadZoneSld;

    [Header("Audio")]
    [SerializeField] Slider bgmVolSld;
    [SerializeField] Slider sfxVolSld;

    [Header("Graphics")]
    [SerializeField] TMP_Dropdown qualityDropdown; // ¶Ç´Â TMP_Dropdown

    [Header("Buttons")]
    [SerializeField] Button defaultsBtn;
    [SerializeField] Button deleteBtn;
    [SerializeField] Button mainBtn;

    SettingsSaveData snapshot; // Cancel¿ë ½º³À¼¦

    void Awake()
    {
        if (!settings) settings = FindFirstObjectByType<SettingsManager>();
    }

    void OnEnable()
    {
        // UI ¡ç Settings (ÃÊ±â Ç¥½Ã)
        LoadToUI(settings);

        // ½º³À¼¦ ÀúÀå(Ãë¼Ò¿ë)
        snapshot = settings.ExportSave();

        var uimanager = FindFirstObjectByType<UIManager>();
        tutorialCompletedTgl.onValueChanged.AddListener(v => { 
            if (v > 0) Time.timeScale = 1f;
            settings.tutorialCompleted = (int)v; settings.ApplyRuntime();
            uimanager.OnAbleButtons(v == 0);
        });
        showFpsTgl.onValueChanged.AddListener(v => { settings.showFPS = (int)v; settings.ApplyRuntime(); });
        sensitivitySld.onValueChanged.AddListener(v => { settings.lookSensitivity = Mathf.Clamp(v, 0.1f, 1f); settings.ApplyRuntime(); });
        invertYTgl.onValueChanged.AddListener(v => { settings.invertY = Mathf.Clamp(v, 0.1f, 1f); settings.ApplyRuntime(); });
        vibrationTgl.onValueChanged.AddListener(v => { settings.vibration = (int)v; settings.ApplyRuntime(); });
        //deadZoneSld.onValueChanged.AddListener(v => { settings.deadZone = Mathf.Clamp01(v); settings.ApplyRuntime(); });

        bgmVolSld.onValueChanged.AddListener(v => { settings.bgmVol = Mathf.Clamp01(v); settings.ApplyRuntime(); });
        sfxVolSld.onValueChanged.AddListener(v => { settings.sfxVol = Mathf.Clamp01(v); settings.ApplyRuntime(); });

        var names = QualitySettings.names;
        var opts = new List<TMP_Dropdown.OptionData>(names.Length);
        foreach (var n in names) opts.Add(new TMP_Dropdown.OptionData(n));

        qualityDropdown.ClearOptions();
        qualityDropdown.AddOptions(opts);
        qualityDropdown.SetValueWithoutNotify(
            Mathf.Clamp(settings.qualityLevel, 0, names.Length - 1));
    
        qualityDropdown.onValueChanged.AddListener(i => { settings.qualityLevel = i; settings.ApplyRuntime(); });

        defaultsBtn.onClick.AddListener(LoadDefaults);

        deleteBtn.onClick.AddListener(DeleteData);
        mainBtn.onClick.AddListener(()=> { SceneLoader.I.Load("MainScreen"); Time.timeScale = 1f; });
    }

    private void OnDisable()
    {
        tutorialCompletedTgl.onValueChanged.RemoveAllListeners();
        showFpsTgl.onValueChanged.RemoveAllListeners();
        sensitivitySld.onValueChanged.RemoveAllListeners();
        invertYTgl.onValueChanged.RemoveAllListeners();
        vibrationTgl.onValueChanged.RemoveAllListeners();
        //deadZoneSld.onValueChanged.RemoveAllListeners();

        bgmVolSld.onValueChanged.RemoveAllListeners();
        sfxVolSld.onValueChanged.RemoveAllListeners();

        qualityDropdown.onValueChanged.RemoveAllListeners();

        defaultsBtn.onClick.RemoveAllListeners();

        deleteBtn.onClick.RemoveAllListeners();

        mainBtn.onClick.RemoveAllListeners();

        ApplyAndSave();
    }

    private void DeleteData()
    {
        SaveService.Delete();
        Time.timeScale = 1f;

        settings.tutorialCompleted = 1;

        var Bootstrap = FindFirstObjectByType<GameBootstrap>();
        if (Bootstrap != null)
        {
            Bootstrap.skipAutoSaveOnce = true;
        }
        RuntimeToast.Show("¼¼ÀÌºê µ¥ÀÌÅÍ¸¦ »èÁ¦Çß¾î¿ä.", 1.8f, 32,
        onDone: () => SceneManager.LoadScene(SceneManager.GetActiveScene().name));
    }

    private void LoadToUI(SettingsManager s)
    {
        Debug.Log("SettingsUIController: LoadToUI : " + s.tutorialCompleted + " | " + s.invertY);
        tutorialCompletedTgl.SetValueWithoutNotify(s.tutorialCompleted);
        showFpsTgl.SetValueWithoutNotify(s.showFPS);
        sensitivitySld.SetValueWithoutNotify(s.lookSensitivity);
        invertYTgl.SetValueWithoutNotify(s.invertY);
        vibrationTgl.SetValueWithoutNotify(s.vibration);
        //deadZoneSld.SetValueWithoutNotify(s.deadZone);

        bgmVolSld.SetValueWithoutNotify(s.bgmVol);
        sfxVolSld.SetValueWithoutNotify(s.sfxVol);

        qualityDropdown.SetValueWithoutNotify(Mathf.Clamp(s.qualityLevel, 0, QualitySettings.names.Length - 1));
    }

    private void ApplyAndSave()
    {
        // ÀÌ¹Ì ·±Å¸ÀÓ¿£ ¹Ý¿µµÅ ÀÖÀ¸´Ï ÆÄÀÏ¸¸ ÀúÀå
        SettingsService.Save(settings.ExportSave());
        // ½º³À¼¦ °»½Å
        snapshot = settings.ExportSave();
    }

    private void CancelChanges()
    {
        // ½º³À¼¦À» Àû¿ëÇÏ°í UI °»½Å
        settings.ApplySave(snapshot);
        LoadToUI(settings);
    }

    private void LoadDefaults()
    {
        var def = new SettingsSaveData
        {
            tutorial = 1,
            showFPS = 0,
            vibration = 1,
            lookSensitivity = 0.32f,
            invertY = 0.1f,
            deadZone = 0.1f,
            bgmVol = 1f,
            sfxVol = 1f,
            qualityLevel = 2,
        };
        settings.ApplySave(def);
        LoadToUI(settings);
    }
}
