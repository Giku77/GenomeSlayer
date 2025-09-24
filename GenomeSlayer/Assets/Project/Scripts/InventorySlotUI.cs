using NUnit.Framework.Interfaces;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class InventorySlotUI : MonoBehaviour
{
    public TextMeshProUGUI itemName;
    public TextMeshProUGUI itemCount;
    public Slider durSlider;
    public int slotIndex { get; set; }
    private int[] s;

    private Player player => GameObject.FindGameObjectWithTag("Player").GetComponent<Player>();
    private EquipItem equipItem => GameObject.FindGameObjectWithTag("Player").GetComponent<EquipItem>();

    void OnEnable() { EventBus.AttackDur += UpdateWeapondur; }
    void OnDisable() { EventBus.AttackDur -= UpdateWeapondur; }

    public void UpdateWeapondur()
    {
        if (!this) return;
        if (!durSlider) return;
        if (!durSlider.gameObject.activeSelf) return;
        var selected = equipItem ? equipItem.SelectedIndex : -1;
        if (selected != slotIndex) return;

        UpdateDur();
    }

    public void UpdateDur()
    {
        var inventory = player.quickSlotInventory;
        var eId = inventory.GetSlot(slotIndex).itemId;
        //var eNum = DataTableManger.EquipmentTable.GetItem(eId).equipImprovedNum;

        var normalSeed = DataTableManger.EquipmentTable.GetItem(eId).seedID;
        var improvedSeed = DataTableManger.EquipmentTable.GetItem(eId).improvedSeedID;
        var regpressSeed = DataTableManger.EquipmentTable.GetItem(eId).regressedSeedID;

        int[] seeds = new int[] { normalSeed, improvedSeed, regpressSeed };

        if (s == null || s.Length == 0)
            s = PickUniqueInts(4, 0, 4);

        if (durSlider.value > 0)
            durSlider.value -= 1;

        if (durSlider.value <= durSlider.maxValue * 0.75 && s[0] != -1)
        {
            Debug.Log("Adding seed at 75% durability");
            AddSeed(seeds, s[0], inventory);
            s[0] = -1;
        }
        if (durSlider.value <= durSlider.maxValue * 0.5 && s[1] != -1)
        {
            Debug.Log("Adding seed at 50% durability");
            AddSeed(seeds, s[1], inventory);
            s[1] = -1;
        }
        if (durSlider.value <= durSlider.maxValue * 0.25 && s[2] != -1)
        {
            Debug.Log("Adding seed at 25% durability");
            AddSeed(seeds, s[2], inventory);
            s[2] = -1;
        }
        if (durSlider.value <= 0 && s[3] != -1)
        {

            inventory.RemoveItem(slotIndex);
            //EventBus.UpdateSlot?.Invoke(slotIndex, string.Empty, string.Empty, -1);
            durSlider.gameObject.SetActive(false);
            equipItem.UnEquipItem();

            Debug.Log("Adding seed at 0% durability and unequipping item");
            AddSeed(seeds, s[3], inventory);
            s[3] = -1;
            s = null;


            //var itable = DataTableManger.ItemTable;
            //inventory.AddItem(normalSeed, 2);
            //EventBus.UpdateSlot?.Invoke(inventory.SelectedIndex, itable.GetItem(normalSeed).itemName, inventory.GetSlotCount().ToString(), -1);
            //inventory.AddItem(improvedSeed, 1);
            //EventBus.UpdateSlot?.Invoke(inventory.SelectedIndex, itable.GetItem(improvedSeed).itemName, inventory.GetSlotCount().ToString(), -1);
            //inventory.AddItem(regpressSeed, 1);
            //EventBus.UpdateSlot?.Invoke(inventory.SelectedIndex, itable.GetItem(regpressSeed).itemName, inventory.GetSlotCount().ToString(), -1);
        }
    }

    private void AddSeed(int[] seedid, int index, QuickSlotInventory inv)
    {
        var itable = DataTableManger.ItemTable;
        switch (index)
        {
            case 0:
            case 1:
                inv.TryAddItem(seedid[0], 1);
                //EventBus.UpdateSlot?.Invoke(inv.SelectedIndex, itable.GetItem(seedid[0]).itemName, inv.GetSlotCount().ToString(), -1);
                break;
            case 2:
                inv.TryAddItem(seedid[1], 1);
                //EventBus.UpdateSlot?.Invoke(inv.SelectedIndex, itable.GetItem(seedid[1]).itemName, inv.GetSlotCount().ToString(), -1);
                break;
            case 3:
                inv.TryAddItem(seedid[2], 1);
                //EventBus.UpdateSlot?.Invoke(inv.SelectedIndex, itable.GetItem(seedid[2]).itemName, inv.GetSlotCount().ToString(), -1);
                break;
            default:
                break;
        }
    }

    private int[] PickUniqueInts(int count, int minInclusive, int maxExclusive)
    {
        if (maxExclusive - minInclusive < count)
            throw new System.ArgumentException("범위가 너무 좁아요.");

        var set = new HashSet<int>();
        while (set.Count < count)
        {
            set.Add(Random.Range(minInclusive, maxExclusive)); 
        }
        var arr = new int[count];
        set.CopyTo(arr);
        return arr;
    }
}
