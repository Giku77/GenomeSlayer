using System.Collections;
using UnityEngine;
using UnityEngine.UI;

[RequireComponent(typeof(Button))]
public class BlinkBackground : MonoBehaviour
{
    [Tooltip("깜빡일 대상(미지정 시 Button.targetGraphic 사용)")]
    public Graphic target;

    [Header("Blink")]
    [Range(0f, 1f)] public float minAlpha = 0.4f;
    [Range(0f, 1f)] public float maxAlpha = 1f;
    public float period = 0.6f;     // 한 번 커졌다 작아지는 시간
    public bool playOnEnable = true;
    public bool useUnscaledTime = true;
    public int pulseCount = 0;      // 0이면 무한, 그 외엔 N번만

    Coroutine co;

    void Awake()
    {
        if (target == null)
        {
            var btn = GetComponent<Button>();
            target = btn ? btn.targetGraphic : GetComponent<Graphic>();
        }
    }

    void OnEnable()
    {
        if (playOnEnable) Play();
    }

    void OnDisable()
    {
        Stop();
        if (target) target.canvasRenderer.SetAlpha(1f);
    }

    public void Play()
    {
        if (co != null) StopCoroutine(co);
        co = StartCoroutine(CoBlink());
    }

    public void Stop()
    {
        if (co != null) StopCoroutine(co);
        co = null;
    }

    IEnumerator CoBlink()
    {
        if (!target) yield break;

        var half = Mathf.Max(0.01f, period * 0.5f);
        int played = 0;

        target.canvasRenderer.SetAlpha(maxAlpha);

        while (pulseCount == 0 || played < pulseCount)
        {
            target.CrossFadeAlpha(minAlpha, half, useUnscaledTime);
            yield return (useUnscaledTime
                ? new WaitForSecondsRealtime(half)
                : new WaitForSeconds(half));

            target.CrossFadeAlpha(maxAlpha, half, useUnscaledTime);
            yield return (useUnscaledTime
                ? new WaitForSecondsRealtime(half)
                : new WaitForSeconds(half));

            played++;
        }

        co = null;
    }
}
