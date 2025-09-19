using System.Collections.Generic;
using Unity.Cinemachine;
using UnityEngine;
using UnityEngine.EventSystems;

public class CamHorizontalDrag : MonoBehaviour
{
    public CinemachineCamera vcam;
    public float dragSpeedX = 0.15f;   
    public float dragSpeedY = 0.10f;  
    public bool useDpiScale = true;    

    Vector2 lastPos;
    int camFingerId = -1;             
    CinemachineOrbitalFollow orbital;


    bool IsOverBlockingUI(Vector2 screenPos)
    {
        if (EventSystem.current == null) return false;

        var ped = new PointerEventData(EventSystem.current) { position = screenPos };
        var results = new List<RaycastResult>();
        EventSystem.current.RaycastAll(ped, results);

        foreach (var r in results)
        {
            var go = r.gameObject;

            if (go.CompareTag("Joystick") || go.name.Contains("Joystick"))
                continue;

            if (go.CompareTag("BlockUI") || go.name.Contains("Pause") || go.name.Contains("Menu"))
                return true;
        }
        return false;
    }

    void Awake()
    {
        Input.multiTouchEnabled = true;
        Input.simulateMouseWithTouches = false;
        orbital = vcam.GetComponent<CinemachineOrbitalFollow>();
    }

    void Update()
    {
        if (orbital == null) return;

#if UNITY_EDITOR || UNITY_STANDALONE
        // 마우스: UI 위면 무시
        if (EventSystem.current && EventSystem.current.IsPointerOverGameObject()) return;

        if (Input.GetMouseButtonDown(0))
        {
            lastPos = Input.mousePosition;
            camFingerId = 0; // 임의
        }
        else if (Input.GetMouseButtonUp(0))
        {
            camFingerId = -1;
        }

        if (camFingerId != -1 && Input.GetMouseButton(0))
        {
            Vector2 now = Input.mousePosition;
            ApplyDelta(now - lastPos);
            lastPos = now;
        }
#else
        if (EventSystem.current == null) return;

        for (int i = 0; i < Input.touchCount; i++)
        {
            var t = Input.GetTouch(i);
            //bool overUI = EventSystem.current.IsPointerOverGameObject(t.fingerId);
            bool overUI = IsOverBlockingUI(t.position);

            if (t.phase == TouchPhase.Began)
            {
                if (!overUI && camFingerId == -1)
                {
                    camFingerId = t.fingerId;
                    lastPos = t.position;
                }
            }

            if (t.fingerId == camFingerId)
            {
                if (t.phase == TouchPhase.Moved || t.phase == TouchPhase.Stationary)
                {
                    ApplyDelta(t.position - lastPos);
                    lastPos = t.position;
                }
                if (t.phase == TouchPhase.Ended || t.phase == TouchPhase.Canceled)
                {
                    camFingerId = -1;
                }
            }
        }
#endif
    }

    void ApplyDelta(Vector2 delta)
    {
        float dpiScale = (useDpiScale && Screen.dpi > 0f) ? (Screen.dpi / 160f) : 1f;
        float dx = delta.x / dpiScale;
        float dy = delta.y / dpiScale;

        orbital.HorizontalAxis.Value += dx * dragSpeedX;

        orbital.VerticalAxis.Value -= dy * dragSpeedY;

        // 필요하면 범위 클램프(옵션) — OrbitalFollow가 내부에서 처리하는 경우가 많음.
        // orbital.VerticalAxis.Value = Mathf.Clamp(orbital.VerticalAxis.Value, min, max);
    }
}
