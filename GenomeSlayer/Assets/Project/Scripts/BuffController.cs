using System.Collections.Generic;
using UnityEngine;

public class BuffController : MonoBehaviour
{
    class BuffInstance
    {
        public BuffDef def;
        public Object issuer;   
        public int stacks;
        public float expireAt;  
    }

    private readonly List<BuffInstance> active = new();

    public float MoveSpeedMul { get; private set; } = 1f;
    public float DamageAdd { get; private set; } = 1f;
    public float AttackSpeed { get; private set; } = 1f;
    public float SetHealthSec { get; private set; } = 0f;

    private void Update()
    {
        float now = Time.time;
        bool dirty = false;
        for (int i = active.Count - 1; i >= 0; --i)
        {
            var bi = active[i];
            if (bi.def.duration > 0f && now >= bi.expireAt)
            {
                active.RemoveAt(i);
                dirty = true;
            }
        }
        if (dirty) Recalc();
    }

    public void ClearAll()
    {
        if (active.Count == 0) return;
        active.Clear();
        Recalc();
    }

    private void OnDisable()
    {
        ClearAll();
    }

    public void AddOrRefresh(BuffDef def, Object issuer, int stacks = 1)
    {
        var found = active.Find(b => b.def == def && b.issuer == issuer);
        if (found == null)
        {
            found = new BuffInstance { def = def, issuer = issuer, stacks = 0 };
            active.Add(found);
        }
        found.stacks = Mathf.Clamp(found.stacks + stacks, 1, def.maxStacks);
        if (def.duration > 0f) found.expireAt = Time.time + def.duration;
        Recalc();
    }

    public void RemoveByIssuer(BuffDef def, Object issuer)
    {
        int idx = active.FindIndex(b => b.def == def && b.issuer == issuer);
        if (idx >= 0) { active.RemoveAt(idx); Recalc(); }
    }

    private void Recalc()
    {
        MoveSpeedMul = 1f; DamageAdd = 1f; AttackSpeed = 1f; SetHealthSec = 0f;

        foreach (var bi in active)
        {
            var d = bi.def;

            for (int s = 0; s < bi.stacks; s++)
            {
                MoveSpeedMul *= d.moveSpeedMul;
                DamageAdd *= d.damageAdd;
                AttackSpeed *= d.AttackSpeed;
                SetHealthSec += d.SetHealthSec;
            }
        }
    }
}
