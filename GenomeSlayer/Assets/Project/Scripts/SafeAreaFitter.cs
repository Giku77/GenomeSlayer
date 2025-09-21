using UnityEngine;

[ExecuteAlways]
[RequireComponent(typeof(RectTransform))]
public class SafeAreaFitter : MonoBehaviour
{
    RectTransform rt;
    Rect lastSafe;
    ScreenOrientation lastOri;
    Vector2 lastRes;

    void OnEnable() { rt = GetComponent<RectTransform>(); Apply(); }
    void Update()
    {
        if (Screen.safeArea != lastSafe ||
            Screen.orientation != lastOri ||
            lastRes != new Vector2(Screen.width, Screen.height))
        {
            Apply();
        }
    }

    void Apply()
    {
        if (rt == null) rt = GetComponent<RectTransform>();
        var sa = Screen.safeArea;

        Vector2 min = sa.position;
        Vector2 max = sa.position + sa.size;
        min.x /= Screen.width; min.y /= Screen.height;
        max.x /= Screen.width; max.y /= Screen.height;

        rt.anchorMin = min;
        rt.anchorMax = max;
        rt.offsetMin = Vector2.zero;
        rt.offsetMax = Vector2.zero;

        lastSafe = sa;
        lastOri = Screen.orientation;
        lastRes = new Vector2(Screen.width, Screen.height);
    }
}
