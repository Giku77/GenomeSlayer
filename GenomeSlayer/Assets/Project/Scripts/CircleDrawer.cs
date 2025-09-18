using UnityEngine;

[RequireComponent(typeof(LineRenderer))]
public class CircleDrawer : MonoBehaviour
{
    public SphereCollider sphereCollider;
    public int segments = 64;
    private LineRenderer line;

    void Start()
    {
        line = GetComponent<LineRenderer>();
        line.useWorldSpace = false;
        line.loop = true;
        line.positionCount = segments;

        DrawCircle();
    }

    void DrawCircle()
    {
        float radius = sphereCollider.radius;
        Vector3 center = sphereCollider.center;

        for (int i = 0; i < segments; i++)
        {
            float angle = ((float)i / segments) * Mathf.PI * 2f;
            float x = Mathf.Cos(angle) * radius;
            float z = Mathf.Sin(angle) * radius;
            line.SetPosition(i, center + new Vector3(x, 0, z));
        }
    }
}
