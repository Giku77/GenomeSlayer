using System;
using System.Collections.Generic;

[System.Serializable]
public class InventorySlot
{
    public int itemId = -1;
    public int quantity = 0;
    public int durability = 0;
    public bool IsEmpty => itemId == -1;

    public void Clear()
    {
        itemId = -1;
        quantity = 0;
        durability = 0;
    }
}

public class QuickSlotInventory
{
    public event Action<int, InventorySlot> OnSlotChanged; 
    public event Action<int, int> OnSelectedIndexChanged;  
    public event Action OnInventoryChanged;               

    private readonly InventorySlot[] slots;

    private readonly Dictionary<int, int> indexByItem = new();

    public int Size => slots.Length;

    private int selectedIndex = -1;
    public int SelectedIndex
    {
        get => selectedIndex;
        set
        {
            if (value == selectedIndex) return;
            int old = selectedIndex;
            selectedIndex = (value >= 0 && value < slots.Length) ? value : -1;
            OnSelectedIndexChanged?.Invoke(old, selectedIndex);
        }
    }

    public QuickSlotInventory(int size)
    {
        slots = new InventorySlot[size];
        for (int i = 0; i < size; i++)
            slots[i] = new InventorySlot();
    }

    public InventorySlot GetSlot(int index) => slots[index];

    public bool TryGetSlotSafe(int index, out InventorySlot slot)
    {
        if (index >= 0 && index < slots.Length)
        {
            slot = slots[index];
            return true;
        }
        slot = null;
        return false;
    }

    public int GetSlotCount()
    {
        if (SelectedIndex < 0 || SelectedIndex >= slots.Length) return 0;
        return slots[SelectedIndex].quantity;
    }

    private void RaiseSlotChanged(int index)
    {
        OnSlotChanged?.Invoke(index, slots[index]);
        OnInventoryChanged?.Invoke();
    }

    public int FindFirstEmpty()
    {
        for (int i = 0; i < slots.Length; i++)
            if (slots[i].IsEmpty) return i;
        return -1;
    }

    public int FindFirstSeedSlot(HashSet<int> seedIds)
    {
        for (int i = 0; i < slots.Length; i++)
            if (!slots[i].IsEmpty && seedIds.Contains(slots[i].itemId)) return i;
        return -1;
    }

    public int FindSlotWithItem(int itemId)
    {
        return indexByItem.TryGetValue(itemId, out var idx) ? idx : -1;
    }

    public bool TryAddItem(int itemId, int amount = 1, int durability = -1, int maxStack = 99)
    {
        if (amount <= 0) return false;

        if (indexByItem.TryGetValue(itemId, out int idx) && maxStack > 1)
        {
            var s = slots[idx];
            if (s.quantity < maxStack)
            {
                int canAdd = maxStack - s.quantity;
                int add = amount <= canAdd ? amount : canAdd;
                s.quantity += add;

                SelectedIndex = idx;
                RaiseSlotChanged(SelectedIndex);
                return add > 0;
            }
            return false; 
        }

        int empty = FindFirstEmpty();
        if (empty == -1) return false;

        var slot = slots[empty];
        slot.itemId = itemId;
        slot.quantity = amount > maxStack ? maxStack : amount;
        slot.durability = durability;

        indexByItem[itemId] = empty;
        SelectedIndex = empty;

        RaiseSlotChanged(SelectedIndex);
        return true;
    }

    public void RemoveItem(int slotIndex)
    {
        if (slotIndex < 0 || slotIndex >= slots.Length) return;

        var oldId = slots[slotIndex].itemId;
        if (oldId != -1 && indexByItem.TryGetValue(oldId, out var mapped) && mapped == slotIndex)
            indexByItem.Remove(oldId);

        slots[slotIndex].Clear();
        RaiseSlotChanged(slotIndex);
        if (SelectedIndex == slotIndex) SelectedIndex = -1;
    }

    public bool TryConsume(int slotIndex, int count)
    {
        if (count <= 0) return false;

        if (!TryGetSlotSafe(slotIndex, out var s) || s.IsEmpty) return false;

        s.quantity -= count;
        if (s.quantity <= 0)
            RemoveItem(slotIndex);

        RaiseSlotChanged(slotIndex);
        return true;
    }

    public bool TryConsumeSelected(int count)
    {
        if (SelectedIndex < 0) return false;
        return TryConsume(SelectedIndex, count);
    }
}
