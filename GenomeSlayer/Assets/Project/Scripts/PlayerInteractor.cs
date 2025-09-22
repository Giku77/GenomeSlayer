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
    public GameObject treePrefab;
    public SeedDef[] seedDefs;               
    private Dictionary<int, SeedDef> seedMap;

    public bool IsInteracting { get; set; } = false;
    public bool IsHarv { get; set; } = false;
    public Vector3 lastWorldPos { get; set; }

    private QuickSlotInventory quickSlotInventory;
    private EquipItem equipItem;

    void Start()
    {
        equipItem = GetComponent<EquipItem>();
        quickSlotInventory = GetComponent<Player>().quickSlotInventory;
//#if UNITY_EDITOR
//        if (seedDefs.Length != DataTableManger.ItemTable.GetItemCount() - 3)
//        {
//            seedDefs = new SeedDef[DataTableManger.ItemTable.GetItemCount() - 3];
//            for(int i = 0; i < seedDefs.Length; i++)
//            {
//                var item = DataTableManger.ItemTable.GetAllItems()[i + 3];
//                if(item.itemType == 2) // Seed type
//                {
//                    var seedDef = ScriptableObjectCreator.CreateAssetAt<SeedDef>($"Assets/Project/Scripts/ScriptableObject", $"Seed_{item.itemID}");

//                    seedDef.itemId = item.itemID;
//                    seedDef.treePrefab = treePrefab;

//                    UnityEditor.EditorUtility.SetDirty(seedDef);
//                    UnityEditor.AssetDatabase.SaveAssets();
//                    seedDefs[i] = seedDef;
//                }
//            }
//        }
//#endif
        seedMap = new();
        foreach (var s in seedDefs) seedMap[s.itemId] = s;
    }

    void Update()
    {
#if UNITY_ANDROID
        if (!IsInteracting && !IsHarv) return;

        bool doHarv = IsHarv;
        bool doInteract = IsInteracting;

        IsHarv = false;
        IsInteracting = false;

        Vector3 point = lastWorldPos == Vector3.zero ? transform.position : lastWorldPos;

        if (doHarv)
        {
            TryHarvest(point);
            return;
        }
        if (doInteract)
        {
            TryPlantOrInteract(point);
            return;
        }
        return;
#endif
    }

    void TryHarvest(Vector3 point)
    {
        var hits = Physics.OverlapSphere(point, 5f);
        foreach (var h in hits)
        {
            if (h.TryGetComponent<Fruit>(out var fruit))
            {
                fruit.Interact(GetComponent<Player>());
                return;
            }
        }
    }

    void TryPlantOrInteract(Vector3 point)
    {
        int slotIndex = equipItem.SelectedIndex;
        Debug.Log($"Selected slot index: {slotIndex}");
        if (slotIndex == -1) return;
        var slot = quickSlotInventory.GetSlot(slotIndex);
        if (slot == null || slot.IsEmpty) return;
        Debug.Log($"Selected slot itemId: {slot.itemId}, quantity: {slot.quantity}");
        int selectedId = quickSlotInventory.GetSlot(slotIndex).itemId;
        if (selectedId == -1) return;
        Debug.Log($"Selected item ID: {selectedId}");
        int itemType = DataTableManger.ItemTable.GetItem((int)selectedId).itemType;

        var hits = Physics.OverlapSphere(point, 5f);
        foreach (var h in hits)
        {
            if (itemType == 1) 
            {
                if (h.TryGetComponent<PlantPlot>(out var inter))
                {
                    inter.Interact(GetComponent<Player>());
                    return;
                }
            }
        }

        if (!seedMap.TryGetValue(slot.itemId, out var seedDef)) return;

        if (NavMesh.SamplePosition(point, out var navHit, 2f, NavMesh.AllAreas))
            point = navHit.position;

        var go = Instantiate(plantPlotPrefab, point + Vector3.forward, Quaternion.identity);
        EventBus.RemoveObj.Add(go);
        var plot = go.GetComponent<PlantPlot>();
        plot.seed = seedDef;

        quickSlotInventory.Consume(slotIndex, 1);
        EventBus.UpdateSlot?.Invoke(
            slotIndex,
            slot.IsEmpty ? string.Empty : "0",
            slot.IsEmpty ? string.Empty : slot.quantity.ToString(),
            -1
        );
    }



    public void TryPlantAt(Vector3 point)
    {
        //var slotIndex = quickSlotInventory.SelectedIndex;
        var slotIndex = equipItem.GetSelectedIndex();
        if (slotIndex == -1) return;
        var seletedId = quickSlotInventory.GetSlot(slotIndex).itemId;
        if (seletedId == -1) return;
        var checkSeed = DataTableManger.ItemTable.GetItem((int)seletedId).itemType;

   
        //int slotIndex = quickSlotInventory.FindFirstSeedSlot(seedMap.Keys.ToHashSet());
        //if (slotIndex == -1)
        //{
        //    Debug.Log("No seed in quick slots.");
        //    return;
        //}
        var slot = quickSlotInventory.GetSlot(slotIndex);
        if (slot == null || slot.IsEmpty) return;

        Collider[] hits = Physics.OverlapSphere(point, 5f); // 반경 3f
        foreach (var h in hits)
        {
            Debug.Log($"PlayerInteractor: Checking hit {h.name}");

            if (checkSeed == 1 && !IsHarv)
            {
                if (h.TryGetComponent<PlantPlot>(out var inter))
                {
                    Debug.Log("PlayerInteractor: Interacting with " + h.name);
                    inter.Interact(GetComponent<Player>());
                    return;
                }
            }
            if(IsHarv)
            {
                if (h.TryGetComponent<Fruit>(out var fruit))
                {
                    Debug.Log("PlayerInteractor: Harvesting " + h.name);
                    fruit.Interact(GetComponent<Player>());
                    return;
                }
            }
        }

        if (!seedMap.TryGetValue(slot.itemId, out var seedDef)) return;

        // NavMesh가 있다면 근처 NavMesh로 스냅(선택)
        if (NavMesh.SamplePosition(point, out var navHit, 2f, NavMesh.AllAreas))
            point = navHit.position;

        var go = Instantiate(plantPlotPrefab, point + Vector3.forward, Quaternion.identity);
        EventBus.RemoveObj.Add(go);
        var plot = go.GetComponent<PlantPlot>();
        plot.seed = seedDef;



        quickSlotInventory.Consume(slotIndex, 1);
        EventBus.UpdateSlot?.Invoke(slotIndex, quickSlotInventory.GetSlot(slotIndex).IsEmpty ? string.Empty : "0", quickSlotInventory.GetSlot(slotIndex).IsEmpty ? string.Empty : quickSlotInventory.GetSlot(slotIndex).quantity.ToString(), -1);
    }
}