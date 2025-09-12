using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class UIManager : MonoBehaviour
{
    public Slider HealthSilder;
    public TextMeshProUGUI CurrentWave;
    public TextMeshProUGUI WaveTimer;
    public Button WaveButton;


    public void UpdateHealth(int health, int max)
    {
        HealthSilder.maxValue = max;
        HealthSilder.value = health;
    }

    public void ActiveWaveButton(bool t)
    {
        WaveButton.gameObject.SetActive(t);
    }

    public void UpdateWave(int wave)
    {
        CurrentWave.text = "CHAPTER: " + wave.ToString("D2");
    }

    public void UpdateWaveTimer(float time)
    {
        WaveTimer.text = $"{time:F0}";
    }
}
