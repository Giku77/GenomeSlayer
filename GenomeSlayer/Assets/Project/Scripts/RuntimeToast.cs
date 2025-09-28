using System.Collections;
using UnityEngine;

public class RuntimeToast : MonoBehaviour
{
    static RuntimeToast _instance;

    string _msg;
    float _duration;
    float _t;

    GUIStyle _style;
    Rect _rect;

    float _fadeIn = 0.1f;
    float _fadeOut = 0.25f;

    public static int DefaultFontSize = 28;

    public static void Show(string message, float duration = 1.8f, int? fontSizePx = null, System.Action onDone = null)
    {
        if (_instance == null)
        {
            var go = new GameObject("[RuntimeToast]");
            DontDestroyOnLoad(go);
            _instance = go.AddComponent<RuntimeToast>();
        }
        _instance.StartToast(message, duration, fontSizePx ?? DefaultFontSize, onDone);
    }

    int _fontSizePx = DefaultFontSize;

    System.Action _onDone;
    void StartToast(string message, float duration, int fontSizePx, System.Action onDone)
    {
        _msg = message;
        _duration = Mathf.Max(0.3f, duration);
        _t = 0f;
        _fontSizePx = Mathf.Max(12, fontSizePx);
        _onDone = onDone;

        float w = Mathf.Min(Screen.width * 0.7f, 800f);
        _rect = new Rect((Screen.width - w) * 0.5f, 0f, w, 0f); 

 
        _style = null;

        StopAllCoroutines();
        StartCoroutine(CoLife());
    }

    IEnumerator CoLife()
    {
        while (_t < _duration)
        {
            _t += Time.unscaledDeltaTime;
            yield return null;
        }
        _msg = null;
        _onDone?.Invoke(); 
        _onDone = null;
    }

    void EnsureStyle()
    {
        if (_style != null) return;

        _style = new GUIStyle(GUI.skin.box)
        {
            alignment = TextAnchor.MiddleCenter,
            wordWrap = true,
            richText = true,                     
            padding = new RectOffset(18, 18, 12, 12),
            fontSize = _fontSizePx             
        };

        var content = new GUIContent(_msg);
        float textH = _style.CalcHeight(content, _rect.width);
        float boxH = textH + _style.padding.top + _style.padding.bottom;

        float y = Screen.height - boxH - 120f;
        _rect = new Rect(_rect.x, y, _rect.width, boxH);
    }

    void OnGUI()
    {
        if (string.IsNullOrEmpty(_msg)) return;

        EnsureStyle();

        float a = 1f;
        if (_t < _fadeIn) a = Mathf.InverseLerp(0, _fadeIn, _t);
        else if (_t > _duration - _fadeOut) a = Mathf.InverseLerp(_duration, _duration - _fadeOut, _t);

        var prev = GUI.color;

        GUI.color = new Color(0f, 0f, 0f, 0.75f * a);
        GUI.Box(_rect, GUIContent.none, _style);

        GUI.color = new Color(1f, 1f, 1f, a);
        GUI.Label(_rect, _msg, _style);

        GUI.color = prev;
    }
}
