using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(SphereCollider))]
public class BuffEmitter : MonoBehaviour
{
    public BuffDef buff;           
    public float radius = 5f;       
    public bool enableWhenPaired = false;  // 1그루일 때만 on/off

    public SphereCollider trig;
    private readonly HashSet<BuffController> inside = new();

    private void Awake()
    {
        //trig = GetComponent<SphereCollider>();
        trig.isTrigger = true;
        trig.radius = radius;
        gameObject.layer = LayerMask.NameToLayer("Ignore Raycast");
        //gameObject.layer = gameObject.layer; 
    }

    private void OnTriggerEnter(Collider other)
    {
        if (((1 << other.gameObject.layer) & buff.targets) == 0) return;
        if (!other.TryGetComponent<BuffController>(out var bc))
            bc = other.GetComponentInParent<BuffController>();
        if (!bc) return;

        if (inside.Add(bc))
            bc.AddOrRefresh(buff, this, 1);
    }

    private void OnTriggerExit(Collider other)
    {
        if (!other.TryGetComponent<BuffController>(out var bc))
            bc = other.GetComponentInParent<BuffController>();
        if (!bc) return;

        if (inside.Remove(bc))
            bc.RemoveByIssuer(buff, this);
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
