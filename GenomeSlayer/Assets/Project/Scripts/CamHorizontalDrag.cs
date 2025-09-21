using System.Collections.Generic;
using Unity.Cinemachine;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI; // Graphic
using System.Collections;

public class CamHorizontalDrag : MonoBehaviour
{
    public CinemachineCamera vcam;
    public Transform joystickRoot;

    [Header("Drag")]
    public float dragSpeedX = 0.15f;
    public float dragSpeedY = 0.10f;
    public bool useDpiScale = true;

    [Header("Double Tap to Snap")]
    public float doubleTapTime = 0.3f;         // 두 번 탭 사이 최대 시간(초)
    public float doubleTapMaxPixels = 40f;     // 두 탭 사이 최대 이동(픽셀)
    public float snapDuration = 0.25f;         // 스냅 보간 시간
    public bool snapEaseOut = true;           // 부드러운 감쇠

    Vector2 lastPos;
    int camFingerId = -1;
    CinemachineOrbitalFollow orbital;


    float lastTapTime = -10f;
    Vector2 lastTapPos;

  
    float defaultH;
    float defaultV;

    Coroutine snapCo;

    bool IsOverBlockingUI(Vector2 screenPos)
    {
        if (EventSystem.current == null) return false;

        var ped = new PointerEventData(EventSystem.current) { position = screenPos };
        var results = new List<RaycastResult>();
        EventSystem.current.RaycastAll(ped, results);

        foreach (var r in results)
        {
            var go = r.gameObject;

  
            if (joystickRoot != null && go.transform.IsChildOf(joystickRoot))
                return true;

            var g = go.GetComponent<Graphic>();
            if (g != null && g.raycastTarget == false)
                continue;

            if (go.CompareTag("BlockUI") || go.name.Contains("Pause") || go.name.Contains("Menu"))
                return true;

            if (go.GetComponent<Button>() || go.GetComponent<Slider>() || go.GetComponent<Toggle>())
                return true;
        }
        return false;
    }

    void Awake()
    {
        Input.multiTouchEnabled = true;
        Input.simulateMouseWithTouches = false;

        orbital = vcam.GetComponent<CinemachineOrbitalFollow>();

   
        if (orbital != null)
        {
            defaultH = orbital.HorizontalAxis.Value;
            defaultV = orbital.VerticalAxis.Value;
        }
    }

    void Update()
    {
        if (orbital == null) return;

#if UNITY_EDITOR || UNITY_STANDALONE
    
        if (Input.GetMouseButtonDown(0))
        {
            if (EventSystem.current && EventSystem.current.IsPointerOverGameObject())
                return;

            Vector2 pos = Input.mousePosition;
            if (IsDoubleTap(pos))
            {
                StartSnap();
         
                camFingerId = -1;
                lastPos = pos;
                return;
            }

            lastPos = pos;
            camFingerId = 0;
        }
        else if (Input.GetMouseButtonUp(0))
        {
            camFingerId = -1;
        }

        if (camFingerId != -1 && Input.GetMouseButton(0))
        {
            Vector2 now = (Vector2)Input.mousePosition;
            ApplyDelta(now - lastPos);
            lastPos = now;
        }
#else
        if (EventSystem.current == null) return;

        for (int i = 0; i < Input.touchCount; i++)
        {
            var t = Input.GetTouch(i);
            bool overUI = IsOverBlockingUI(t.position);

            if (t.phase == TouchPhase.Began)
            {
  
                if (!overUI && IsDoubleTap(t.position))
                {
                    StartSnap();
     
                    camFingerId = -1;
                    lastPos = t.position;
                    continue;
                }

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

    bool IsDoubleTap(Vector2 currentPos)
    {
        float now = Time.unscaledTime;

        float dpiScale = (useDpiScale && Screen.dpi > 0f) ? (Screen.dpi / 160f) : 1f;
        float maxDist = doubleTapMaxPixels * dpiScale;

        bool withinTime = (now - lastTapTime) <= doubleTapTime;
        bool withinDist = (Vector2.SqrMagnitude(currentPos - lastTapPos) <= maxDist * maxDist);

        bool isDouble = (withinTime && withinDist);

        lastTapTime = now;
        lastTapPos = currentPos;

        return isDouble;
    }

    void StartSnap()
    {
        if (snapCo != null) StopCoroutine(snapCo);
        snapCo = StartCoroutine(CoSnapHeightOnly());
        //snapCo = StartCoroutine(CoSnapToDefault());
    }

    IEnumerator CoSnapToDefault()
    {
        float startH = orbital.HorizontalAxis.Value;
        float startV = orbital.VerticalAxis.Value;

        float t = 0f;
        while (t < 1f)
        {
            t += (snapDuration > 0f ? Time.unscaledDeltaTime / snapDuration : 1f);
            float k = Mathf.Clamp01(t);
            if (snapEaseOut)
                k = 1f - Mathf.Pow(1f - k, 3f); 

            orbital.HorizontalAxis.Value = Mathf.LerpAngle(startH, defaultH, k);
            orbital.VerticalAxis.Value = Mathf.Lerp(startV, defaultV, k);
            yield return null;
        }

        orbital.HorizontalAxis.Value = defaultH;
        orbital.VerticalAxis.Value = defaultV;
        snapCo = null;
    }

    IEnumerator CoSnapHeightOnly()
    {
        float startV = orbital.VerticalAxis.Value;
        float startH = orbital.HorizontalAxis.Value; // 방향은 고정

        float targetV = defaultV;

        float t = 0f;
        while (t < 1f)
        {
            t += (snapDuration > 0f ? Time.unscaledDeltaTime / snapDuration : 1f);
            float k = Mathf.Clamp01(t);
            if (snapEaseOut) k = 1f - Mathf.Pow(1f - k, 3f); // ease-out

  
            orbital.HorizontalAxis.Value = startH;
   
            orbital.VerticalAxis.Value = Mathf.Lerp(startV, targetV, k);

            yield return null;
        }

        orbital.HorizontalAxis.Value = startH;   
        orbital.VerticalAxis.Value = targetV;  
        snapCo = null;
    }

    void ApplyDelta(Vector2 delta)
    {
        float dpiScale = (useDpiScale && Screen.dpi > 0f) ? (Screen.dpi / 160f) : 1f;
        float dx = delta.x / dpiScale;
        float dy = delta.y / dpiScale;

        orbital.HorizontalAxis.Value += dx * dragSpeedX;
        orbital.VerticalAxis.Value -= dy * dragSpeedY;
    }
}
