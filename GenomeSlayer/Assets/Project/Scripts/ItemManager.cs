using NUnit.Framework;
using System.Collections.Generic;
using UnityEngine;

public enum ItemIds
{
    UNKNOWN_ITEM = -1,
    Mace_Durian = 1011001,
    Katana_Pepper = 1011002,
    Bowling_Coconut = 1022001,
    Armor_Watermelon = 1032001,
    Water = 1041001,
    Earthy_Fertilizer = 1041002,
    Durian_Seed = 1051001,
    Watermelon_Seed = 1051002,
    Pepper_Seed = 1051003,
    Coconut_Seed = 1051004,
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
        var weapon = DataTableManger.ItemTable.GetItem((int)ItemIds.Mace_Durian);
        player.quickSlotInventory.AddItem((int)ItemIds.Mace_Durian, 1, weapon.itemDurability, weapon.itemQuantity);
        EventBus.UpdateSlot?.Invoke(player.quickSlotInventory.SelectedIndex, DataTableManger.ItemTable.GetItem((int)ItemIds.Mace_Durian).itemName, player.quickSlotInventory.GetSlotCount().ToString());
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
        AddItemData(s, (int)ItemIds.Durian_Seed);
    }

    public void SpawnItem(Vector3 position)
    {
        position.y += 0.3f;
        position.x += Random.Range(-1f, 1f);
        position.z += Random.Range(-1f, 1f);
        var i = Instantiate(ItemPrefab, position, Quaternion.identity);
        AddItemData(i, (int)ItemIds.Earthy_Fertilizer);
    }

}
