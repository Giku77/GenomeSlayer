using System.Collections.Generic;

[System.Serializable]
public class InventorySlot
{
    public int itemId = -1;   
    public int quantity = 0;
    public int durability = 0;

    public bool IsEmpty => itemId == -1;
}

public class QuickSlotInventory
{
    private InventorySlot[] slots;

    public QuickSlotInventory(int size)
    {
        slots = new InventorySlot[size];
        for (int i = 0; i < size; i++)
            slots[i] = new InventorySlot();
    }

    public bool AddItem(int itemId, int amount = 1, int durability = 100)
    {
        for (int i = 0; i < slots.Length; i++)
        {
            if (slots[i].IsEmpty)
            {
                slots[i].itemId = itemId;
                slots[i].quantity = amount;
                slots[i].durability = durability;
                return true;
            }
        }

        return false;
    }

    public void RemoveItem(int slotIndex)
    {
        if (slotIndex >= 0 && slotIndex < slots.Length)
        {
            slots[slotIndex] = new InventorySlot();
        }
    }

    public void Consume(int slotIndex, int c)
    {
        if (slotIndex >= 0 && slotIndex < slots.Length && !slots[slotIndex].IsEmpty)
        {
            slots[slotIndex].quantity -= c;
            if (slots[slotIndex].quantity <= 0)
            {
                RemoveItem(slotIndex);
            }
        }
    }

    public int FindFirstSeedSlot(HashSet<int> seedIds)
    {
        for (int i = 0; i < slots.Length; i++)
        {
            if (!slots[i].IsEmpty && seedIds.Contains(slots[i].itemId))
            {
                return i;
            }
        }
        return -1;
    }

    public int FindSlotWithItem(int itemId)
    {
        for (int i = 0; i < slots.Length; i++)
        {
            if (!slots[i].IsEmpty && slots[i].itemId == itemId)
                return i;
        }
        return -1;
    }


    public InventorySlot GetSlot(int index) => slots[index];
}
