using UnityEngine;

public class Item : MonoBehaviour
{
    public class ItemData
    {
        public string itemName;
        public int itemID;
        public string description;
        public Sprite icon;
    }
    public ItemData itemData;

    private void OnCollisionEnter(Collision collision)
    {
        if (collision.gameObject.CompareTag("Player"))
        {
          Debug.Log("Item collected by Player");
          Destroy(gameObject);
        }
    }
}
