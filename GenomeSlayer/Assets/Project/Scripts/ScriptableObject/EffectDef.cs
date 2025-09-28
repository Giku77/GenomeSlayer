using UnityEngine;

[CreateAssetMenu(menuName = "VFX/EffectDef")]
public class EffectDef : ScriptableObject
{
    public string id;                 
    public GameObject prefab;
    public float defaultLife = 1.2f;  // 파티클 자동 회수 시간
    public bool attachToParent = false;
}
