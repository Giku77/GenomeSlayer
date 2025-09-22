using UnityEngine;
using System;
using NUnit.Framework;
using System.Collections.Generic;

public static class EventBus
{
    public static List<GameObject> RemoveObj = new List<GameObject>();

    //추후 이벤트형식으로 변경해서 안정성 챙길것
    public static Action<GameObject> EnemyDied;
    public static Action<Vector3> EnemyDropSeed;
    public static Action<int> WaveStarted;
    public static Action AttackDur;
    //public static Action<TreeEntity> TreeGrown;
    public static Action<int> PointsChanged;
    public static Action<int, string, string, int> UpdateSlot;
    public static Action<int> UpdateSelected;
    public static Action<int, int> RaiseFruitHarvested;

    public static void WireToEventBus(QuickSlotInventory inv, Func<int, string> getItemName)
    {
        inv.OnSlotChanged += (idx, slot) =>
        {
            string name = slot.IsEmpty ? string.Empty : getItemName(slot.itemId);
            string count = slot.IsEmpty ? string.Empty : slot.quantity.ToString();
            int dur = slot.IsEmpty ? -1 : slot.durability;
            UpdateSlot?.Invoke(idx, name, count, dur);
        };

        inv.OnSelectedIndexChanged += (oldIdx, newIdx) =>
        {
            UpdateSelected?.Invoke(newIdx);
        };
    }
}
