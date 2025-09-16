using NUnit.Framework;
using System.Collections.Generic;
using UnityEngine;

public enum ItemIds
{
    UNKNOWN_ITEM = -1,
    Mace_Durian = 1010001,
    Katana_Pepper = 1010002,
    Bowling_Coconut = 1020003,
    Armor_Watermelon = 1030004,
    Water = 1110001,
    Earthy_Fertilizer = 1110002,
    Durian_Seed = 1120001,
    Watermelon_Seed = 1120002,
    Pepper_Seed = 1120003,
    Coconut_Seed = 1120004,
}


public class ItemManager : MonoBehaviour
{
    public GameObject SeedPrefab;
    public GameObject ItemPrefab;
    private List<ItemData> activeItems = new List<ItemData>();

    private void Awake()
    {
        EventBus.EnemyDropSeed += SpawnItems;
        EventBus.RaiseFruitHarvested += AddEquipItem;
    }

    private void AddEquipItem()
    {
        var player = GameObject.FindGameObjectWithTag("Player").GetComponent<Player>();
        var weapon = DataTableManger.EquipmentTable.GetItem((int)ItemIds.Mace_Durian);
        player.quickSlotInventory.AddItem((int)ItemIds.Mace_Durian, 1, weapon.equipDurability, weapon.equipQuantity);
        EventBus.UpdateSlot?.Invoke(player.quickSlotInventory.SelectedIndex, DataTableManger.EquipmentTable.GetItem((int)ItemIds.Mace_Durian).equipName, player.quickSlotInventory.GetSlotCount().ToString());
    }

    private void AddItemData(GameObject go, int id)
    {
        var i = go.GetComponent<Item>();
        i.SetItemData(DataTableManger.ItemTable.GetItem(id));
        activeItems.Add(DataTableManger.ItemTable.GetItem(id));
    }

    public void SpawnItems(Vector3 position)
    {
        SpawnSeed(position);
        SpawnItem(position);
    }

    public void SpawnSeed(Vector3 position)
    {
        position.y += 1f;
        var s = Instantiate(SeedPrefab, position, Quaternion.identity);
        Destroy(s, 10f);
        EventBus.RemoveObj.Add(s);
        AddItemData(s, (int)ItemIds.Durian_Seed);
    }

    public void SpawnItem(Vector3 position)
    {
        position.y += 0.3f;
        position.x += Random.Range(-1f, 1f);
        position.z += Random.Range(-1f, 1f);
        var i = Instantiate(ItemPrefab, position, Quaternion.identity);
        Destroy(i, 10f);
        EventBus.RemoveObj.Add(i);
        AddItemData(i, (int)ItemIds.Earthy_Fertilizer);
    }

}
