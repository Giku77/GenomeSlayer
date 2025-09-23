using UnityEngine;

public class PlayerAttackAnimEvents : MonoBehaviour
{
    public Hitbox righthitbox;
    public Hitbox lefthitbox;

    public Hitbox DurianAttackbox;
    public Hitbox PepperAttackbox;
    public Hitbox CoconutAttackbox;
    public void AEr_AttackStart()
    {
        if (lefthitbox.currentWeaponId != WeaponIds.UNKNOWN_WEAPON) return;
        Debug.Log("Right Attack Start");
        righthitbox?.Open();
    }
    public void AEr_AttackEnd()
    {
        if (lefthitbox.currentWeaponId != WeaponIds.UNKNOWN_WEAPON) return;
        righthitbox?.Close();
    }

    public void AEl_AttackStart()
    {
        if (lefthitbox.currentWeaponId != WeaponIds.UNKNOWN_WEAPON) return;
        Debug.Log("Left Attack Start");
        lefthitbox?.Open();
    }
    public void AEl_AttackEnd()
    {
        if (lefthitbox.currentWeaponId != WeaponIds.UNKNOWN_WEAPON) return;
        lefthitbox?.Close();
    }
    public void AE_DurianAttackStart()
    {
        if (lefthitbox.currentWeaponId != WeaponIds.Mace_Durian) return;
        DurianAttackbox?.Open();
    }
    public void AE_DurianAttackEnd()
    {
        if (lefthitbox.currentWeaponId != WeaponIds.Mace_Durian) return;
        DurianAttackbox?.Close();
    }
    public void AE_PepperAttackStart()
    {
        if (lefthitbox.currentWeaponId != WeaponIds.Katana_Pepper) return;
        Debug.Log("Pepper Attack Start");
        PepperAttackbox?.Open();
    }
    public void AE_PepperAttackEnd()
    {
        if (lefthitbox.currentWeaponId != WeaponIds.Katana_Pepper) return;
        PepperAttackbox?.Close();
    }
    public void AE_CoconutAttackStart()
    {
        if (lefthitbox.currentWeaponId != WeaponIds.Bowling_Coconut) return;
        Debug.Log("Coconut Attack Start");  
        CoconutAttackbox?.Open();
    }
    public void AE_CoconutAttackEnd()
    {
        if (lefthitbox.currentWeaponId != WeaponIds.Bowling_Coconut) return;
        CoconutAttackbox?.Close();
    }

}