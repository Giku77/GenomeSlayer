using UnityEngine;

[CreateAssetMenu(fileName = "WeaponDef", menuName = "Scriptable Objects/WeaponDef")]
public class WeaponDef : ScriptableObject
{
    public WeaponIds weaponId = WeaponIds.UNKNOWN_WEAPON;
    public int damage = 10;
}
