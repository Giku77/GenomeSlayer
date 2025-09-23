using System.Collections;
using UnityEngine;

public enum WeaponStance { None = 0, OneHand = 1, TwoHand = 2 }

public class EquipController : MonoBehaviour
{
    public Animator animator;
    static readonly int HashIsEquip = Animator.StringToHash("IsEquip");
    static readonly int HashStance = Animator.StringToHash("WeaponStance");

    int upperLayer;
    Coroutine fadeCo;

    void Awake()
    {
        upperLayer = animator.GetLayerIndex("Equip Layer");
        if (upperLayer >= 0) animator.SetLayerWeight(upperLayer, 0f);
    }

    public void SetEquipped(WeaponStance stance, float fade = 0.2f)
    {
        bool on = stance != WeaponStance.None;
        animator.SetBool(HashIsEquip, on);
        animator.SetInteger(HashStance, (int)stance);

        if (upperLayer < 0) return;
        if (fadeCo != null) StopCoroutine(fadeCo);
        fadeCo = StartCoroutine(FadeLayerWeight(upperLayer, on ? 1f : 0f, fade));
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
