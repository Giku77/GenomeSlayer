using System.Collections;
using UnityEngine;

public class EquipController : MonoBehaviour
{
    public Animator animator;
    static readonly int HashIsEquip = Animator.StringToHash("IsEquip");

    int equipLayer;
    Coroutine fadeCo;

    void Awake()
    {
        equipLayer = animator.GetLayerIndex("Equip Layer");
        if (equipLayer >= 0) animator.SetLayerWeight(equipLayer, 0f); 
    }

    public void SetEquipped(bool on, float fade = 0.2f)
    {
        animator.SetBool(HashIsEquip, on); 
        if (equipLayer < 0) return;

        if (fadeCo != null) StopCoroutine(fadeCo);
        fadeCo = StartCoroutine(FadeLayerWeight(equipLayer, on ? 1f : 0f, fade));
    }

    IEnumerator FadeLayerWeight(int layer, float target, float dur)
    {
        float start = animator.GetLayerWeight(layer);
        float t = 0f;
        while (t < dur)
        {
            t += Time.deltaTime;
            animator.SetLayerWeight(layer, Mathf.Lerp(start, target, t / dur));
            yield return null;
        }
        animator.SetLayerWeight(layer, target);
    }
}
