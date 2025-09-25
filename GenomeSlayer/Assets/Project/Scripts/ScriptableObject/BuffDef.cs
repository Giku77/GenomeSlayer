using UnityEngine;

[CreateAssetMenu(fileName = "BuffDef", menuName = "Scriptable Objects/BuffDef")]
public class BuffDef : ScriptableObject
{
    public BuffIds id;                    
    public string displayName;
    public float duration = 0f;       // 0 = Áö¼Ó
    public int maxStacks = 1;
    public bool isAura = true;

    public LayerMask targets;         

    public float moveSpeedMul = 1f;   
    public float damageAdd = 1f;
    public float AttackSpeed = 1f;
    public float SetHealthSec = 0f;
}
