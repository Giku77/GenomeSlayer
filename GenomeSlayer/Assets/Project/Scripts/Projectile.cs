using System.Collections.Generic;
using UnityEngine;

//[RequireComponent(typeof(Collider))]
public class Projectile : MonoBehaviour
{
    Entity owner;
    WeaponDef def;
    LayerMask targetLayers;

    readonly HashSet<Entity> hitSet = new();
    int pierceCount;
    float t;
    Vector3 outDir;
    bool returning;

    public void Init(Entity owner, WeaponDef def, LayerMask targetLayers, Vector3 dir)
    {
        this.owner = owner;
        this.def = def;
        this.targetLayers = targetLayers;
        t = 0f;
        returning = false;
        hitSet.Clear();
        pierceCount = 0;

        //outDir = owner.transform.forward.normalized;
        //outDir = muzzle.forward.normalized;
        outDir = dir.normalized;
        transform.rotation = Quaternion.LookRotation(outDir, Vector3.up);
        //transform.forward = outDir;

        var col = GetComponentInChildren<Collider>();
        var ownerCols = owner.GetComponentsInChildren<Collider>();
        foreach (var oc in ownerCols) Physics.IgnoreCollision(col, oc, true);
    }

    void Update()
    {
        float dt = Time.deltaTime;

        if (!returning)
        {
            if (def == null) return;
            transform.position += outDir * def.projectileSpeed * dt;
            t += dt;
            if (t >= def.maxFlightTime) returning = true;
        }
        else
        {
            Vector3 toOwner = (owner.transform.position - transform.position);
            float dist = toOwner.magnitude;
            if (dist < 0.5f)
            {
                var e = GameObject.FindGameObjectWithTag("Player").GetComponent<EquipItem>();
                if (e.currentWeapon != null)
                    e.currentWeapon.SetActive(true);
                Destroy(gameObject);
                return;
            }
            Vector3 dir = toOwner / Mathf.Max(dist, 0.001f);
            transform.position += dir * def.projectileSpeed * dt;
            transform.forward = dir; 
        }
    }


    void OnTriggerEnter(Collider other)
    {
        if (((1 << other.gameObject.layer) & targetLayers.value) == 0) return;

        if (!other.TryGetComponent<Entity>(out var e))
            e = other.GetComponentInParent<Entity>();
        if (e == null || e == owner || e.isDead) return;

        if (!hitSet.Add(e)) return;

        Haptics.Light(); // 선택
        var addDmg = GameObject.FindGameObjectWithTag("Ges").GetComponent<StateManager>().GetUpgradeStatAmount((int)GenomIds.BowlingCoconutAttackUp);
        var damage = def.damage + (def.damage * (int)addDmg);
        e.OnDamage(damage);
        pierceCount++;

        if (def.kind == WeaponKind.ThrownReturn)
        {
            // 부메랑 느낌: 맞자마자 돌아올 수도 있음(선택)
            // returning = true;
        }

        if (pierceCount >= def.maxPierce)
        {
            if (!returning) returning = true;        
            else Destroy(gameObject);              
        }
        if (other.GetComponent<Enemy>())
            EventBus.AttackDur?.Invoke();
    }

    void OnDestroy()
    {
        // 충돌 무시 해제 등 필요시 정리
    }
}
