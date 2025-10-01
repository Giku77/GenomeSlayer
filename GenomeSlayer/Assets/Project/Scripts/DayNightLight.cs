using UnityEngine;

public class DayNightLight : MonoBehaviour
{
    public Light sunLight;
    public Light Light;
    private float maxTime;
    private float timer;

    public WavesManager waveManager;

    public float dayIntensity = 2.0f;
    public float dawnIntensity = 0.91f;
    public float nightIntensity = 0f;

    public Color dayColor = Color.white;
    public Color nightColor = new Color(0.1f, 0.1f, 0.35f); // ¹ã ÆÄ¶õºû

    public void StartDay()
    {
        maxTime = waveManager.currentInterval;
        timer = maxTime;
    }

    void Update()
    {
        if (!waveManager.waveDone) return;

        timer -= Time.deltaTime;
        if (timer < 0f) timer = 0f;

        float stopSec = maxTime * 0.1f;
        float t = Mathf.Clamp01(Mathf.InverseLerp(stopSec, maxTime, timer));
        float ratio = Mathf.SmoothStep(0f, 1f, t); 
        //float ratio = (maxTime > 0f) ? timer / maxTime : 0f;

        sunLight.intensity = Mathf.Lerp(nightIntensity, dayIntensity, ratio);
        Light.intensity = Mathf.Lerp(nightIntensity, dawnIntensity, ratio);
        sunLight.color = Color.Lerp(nightColor, dayColor, ratio);

        if (timer <= 0f) timer = maxTime;
    }
}
