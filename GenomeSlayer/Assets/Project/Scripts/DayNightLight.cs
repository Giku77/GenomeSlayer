using UnityEngine;

public class DayNightLight : MonoBehaviour
{
    public Light sunLight;
    private float maxTime;
    private float timer;

    public WavesManager waveManager;

    public float dayIntensity = 2.0f;
    public float nightIntensity = 0.1f;

    public Color dayColor = Color.white;
    public Color nightColor = new Color(0.1f, 0.1f, 0.35f); // ¹ã ÆÄ¶õºû

    void Start()
    {
        maxTime = waveManager.currentInterval;
        timer = maxTime;
    }

    void Update()
    {
        if (waveManager.waveDone)
        {
            if (timer == 0)
                timer = maxTime;
            timer -= Time.deltaTime;
            if (timer < 0) timer = 0;

            float ratio = timer / maxTime; // 1 ¡æ 0

            sunLight.intensity = Mathf.Lerp(nightIntensity, dayIntensity, ratio);
            sunLight.color = Color.Lerp(nightColor, dayColor, ratio);
        }
    }
}
