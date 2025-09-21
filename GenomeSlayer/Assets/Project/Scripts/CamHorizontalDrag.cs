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
    public float doubleTapTime = 0.3f;         
    public float doubleTapMaxPixels = 40f;    
    public float snapDuration = 0.25f;        
    public bool snapEaseOut = true;           

    [Header("Zoom (FOV)")]
    public float minFov = 35f;         
    public float wheelZoomSpeed = 5f;  
    public float pinchZoomSpeed = 0.05f; 
    public float fovSmoothTime = 0.08f;

    [Header("Turn Smoothing")]
    public float turnSmoothTime = 0.07f;     
    public float turnMaxSpeed = 1080f;    
    public float minVertical = -30f;      
    public float maxVertical = 60f;

    float targetH, targetV;   
    float hVel, vVel;        
    bool dragging;           

    float defaultFov;    
    float fovVel;        
    bool isPinching;   


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

        defaultFov = vcam.Lens.FieldOfView;

        if (orbital != null)
        {
            defaultH = orbital.HorizontalAxis.Value;
            defaultV = orbital.VerticalAxis.Value;

            targetH = defaultH;
            targetV = defaultV;
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
            dragging = true;
        }
        else if (Input.GetMouseButtonUp(0))
        {
            camFingerId = -1;
            dragging = false;
        }

        if (camFingerId != -1 && Input.GetMouseButton(0))
        {
            Vector2 now = (Vector2)Input.mousePosition;
            ApplyDelta(now - lastPos);
            lastPos = now;
        }

        float wheel = Input.mouseScrollDelta.y;
        if (Mathf.Abs(wheel) > 0.0001f)
        {
            float target = vcam.Lens.FieldOfView - wheel * wheelZoomSpeed;
            target = Mathf.Clamp(target, minFov, defaultFov);
            // 부드럽게
            float fov = Mathf.SmoothDamp(vcam.Lens.FieldOfView, target, ref fovVel, fovSmoothTime, Mathf.Infinity, Time.unscaledDeltaTime);
            vcam.Lens.FieldOfView = fov;
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
                    dragging = true;
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
                    dragging = false;    
                }
            }
        }
        if (Input.touchCount >= 2)
{
    var t0 = Input.GetTouch(0);
    var t1 = Input.GetTouch(1);

    if (!(IsOverBlockingUI(t0.position) || IsOverBlockingUI(t1.position)))
    {
        Vector2 p0Prev = t0.position - t0.deltaPosition;
        Vector2 p1Prev = t1.position - t1.deltaPosition;

        float prevDist = (p0Prev - p1Prev).magnitude;
        float currDist = (t0.position - t1.position).magnitude;
        float delta = currDist - prevDist; // +면 벌림(줌 인), -면 좁힘(줌 아웃)

        if (!isPinching)
        {
            isPinching = true;
            camFingerId = -1;
        }

        float target = vcam.Lens.FieldOfView - delta * pinchZoomSpeed;
        target = Mathf.Clamp(target, minFov, defaultFov);
        float fov = Mathf.SmoothDamp(vcam.Lens.FieldOfView, target, ref fovVel, fovSmoothTime, Mathf.Infinity, Time.unscaledDeltaTime);
        vcam.Lens.FieldOfView = fov;
    }
}
else
{
    isPinching = false;
}
#endif
        if (orbital != null && snapCo == null)
        {
            float dt = Time.unscaledDeltaTime;  

            float curH = orbital.HorizontalAxis.Value;
            float curV = orbital.VerticalAxis.Value;

            curH = Mathf.SmoothDampAngle(curH, targetH, ref hVel, turnSmoothTime, turnMaxSpeed, dt);
            curV = Mathf.SmoothDamp(curV, targetV, ref vVel, turnSmoothTime, turnMaxSpeed, dt);

            orbital.HorizontalAxis.Value = curH;
            orbital.VerticalAxis.Value = curV;
        }

        if (!dragging)
        {
            hVel *= 0.90f;   
            vVel *= 0.90f;
        }
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

        targetH = defaultH;
        targetV = defaultV;
        hVel = vVel = 0f;

        snapCo = null;
    }

    IEnumerator CoSnapHeightOnly()
    {
        float startV = orbital.VerticalAxis.Value;
        float startH = orbital.HorizontalAxis.Value; // 방향은 고정


        float snapV = defaultV;

        float t = 0f;
        while (t < 1f)
        {
            t += (snapDuration > 0f ? Time.unscaledDeltaTime / snapDuration : 1f);
            float k = Mathf.Clamp01(t);
            if (snapEaseOut) k = 1f - Mathf.Pow(1f - k, 3f); // ease-out

  
            orbital.HorizontalAxis.Value = startH;
   
            orbital.VerticalAxis.Value = Mathf.Lerp(startV, snapV, k);

            yield return null;
        }

        orbital.HorizontalAxis.Value = startH;   
        orbital.VerticalAxis.Value = snapV;

        targetH = orbital.HorizontalAxis.Value;
        targetV = orbital.VerticalAxis.Value;

        hVel = 0f;
        vVel = 0f;
        snapCo = null;
    }

    void ApplyDelta(Vector2 delta)
    {
        float dpiScale = (useDpiScale && Screen.dpi > 0f) ? (Screen.dpi / 160f) : 1f;
        float dx = delta.x / dpiScale;
        float dy = delta.y / dpiScale;

        targetH += dx * dragSpeedX;
        targetV -= dy * dragSpeedY;
        targetV = Mathf.Clamp(targetV, minVertical, maxVertical);

        //orbital.HorizontalAxis.Value += dx * dragSpeedX;
        //orbital.VerticalAxis.Value -= dy * dragSpeedY;
    }
}
