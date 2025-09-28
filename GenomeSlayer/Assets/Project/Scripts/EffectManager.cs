using System.Collections.Generic;
using UnityEngine;

public class EffectManager : MonoBehaviour
{
    public static EffectManager I { get; private set; }
    public EffectDef[] defs;
    Dictionary<string, EffectDef> map;
    Dictionary<string, Queue<GameObject>> pools = new();

    void Awake()
    {
        if (I != null) { Destroy(gameObject); return; }
        I = this; DontDestroyOnLoad(gameObject);
        map = new();
        foreach (var d in defs) if (d) map[d.id] = d;
    }

    GameObject Rent(EffectDef d)
    {
        if (!pools.TryGetValue(d.id, out var q)) { q = new(); pools[d.id] = q; }
        if (q.Count > 0) return q.Dequeue();
        return Instantiate(d.prefab);
    }
    void Return(EffectDef d, GameObject go)
    {
        go.SetActive(false);
        go.transform.SetParent(transform);
        pools[d.id].Enqueue(go);
    }

    public GameObject Play(string id, Vector3 pos, Quaternion rot, Transform parent = null, float life = -1f)
    {
        if (!map.TryGetValue(id, out var d) || !d.prefab) return null;
        var go = Rent(d);
        go.transform.SetPositionAndRotation(pos, rot);
        if (d.attachToParent && parent) go.transform.SetParent(parent); else go.transform.SetParent(null);
        go.SetActive(true);

        foreach (var p in go.GetComponentsInChildren<ParticleSystem>()) p.Play(true);

        float t = (life > 0) ? life : d.defaultLife;
        StartCoroutine(ReturnLater(d, go, t));
        return go;
    }

    System.Collections.IEnumerator ReturnLater(EffectDef d, GameObject go, float t)
    {
        yield return new WaitForSeconds(t);
        Return(d, go);
    }
}
