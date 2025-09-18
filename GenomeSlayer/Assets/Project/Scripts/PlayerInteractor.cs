using UnityEngine;
using UnityEngine.AI;
using System.Collections.Generic;
using System.Linq;


public class PlayerInteractor : MonoBehaviour
{
    public KeyCode key = KeyCode.F;
    public LayerMask groundMask;             
    public float maxDistance = 8f;
    public GameObject plantPlotPrefab;        
    public SeedDef[] seedDefs;               
    private Dictionary<int, SeedDef> seedMap;

    public bool IsInteracting { get; set; } = false;
    public Vector3 lastWorldPos { get; set; }

    private QuickSlotInventory quickSlotInventory;

    void Awake()
    {
        quickSlotInventory = GetComponent<Player>().quickSlotInventory;
        seedMap = new();
        foreach (var s in seedDefs) seedMap[s.itemId] = s;
    }

    void Update()
    {
#if UNITY_EDITOR
        if (!Input.GetKeyDown(key)) return;
#endif
#if UNITY_ANDROID && !UNITY_EDITOR
        if (!IsInteracting) return;
        IsInteracting = false;
#endif
        TryPlantAt(transform.position);
        //var cam = Camera.main;
        //var ray = cam.ScreenPointToRay(Input.mousePosition);

        //if (Physics.Raycast(ray, out var hit, maxDistance))
        //{
        //    Debug.Log($"PlayerInteractor: Hit {hit.collider.name} at {hit.point}");
        //    if (hit.collider.TryGetComponent<IInteractable>(out var inter))
        //    {
        //        Debug.Log("PlayerInteractor: Interacting with " + hit.collider.name);
        //        inter.Interact(GetComponent<Player>());
        //        return;
        //    }


        //    if (((1 << hit.collider.gameObject.layer) & groundMask) != 0)
        //    {
        //        Debug.Log("PlayerInteractor: Planting at ground");
        //        TryPlantAt(hit.point);
        //    }
        //}
    }

    void TryPlantAt(Vector3 point)
    {
        //var slotIndex = quickSlotInventory.SelectedIndex;
        int slotIndex = quickSlotInventory.FindFirstSeedSlot(seedMap.Keys.ToHashSet());
        if (slotIndex == -1)
        {
            Debug.Log("No seed in quick slots.");
            return;
        }
        var slot = quickSlotInventory.GetSlot(slotIndex);
        if (slot == null || slot.IsEmpty) return;
        Debug.Log($"Trying to plant seed itemId {slot.itemId} from slot {slotIndex}");

        if (!seedMap.TryGetValue(slot.itemId, out var seedDef)) return;


        Collider[] hits = Physics.OverlapSphere(point, 5f); // 반경 3f
        foreach (var h in hits)
        {
            Debug.Log($"PlayerInteractor: Checking hit {h.name}");
            if (h.TryGetComponent<IInteractable>(out var inter))
            {
                Debug.Log("PlayerInteractor: Interacting with " + h.name);
                inter.Interact(GetComponent<Player>());
                return;
            }
            //PlantPlot plot = h.GetComponent<PlantPlot>();
            //if (plot != null)
            //{
            //    plot.seed = seedDef;
            //    plot.Grow(); // PlantPlot 안에 Grow() 같은 메서드 있다고 가정
            //    quickSlotInventory.Consume(slotIndex, 1);
            //    EventBus.UpdateSlot?.Invoke(
            //        slotIndex,
            //        quickSlotInventory.GetSlot(slotIndex).IsEmpty ? string.Empty : "0",
            //        quickSlotInventory.GetSlot(slotIndex).IsEmpty ? string.Empty : quickSlotInventory.GetSlot(slotIndex).quantity.ToString()
            //    );
            //    return;
            //}
        }

        //#if UNITY_EDITOR && !UNITY_ANDROID
        //        var go = Instantiate(plantPlotPrefab, point, Quaternion.identity);
        //        EventBus.RemoveObj.Add(go);
        //        var plot = go.GetComponent<PlantPlot>();
        //        plot.seed = seedDef;
        //#endif
        //#if !UNITY_EDITOR && UNITY_ANDROID
        //        var go = Instantiate(plantPlotPrefab, lastWorldPos, Quaternion.identity);
        //        EventBus.RemoveObj.Add(go);
        //        var plot = go.GetComponent<PlantPlot>();
        //        plot.seed = seedDef;
        //#endif

        // NavMesh가 있다면 근처 NavMesh로 스냅(선택)
        if (NavMesh.SamplePosition(point, out var navHit, 2f, NavMesh.AllAreas))
            point = navHit.position;

        var go = Instantiate(plantPlotPrefab, point + Vector3.forward, Quaternion.identity);
        EventBus.RemoveObj.Add(go);
        var plot = go.GetComponent<PlantPlot>();
        plot.seed = seedDef;

        quickSlotInventory.Consume(slotIndex, 1);
        EventBus.UpdateSlot?.Invoke(slotIndex, quickSlotInventory.GetSlot(slotIndex).IsEmpty ? string.Empty : "0", quickSlotInventory.GetSlot(slotIndex).IsEmpty ? string.Empty : quickSlotInventory.GetSlot(slotIndex).quantity.ToString());
    }
}