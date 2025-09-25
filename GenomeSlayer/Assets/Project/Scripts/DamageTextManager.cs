using System.Collections.Generic;
using UnityEngine;

public class DamageTextManager : MonoBehaviour
{
    public static DamageTextManager I { get; private set; }

    public Canvas overlayCanvas;        // Render Mode = Screen Space - Overlay
    public DamageText textPrefab;

    readonly Queue<DamageText> pool = new();

    void Awake() => I = this;

    public void Show(int amount, Vector3 worldPos)
    {
        var ui = Get();
        ui.transform.SetParent(overlayCanvas.transform, false);
        if (!ui.gameObject.activeSelf) ui.gameObject.SetActive(true);
        if (!ui.isActiveAndEnabled) ui.enabled = true;

        ui.ShowOverlay(amount, worldPos, (RectTransform)overlayCanvas.transform);
    }

    DamageText Get()
    {
        if (pool.Count > 0)
        {
            var t = pool.Dequeue();
            if (!t.gameObject.activeSelf) t.gameObject.SetActive(true);
            if (!t.isActiveAndEnabled) t.enabled = true;
            return t;
        }
        return Instantiate(textPrefab);
    }

    public void Return(DamageText t)
    {
        t.gameObject.SetActive(false);
        pool.Enqueue(t);
    }
}
