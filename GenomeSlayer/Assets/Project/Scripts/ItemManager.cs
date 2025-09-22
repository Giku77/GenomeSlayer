using NUnit.Framework;
using System.Collections.Generic;
using UnityEngine;


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

    private void AddEquipItem(int TreeId, int num)
    {
        var player = GameObject.FindGameObjectWithTag("Player").GetComponent<Player>();
        var itemId = (int)ItemIds.Mace_Durian + (1000 * num);
        var weapon = DataTableManger.EquipmentTable.GetItem(itemId);
        player.quickSlotInventory.AddItem(itemId, 1, weapon.equipDurability, weapon.equipQuantity);
        var item = DataTableManger.EquipmentTable.GetItem(itemId);
        EventBus.UpdateSlot?.Invoke(player.quickSlotInventory.SelectedIndex, item.equipName, player.quickSlotInventory.GetSlotCount().ToString(), item.equipDurability);
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
        AddItemData(s, (int)ItemIds.Mystery_Seed);
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
