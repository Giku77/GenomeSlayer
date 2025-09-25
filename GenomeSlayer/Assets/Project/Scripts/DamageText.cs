using System.Collections;
using UnityEngine;
using TMPro;

public class DamageText : MonoBehaviour
{
    public TextMeshProUGUI label;
    public float life = 0.7f;
    public float rise = 60f;       // px 기준(Overlay라 픽셀)
    public float randomXY = 25f;   // px 기준

    RectTransform _rt;

    void Awake() => _rt = (RectTransform)transform;

    public void ShowOverlay(int amount, Vector3 worldPos, RectTransform overlayCanvasRect)
    {
        var cam = Camera.main;
        Vector3 sp = cam ? cam.WorldToScreenPoint(worldPos) : new Vector3(Screen.width * 0.5f, Screen.height * 0.5f, 0);

        if (sp.z < 0f) sp.z = 0.001f;

        RectTransformUtility.ScreenPointToLocalPointInRectangle(overlayCanvasRect, sp, null, out var lp);

        _rt.anchoredPosition = lp;
        label.text = amount.ToString();
        label.alpha = 1f;

        var jitter = new Vector2(Random.Range(-randomXY, randomXY), Random.Range(0f, randomXY));

        StopAllCoroutines();
        StartCoroutine(CoPlayOverlay(lp, jitter));
    }

    IEnumerator CoPlayOverlay(Vector2 startLocalPos, Vector2 jitter)
    {
        float t = 0f;
        while (t < life)
        {
            t += Time.deltaTime;
            float u = t / life;

            Vector2 p = startLocalPos + jitter + Vector2.up * (rise * u);
            _rt.anchoredPosition = p;

            label.alpha = 1f - u;

            yield return null;
        }
        DamageTextManager.I.Return(this);
    }
}
