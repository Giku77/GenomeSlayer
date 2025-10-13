using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(SphereCollider))]
public class BuffEmitter : MonoBehaviour
{
    public BuffDef buff;           
    public float radius = 5f;       
    public bool enableWhenPaired = false;  // 1그루일 때만 on/off

    [Header("FX Limit")]
    [SerializeField, Range(0, 32)] int maxEnemyFx = 5; // 이펙트는 최대 5마리만

    public SphereCollider trig;
    private readonly HashSet<BuffController> inside = new();
    private readonly Dictionary<BuffController, Coroutine> playerCos = new();
    private readonly Dictionary<BuffController, Coroutine> enemyCos = new();

    private void Awake()
    {
        //trig = GetComponent<SphereCollider>();
        trig.isTrigger = true;
        trig.radius = radius;
        gameObject.layer = LayerMask.NameToLayer("Ignore Raycast");
        //gameObject.layer = gameObject.layer; 
    }

    private IEnumerator buffPlayerEffect(Player p)
    {
        var em = EffectManager.I;
        while (true)
        {
            em.Play("Buff", p.transform.position, Quaternion.identity, parent: p.transform, life: 1f);
            yield return new WaitForSeconds(1f);
        }
    }

    private IEnumerator debuffEnemyEffect(Enemy e)
    {
        var em = EffectManager.I;
        while (true)
        {
            em.Play("DeBuff", e.transform.position, Quaternion.identity, parent: e.transform, life: 1f);
            yield return new WaitForSeconds(1f);
        }
    }


    private void OnTriggerEnter(Collider other)
    {
        //Debug.Log($"[BuffEmitter] {other.name} enter {buff.displayName}");
        if (((1 << other.gameObject.layer) & buff.targets) == 0) return;
        if (!other.TryGetComponent<BuffController>(out var bc))
            bc = other.GetComponentInParent<BuffController>();
        if (!bc) return;

        if (bc.TryGetComponent(out Player p) && !playerCos.ContainsKey(bc))
            playerCos[bc] = StartCoroutine(buffPlayerEffect(p));

        if (bc.TryGetComponent(out Enemy e) && !enemyCos.ContainsKey(bc) && enemyCos.Count < maxEnemyFx)
            enemyCos[bc] = StartCoroutine(debuffEnemyEffect(e));

        bc.AddOrRefresh(buff, this, 0);

        inside.Add(bc);

        //if (inside.Add(bc))
        //    bc.AddOrRefresh(buff, this, 1);
    }

    private void OnTriggerExit(Collider other)
    {
        if (!other.TryGetComponent<BuffController>(out var bc))
            bc = other.GetComponentInParent<BuffController>();
        if (!bc) return;

        if (playerCos.TryGetValue(bc, out var pco) && other.TryGetComponent<Player>(out var p))
        {
            //Debug.Log($"[BuffEmitter] {other.name} exit {buff.displayName}");
            if (pco != null) StopCoroutine(pco);
            playerCos.Remove(bc);
            if (inside.Remove(bc))
                bc.RemoveByIssuer(buff, this);
        }
        if (enemyCos.TryGetValue(bc, out var eco))
        {
            if (eco != null) StopCoroutine(eco);
            enemyCos.Remove(bc);
            if (inside.Remove(bc))
                bc.RemoveByIssuer(buff, this);
        }
    }

    private void OnDestroy()
    {
        foreach (var bc in inside)
            bc.RemoveByIssuer(buff, this);
        inside.Clear();
    }

    // 나무 2그루 붙었을 때 오라 on/off 규칙을 TreeEntity에서 호출
    public void SetEnabled(bool on)
    {
        if (trig.enabled == on) return;
        trig.enabled = on;
        if (!on)
        {
            // 범위가 꺼질 때 내부 모두 해제
            foreach (var bc in inside)
                bc.RemoveByIssuer(buff, this);
            inside.Clear();
        }
    }
}
