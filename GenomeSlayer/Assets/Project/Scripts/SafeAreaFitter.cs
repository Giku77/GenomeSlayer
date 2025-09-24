using UnityEngine;
using UnityEngine.UI;

[RequireComponent(typeof(RectTransform))]
public class SafeAreaFitter : MonoBehaviour
{
    RectTransform rt;
    Canvas rootCanvas;
    Rect lastSafe;
    Vector2 lastRes;

    void Awake()
    {
        rt = GetComponent<RectTransform>();
        rootCanvas = GetComponentInParent<Canvas>();
    }

    void OnEnable() { StartCoroutine(ApplySafeAreaNextFrame()); }

    void Update()
    {
        var res = new Vector2(Screen.width, Screen.height);
        if (res != lastRes || Screen.safeArea != lastSafe)
            StartCoroutine(ApplySafeAreaNextFrame());
    }

    System.Collections.IEnumerator ApplySafeAreaNextFrame()
    {
        yield return null;
        Canvas.ForceUpdateCanvases();
        Apply();
    }

    void Apply()
    {
        if (!rt) return;

        int sw = Screen.width;
        int sh = Screen.height;
        if (rootCanvas == null || sw <= 0 || sh <= 0) return;

        if (rootCanvas.renderMode == RenderMode.WorldSpace) return;

        Rect safe = Screen.safeArea;
        lastSafe = safe;
        lastRes = new Vector2(sw, sh);


        float invW = 1f / Mathf.Max(1, sw);
        float invH = 1f / Mathf.Max(1, sh);

        Vector2 anchorMin = new Vector2(safe.xMin * invW, safe.yMin * invH);
        Vector2 anchorMax = new Vector2(safe.xMax * invW, safe.yMax * invH);

  
        anchorMin.x = Mathf.Clamp01(anchorMin.x);
        anchorMin.y = Mathf.Clamp01(anchorMin.y);
        anchorMax.x = Mathf.Clamp01(anchorMax.x);
        anchorMax.y = Mathf.Clamp01(anchorMax.y);

        rt.anchorMin = anchorMin;
        rt.anchorMax = anchorMax;

        rt.offsetMin = Vector2.zero;
        rt.offsetMax = Vector2.zero;
        rt.anchoredPosition = Vector2.zero;
        rt.sizeDelta = Vector2.zero;
    }
}
