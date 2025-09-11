using System.Collections.Generic;
using UnityEngine;

public class Hitbox : MonoBehaviour
{
    public int damage = 10;
    public LayerMask targetLayers;
    private bool active;
    private readonly HashSet<Collider> hitSet = new();

    public void Open()
    {
        active = true;
        hitSet.Clear();
    }
    public void Close() => active = false;

    private void OnTriggerEnter(Collider other)
    {
        if (!active) return;
        if (((1 << other.gameObject.layer) & targetLayers) == 0) return;
        if (!hitSet.Add(other)) return;

        Debug.Log($"Hitbox OnTriggerEnter {other.name}");
        if (other.TryGetComponent<Entity>(out var d))
            d.OnDamage(damage);
    }
}