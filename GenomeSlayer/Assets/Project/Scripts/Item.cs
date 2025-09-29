using UnityEngine;

public class Item : MonoBehaviour
{
    private ItemData itemData;

    public void SetItemData(ItemData d)
    {
        itemData = d;
    }


    private void OnTriggerEnter(Collider collision)
    {
        if (collision.gameObject.CompareTag("Player"))
        {
            var player = collision.gameObject.GetComponent<Player>();
            if (player != null)
            {
                AudioManager.I.PlaySFX("PickUp");
                EffectManager.I.Play("PickUp", player.transform.position, Quaternion.identity, parent : player.transform);
                player.quickSlotInventory.TryAddItem(itemData.itemID, 1);
                //EventBus.UpdateSlot?.Invoke(player.quickSlotInventory.SelectedIndex, itemData.itemName, player.quickSlotInventory.GetSlotCount().ToString(), -1);
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
