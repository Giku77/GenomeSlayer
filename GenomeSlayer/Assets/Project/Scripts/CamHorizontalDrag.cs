using UnityEngine;
using Unity.Cinemachine;

public class CamHorizontalDrag : MonoBehaviour
{
    public CinemachineCamera vcam;
    public float dragSpeed = 0.15f;

    Vector2? last;
    CinemachineOrbitalFollow orbital;

    void Awake() => orbital = vcam.GetComponent<CinemachineOrbitalFollow>();

    void Update()
    {
        if (orbital == null) return;

        if (Input.GetMouseButtonDown(0)) last = Input.mousePosition;
        if (Input.GetMouseButtonUp(0)) last = null;

        if (last.HasValue && Input.GetMouseButton(0))
        {
            var now = (Vector2)Input.mousePosition;
            float dx = now.x - last.Value.x;
            orbital.HorizontalAxis.Value += dx * dragSpeed;   // ← 오비트(수평 회전)
            last = now;
        }
    }
}
