using NUnit.Framework;
using System.Collections.Generic;
using UnityEngine;

public class ItemManager : MonoBehaviour
{
    public GameObject SeedPrefab;
    public GameObject ItemPrefab;
    private List<GameObject> activeItems = new List<GameObject>();

    private void Awake()
    {
        EventBus.EnemyDropSeed += SpawnItems;
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
        activeItems.Add(s);
    }

    public void SpawnItem(Vector3 position)
    {
        position.y += 0.3f;
        position.x += Random.Range(-1f, 1f);
        position.z += Random.Range(-1f, 1f);
        var i = Instantiate(ItemPrefab, position, Quaternion.identity);
        activeItems.Add(i);
    }

    private void Update()
    {
        foreach (var item in activeItems)
        {
            if (item == null) continue;
            item.transform.Rotate(Vector3.up, 50 * Time.deltaTime);
        }
    }
}
