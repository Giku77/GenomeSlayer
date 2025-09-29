using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.UI;

public class PlantPlot : MonoBehaviour, IInteractable
{
    [Header("Refs")]
    public SeedDef seed;          
    public LineRenderer ring;

    [Header("Growth")]
    public float progress;        // 0~1
    public float interactCharge = 0.25f;  // F 한번당 채워질 양
    //public float autoGrowRate = 0f;       // 초당 자동 성장(선택)

    private GameObject player;
    private Slider slider;


    public string Prompt => $"[F] Grow ({Mathf.RoundToInt(progress * 100)}%)";


    void Awake()
    {
        slider = GetComponentInChildren<Slider>();
        if (slider != null)
        {
            slider.minValue = 0f;
            slider.maxValue = 1f;
            slider.value = progress;
        }
        player = GameObject.FindWithTag("Player");
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
        var inventory = player.quickSlotInventory;  
        var i = inventory.FindSlotWithItem((int)ItemIds.Earthy_Fertilizer);
        Debug.Log($"PlantPlot: Found fertilizer slot at index {i}");
        if (inventory.TryConsume(i, 1))
        {
            AudioManager.I.PlaySFX("PlantGrow");
            EffectManager.I.Play("PlantGrow", transform.position + Vector3.up * 0.5f, Quaternion.identity);
            //EventBus.UpdateSlot?.Invoke(i, inventory.GetSlot(i).IsEmpty ? string.Empty : "0", inventory.GetSlot(i).IsEmpty ? string.Empty : inventory.GetSlot(i).quantity.ToString(), -1);
            progress = Mathf.Min(1f, progress + interactCharge);
            if (slider != null)
                slider.value = progress;
            Debug.Log($"PlantPlot: Progress increased to {progress}");
            TryComplete();
        }
    }

    void TryComplete()
    {
        if (progress < 1f || seed == null || seed.treePrefab == null) return;
        var t = Instantiate(seed.treePrefab, transform.position, Quaternion.identity);
        var s = t.GetComponent<TreeEntity>();
        var table = DataTableManger.ItemTable.GetItem(seed.itemId);
        s.SeedNum = table.itemImprovedNum;
        EventBus.RemoveObj.Add(t);
        Destroy(gameObject);
        var g = GameObject.FindGameObjectWithTag("Ges").GetComponent<StateManager>();
        g.AddGenomePoint(100);
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
