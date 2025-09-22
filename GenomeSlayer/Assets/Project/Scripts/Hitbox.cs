using System.Collections.Generic;
using UnityEngine;

public class Hitbox : MonoBehaviour
{
    private int damage = 10;
    public WeaponDef weaponDef;
    public LayerMask targetLayers;
    public int maxTargetsPerSwing = 999;

    private bool active;
    private readonly HashSet<Entity> hitEntities = new();
    private int hitCountThisSwing;

    private void Start()
    {
        if (weaponDef != null)
            damage = weaponDef.damage;
    }
    public void Open()
    {
        //Debug.Log("Hitbox Open");
        active = true;
        hitEntities.Clear();
        hitCountThisSwing = 0;
    }

    public void Close()
    {
        //Debug.Log("Hitbox Close");
        //active = false;
    }

    private void OnTriggerEnter(Collider other)
    {
        //Debug.Log("Hitbox OnTriggerEnter: " + other.name + " / " + active);
        if (!active) return;
        if (((1 << other.gameObject.layer) & targetLayers) == 0) return;

        if (!other.TryGetComponent<Entity>(out var e))
            e = other.GetComponentInParent<Entity>();
        if (e == null || e.isDead) return;
        //Debug.Log("Hitbox OnTriggerEnter2: " + other.name);
        if (!hitEntities.Add(e)) return;

        //Debug.Log("Hitbox OnTriggerEnter3: " + other.name);

        Haptics.Light();
        e.OnDamage(damage);
        hitCountThisSwing++;

        if (hitCountThisSwing >= maxTargetsPerSwing) active = false;
        //hitEntities.Clear();
        if (other.GetComponent<Enemy>())
          EventBus.AttackDur?.Invoke();
        active = false;
    }
}
