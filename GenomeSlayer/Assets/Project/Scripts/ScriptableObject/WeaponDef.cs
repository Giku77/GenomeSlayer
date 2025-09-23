using UnityEngine;
public enum WeaponKind { Melee, ThrownReturn, Projectile }

[CreateAssetMenu(fileName = "WeaponDef", menuName = "Scriptable Objects/WeaponDef")]
public class WeaponDef : ScriptableObject
{
    public WeaponIds weaponId = WeaponIds.UNKNOWN_WEAPON;
    public WeaponKind kind;
    public int damage = 10;

    public Projectile projectilePrefab;
    public float projectileSpeed = 12f;
    public float maxFlightTime = 0.7f; // 나갈 시간
    public int maxPierce = 3;          // 관통 수
}
