using UnityEngine;

public class Item : MonoBehaviour
{
    private void OnCollisionEnter(Collision collision)
    {
        if (collision.gameObject.CompareTag("Player"))
        {
          Debug.Log("Item collected by Player");
          Destroy(gameObject);
        }
    }
}
