using System.Collections.Generic;
using UnityEngine;

public class Hitbox : MonoBehaviour
{
    public static Hitbox Active;
    private int damage = 10;
    public WeaponDef weaponDef;
    public LayerMask targetLayers;
    public int maxTargetsPerSwing = 999;

    private bool active;
    public void SetActive(bool v) => active = v;
    private readonly HashSet<Entity> hitEntities = new();
    private int hitCountThisSwing;
    private Collider[] col;

    private void Awake()
    {
        col = GetComponents<Collider>();
        foreach (var s in col)
            s.enabled = false;
    }

    private void Start()
    {
        if (weaponDef != null)
            damage = weaponDef.damage;
    }
    public void Open()
    {
        //if (isStop) return;
        if (weaponDef == null) return;

        Debug.Log("Hitbox Open: " + weaponDef.weaponId + " / " + weaponDef.kind);

        if (weaponDef.kind == WeaponKind.Projectile || weaponDef.kind == WeaponKind.ThrownReturn)
        {
            FireProjectile();
            return;
        }
        Active = this;                  
        active = true;
        hitEntities.Clear();
        hitCountThisSwing = 0;
        foreach (var s in col)
            s.enabled = true;
        float addDmg = 0;
        switch (weaponDef.weaponId)
        {
            case WeaponIds.Mace_Durian:
                addDmg = GameObject.FindGameObjectWithTag("Ges").GetComponent<StateManager>().GetUpgradeStatAmount((int)GenomIds.MaceDurianAttackUp);
                break;
            case WeaponIds.Katana_Pepper:
                addDmg = GameObject.FindGameObjectWithTag("Ges").GetComponent<StateManager>().GetUpgradeStatAmount((int)GenomIds.KatanaPepperAttackUp);
                break;
            default:
                addDmg = GameObject.FindGameObjectWithTag("Ges").GetComponent<StateManager>().GetUpgradeStatAmount((int)GenomIds.PlayerAttackUp);
                break;
        }
        damage += (damage *(int)addDmg);
    }

    private void FireProjectile()
    {
        Debug.Log("FireProjectile");
        Transform muzzle = transform;

        var player = GameObject.FindGameObjectWithTag("Player").GetComponent<Player>();
        var dir = Vector3.ProjectOnPlane(player.transform.forward, Vector3.up).normalized;
        if (dir.sqrMagnitude < 0.0001f) dir = Vector3.forward;

        //var fwdFlat = Vector3.ProjectOnPlane(muzzle.forward, Vector3.up).normalized;

        const float spawnForward = 0.35f;
        //const float spawnUp = 1f;
        var spawnPos = player.transform.position + dir * spawnForward;

        var rot = Quaternion.LookRotation(dir, Vector3.up);

        var proj = Instantiate(weaponDef.projectilePrefab, spawnPos, rot);
        //var proj = Instantiate(weaponDef.projectilePrefab, muzzle.position, muzzle.rotation);
        proj.Init(owner: GameObject.FindGameObjectWithTag("Player").GetComponent<Player>(),
                  def: weaponDef,
                  targetLayers: targetLayers, dir: dir);
        var e = GameObject.FindGameObjectWithTag("Player").GetComponent<EquipItem>();
        if (e.currentWeapon != null)
            e.currentWeapon.SetActive(false);
    }

    public void Close()
    {
        if (Active == this) Active = null;
        active = false;
        foreach (var s in col)
            s.enabled = false;
    }

    private void OnTriggerEnter(Collider other)
    {
        Debug.Log("active : " + active);
        if (Active != this) return;
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


        if (other.GetComponent<Enemy>())
            EventBus.AttackDur?.Invoke();

        if (hitCountThisSwing >= maxTargetsPerSwing) Close();
        //hitEntities.Clear();
        //active = false;
    }
}
