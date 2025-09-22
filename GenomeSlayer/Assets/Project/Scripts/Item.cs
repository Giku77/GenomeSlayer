using UnityEngine;

public class Item : MonoBehaviour
{
    private ItemData itemData;

    public void SetItemData(ItemData d)
    {
        itemData = d;
    }

    private void OnCollisionEnter(Collision collision)
    {
        if (collision.gameObject.CompareTag("Player"))
        {
            var player = collision.gameObject.GetComponent<Player>();
            if (player != null)
            {
                player.quickSlotInventory.AddItem(itemData.itemID, 1);
                EventBus.UpdateSlot?.Invoke(player.quickSlotInventory.SelectedIndex, itemData.itemName, player.quickSlotInventory.GetSlotCount().ToString(), -1);
            }
            //Debug.Log($"Picked up item: {itemData.itemName}");
            Destroy(gameObject);
        }
    }

    private void Update()
    {
        gameObject.transform.Rotate(Vector3.up, 50 * Time.deltaTime);
    }
}
