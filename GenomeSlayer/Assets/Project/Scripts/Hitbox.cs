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
    private BuffController buffController;

    private int baseDamage;         
    private int swingDamage;

    private void Awake()
    {
        col = GetComponents<Collider>();
        buffController = GetComponentInParent<BuffController>();
        foreach (var s in col)
            s.enabled = false;
    }

    private void Start()
    {
        baseDamage = weaponDef != null ? weaponDef.damage : damage;
    }

    private float GetPlayerAttackUpPercent(WeaponIds id)
    {
        var sm = GameObject.FindGameObjectWithTag("Ges").GetComponent<StateManager>();
        return id switch
        {
            WeaponIds.Mace_Durian => sm.GetUpgradeStatAmount((int)GenomIds.MaceDurianAttackUp), // 0.2f 형태 권장
            WeaponIds.Katana_Pepper => sm.GetUpgradeStatAmount((int)GenomIds.KatanaPepperAttackUp),
            _ => sm.GetUpgradeStatAmount((int)GenomIds.PlayerAttackUp),
        };
    }

    public void Open()
    {
        //if (isStop) return;
        if (weaponDef == null) return;

        Debug.Log("Hitbox Open: " + weaponDef.weaponId + " / " + weaponDef.kind);

        if (weaponDef.kind == WeaponKind.Projectile || weaponDef.kind == WeaponKind.ThrownReturn)
        {
            AudioManager.I.PlaySFX("Bowling", transform.position);
            FireProjectile();
            return;
        }
        Active = this;                  
        active = true;
        hitEntities.Clear();
        hitCountThisSwing = 0;
        foreach (var s in col)
            s.enabled = true;
        if (weaponDef.weaponId == WeaponIds.Mace_Durian)
        {
            //var owner = GetComponentInParent<Player>().transform;
            //var fwd = Vector3.ProjectOnPlane(owner.forward, Vector3.up).normalized;
            //if (fwd.sqrMagnitude < 0.0001f) fwd = Vector3.forward;

            //var rot = Quaternion.LookRotation(fwd, Vector3.up) * Quaternion.Euler(-60f, 0f, 60f);
            var t = GetComponentInChildren<Transform>();
            var ts = GetComponentsInChildren<Transform>();
            var tD = ts[ts.Length - 1];
            EffectManager.I.Play("DurianSlash", t.position, tD.rotation);
            AudioManager.I.PlaySFX("DurianSlash", tD.position);
            //EffectManager.I.Play("DurianSlash", transform.position, rot);
        }
        if (weaponDef.weaponId == WeaponIds.Katana_Pepper)
        {
            var t = GetComponentInChildren<Transform>();
            EffectManager.I.Play("Katana", t.position, t.rotation, parent : t);
            AudioManager.I.PlaySFX("KatanaSlash", t.position);
        }
        if (weaponDef.weaponId == WeaponIds.UNKNOWN_WEAPON)
        {
            var owner = GetComponentInParent<Player>().transform;
            var fwd = Vector3.ProjectOnPlane(owner.forward, Vector3.up).normalized;
            if (fwd.sqrMagnitude < 0.0001f) fwd = Vector3.forward;

            const float yawFix = -90f;
            var rot = Quaternion.LookRotation(fwd, Vector3.up) * Quaternion.Euler(0f, yawFix, 0f);

            EffectManager.I.Play("Punch", transform.position, rot);
        }

        float upgPct = GetPlayerAttackUpPercent(weaponDef.weaponId);

        float mul = 1f + upgPct;
        if (buffController != null) mul *= buffController.DamageAdd;

        swingDamage = Mathf.RoundToInt(baseDamage * mul);
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
        EffectManager.I.Play("Bowling", spawnPos, rot, parent : proj.transform);
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
        Debug.Log("Hitbox Close");
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

        if (!other.TryGetComponent<Enemy>(out var e))
            e = other.GetComponentInParent<Enemy>();
        if (e == null || e.isDead) return;
        //Debug.Log("Hitbox OnTriggerEnter2: " + other.name);
        if (!hitEntities.Add(e)) return;

        //Debug.Log("Hitbox OnTriggerEnter3: " + other.name);

        Haptics.Light();
        Vector3 hitPoint = other.ClosestPoint(transform.position);
        e.OnDamage(swingDamage, hitPoint);

        var dir = (e.transform.position - hitPoint);
        //dir.y = 1f;
        dir.Normalize();

        e.Knockback(dir, force: 13.5f, duration: 0.18f);

        if (weaponDef.weaponId == WeaponIds.UNKNOWN_WEAPON)
        {
            AudioManager.I.PlaySFX("Hit", hitPoint);
        }
        if (weaponDef.weaponId == WeaponIds.Mace_Durian)
        {
            AudioManager.I.PlaySFX("DurianHit", transform.position);
        }
        if (weaponDef.weaponId == WeaponIds.Katana_Pepper)
        {
            AudioManager.I.PlaySFX("KatanaHit", transform.position);
        }
        if (weaponDef.weaponId == WeaponIds.Bowling_Coconut)
        {
            AudioManager.I.PlaySFX("BowlingHit", transform.position);
        }

        EffectManager.I.Play("Hit", hitPoint, Quaternion.identity);

        //e.OnDamage(swingDamage);
        hitCountThisSwing++;


        if (other.GetComponent<Enemy>())
            EventBus.AttackDur?.Invoke();

        if (hitCountThisSwing >= maxTargetsPerSwing) Close();
        //hitEntities.Clear();
        //active = false;
    }
}
