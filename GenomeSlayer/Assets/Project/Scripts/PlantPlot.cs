using UnityEngine;

public class PlantPlot : MonoBehaviour, IInteractable
{
    [Header("Refs")]
    public SeedDef seed;          
    public LineRenderer ring;     

    [Header("Growth")]
    public float progress;        // 0~1
    public float interactCharge = 0.25f;  // F 한번당 채워질 양
    //public float autoGrowRate = 0f;       // 초당 자동 성장(선택)

    public string Prompt => $"[F] Grow ({Mathf.RoundToInt(progress * 100)}%)";

    void Awake()
    {
        if (ring != null)
        {
            ring.useWorldSpace = false;  
            ring.loop = true;
            ring.startWidth = ring.endWidth = 0.15f;
            // ring.material = yourUnlitMaterial; // 인스펙터에서 할당
            DrawCircle(ring, 1.0f, 64);
        }
    }

    void Update()
    {
        //if (autoGrowRate > 0f)
        //{
        //    progress = Mathf.Min(1f, progress + autoGrowRate * Time.deltaTime);
        //    TryComplete();
        //}
    }

    public void Interact(Player player)
    {
        progress = Mathf.Min(1f, progress + interactCharge);
        Debug.Log($"PlantPlot: Progress increased to {progress}");
        TryComplete();
    }

    void TryComplete()
    {
        if (progress < 1f || seed == null || seed.treePrefab == null) return;
        Instantiate(seed.treePrefab, transform.position, Quaternion.identity);
        Destroy(gameObject);
    }

    void DrawCircle(LineRenderer lr, float radius, int seg)
    {
        lr.positionCount = seg;
        for (int i = 0; i < seg; i++)
        {
            float t = (float)i / seg * Mathf.PI * 2f;
            lr.SetPosition(i, new Vector3(Mathf.Cos(t) * radius, 0.05f, Mathf.Sin(t) * radius));
        }
    }
}
