using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TreeEntity : MonoBehaviour
{
    public TreeDef treeDef;

    [Header("Fruit")]
    private GameObject fruitPrefab;
    private Transform fruitSocket;      // 열매 붙일 위치(빈 자식)
    public float pairRadius = 4f;      // 서로 붙어있다고 보는 거리
    public float unpairRadius = 5f;    // 떨어졌다고 보는 거리(히스테리시스)
    public LayerMask treeLayer;

    private TreeEntity partner;
    private BuffEmitter buffEmitter;
    private GameObject fruitInstance;
    private bool hasFruit => fruitInstance != null;

    private void Awake()
    {
        buffEmitter = GetComponent<BuffEmitter>();
        fruitPrefab = treeDef.Fruitprefab;
    }

    private void OnEnable()
    {
        fruitSocket = GetComponentsInChildren<Transform>()[5];
        StartCoroutine(CheckNeighborLoop());
    }

    private IEnumerator CheckNeighborLoop()
    {
        var hits = new Collider[8];
        while (true)
        {
     
            if (partner != null)
            {
                float d = Vector3.Distance(transform.position, partner.transform.position);
                if (d > unpairRadius)
                {

                    partner.NotifyUnpaired(this);
                    NotifyUnpaired(partner);
                    partner = null;
                }
            }
            else
            {
                int count = Physics.OverlapSphereNonAlloc(transform.position, pairRadius, hits, treeLayer);
                TreeEntity best = null;
                float bestD = Mathf.Infinity;

                for (int i = 0; i < count; i++)
                {
                    if (hits[i] == null) continue;
                    if (hits[i].attachedRigidbody && hits[i].attachedRigidbody.gameObject == gameObject) continue;

                    if (!hits[i].TryGetComponent<TreeEntity>(out var other))
                        other = hits[i].GetComponentInParent<TreeEntity>();
                    if (other == null || other == this) continue;

                    if (other.partner != null && other.partner != this) continue;

                    float d = (other.transform.position - transform.position).sqrMagnitude;
                    if (d < bestD)
                    {
                        bestD = d;
                        best = other;
                    }
                }

                if (best != null)
                {
                    buffEmitter.SetEnabled(false);
                    best.buffEmitter.SetEnabled(false);
                    partner = best;
                    best.partner = this;

                    SpawnFruitIfNeeded();
                    best.SpawnFruitIfNeeded();
                }
                else
                {
                    buffEmitter.SetEnabled(true);
                }
            }

            yield return new WaitForSeconds(0.25f); // 빈도 조절
        }
    }

    private void SpawnFruitIfNeeded()
    {
        if (hasFruit || fruitPrefab == null || fruitSocket == null) return;
        fruitInstance = Instantiate(fruitPrefab, fruitSocket.position, fruitSocket.rotation, fruitSocket);
     
        if (fruitInstance.TryGetComponent<Fruit>(out var f))
            f.ownerTree = this;
    }

    public void NotifyUnpaired(TreeEntity other)
    {
        if (fruitInstance) Destroy(fruitInstance);
        fruitInstance = null;
        if (partner == other) partner = null;
    }

    public void OnFruitHarvested()
    {
        if (fruitInstance) Destroy(fruitInstance);
        Destroy(gameObject);
        buffEmitter.SetEnabled(false);
        fruitInstance = null;
        EventBus.RaiseFruitHarvested();

        // EventBus.RaiseFruitHarvested(transform.position);
    }

    private void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.green; Gizmos.DrawWireSphere(transform.position, pairRadius);
        Gizmos.color = Color.red; Gizmos.DrawWireSphere(transform.position, unpairRadius);
    }
}
