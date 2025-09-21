using UnityEngine;
using UnityEngine.UI;

[RequireComponent(typeof(Image))]
public class UIFocusHighlighter : MonoBehaviour
{
    [Header("Target to highlight")]
    public RectTransform target;          // 포커스할 UI
    public Vector2 padding = new Vector2(16, 16); // 테두리 여유

    [Header("Blink")]
    public bool blink = true;
    public float blinkSpeed = 2f;         // 1~5 권장
    public float minAlpha = 0.3f;
    public float maxAlpha = 1f;

    Image ring;
    RectTransform ringRt;
    Canvas rootCanvas;

    void Awake()
    {
        ring = GetComponent<Image>();     
        ringRt = (RectTransform)transform;
        rootCanvas = GetComponentInParent<Canvas>();

        ring.raycastTarget = false;
    }

    void OnEnable()
    {
        UpdateRingToTarget();
    }

    void LateUpdate()
    {
        if (target == null) return;

        UpdateRingToTarget();

        if (blink)
        {
            var c = ring.color;
            float t = (Mathf.Sin(Time.unscaledTime * Mathf.PI * blinkSpeed) + 1f) * 0.5f; // 0~1
            c.a = Mathf.Lerp(minAlpha, maxAlpha, t);
            ring.color = c;
        }
    }

    void UpdateRingToTarget()
    {
        if (target == null) return;

        var size = target.rect.size + padding;
        ringRt.sizeDelta = size;

        ringRt.position = target.TransformPoint(target.rect.center);
        ringRt.rotation = Quaternion.identity;
        ringRt.localScale = Vector3.one;
    }

    public void SetTarget(RectTransform newTarget) => target = newTarget;
}
