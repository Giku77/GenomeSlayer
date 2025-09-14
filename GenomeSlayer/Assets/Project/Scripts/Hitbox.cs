using System.Collections.Generic;
using UnityEngine;

public class Hitbox : MonoBehaviour
{
    public int damage = 10;
    public LayerMask targetLayers;
    public int maxTargetsPerSwing = 999;

    private bool active;
    private readonly HashSet<Entity> hitEntities = new();
    private int hitCountThisSwing;

    public void Open()
    {
        active = true;
        hitEntities.Clear();
        hitCountThisSwing = 0;
    }

    public void Close() => active = false;

    private void OnTriggerEnter(Collider other)
    {
        //if (!active) return;
        Debug.Log("Hitbox OnTriggerEnter: " + other.name);
        if (((1 << other.gameObject.layer) & targetLayers) == 0) return;

        if (!other.TryGetComponent<Entity>(out var e))
            e = other.GetComponentInParent<Entity>();
        if (e == null || e.isDead) return;

        if (!hitEntities.Add(e)) return;

        e.OnDamage(damage);
        hitCountThisSwing++;

        if (hitCountThisSwing >= maxTargetsPerSwing) active = false;
    }
}
