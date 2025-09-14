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
                player.quickSlotInventory.AddItem(itemData.itemId, 1, itemData.itemDurability);
            Debug.Log($"Picked up item: {itemData.itemName}");
            Destroy(gameObject);
        }
    }

    private void Update()
    {
        gameObject.transform.Rotate(Vector3.up, 50 * Time.deltaTime);
    }
}
