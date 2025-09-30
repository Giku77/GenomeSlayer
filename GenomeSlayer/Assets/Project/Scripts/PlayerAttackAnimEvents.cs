using UnityEngine;

public class PlayerAttackAnimEvents : MonoBehaviour
{
    public Hitbox righthitbox;
    public Hitbox lefthitbox;

    public Hitbox DurianAttackbox;
    public Hitbox PepperAttackbox;
    public Hitbox CoconutAttackbox;

    private EquipItem equipItem => GameObject.FindGameObjectWithTag("Player").GetComponent<EquipItem>();
    public void AEr_AttackStart()
    {
        if (equipItem.currentWeaponId != WeaponIds.UNKNOWN_WEAPON) return;
        Debug.Log("Right Attack Start");
        righthitbox?.Open();
    }
    public void AEr_AttackEnd()
    {
        if (equipItem.currentWeaponId != WeaponIds.UNKNOWN_WEAPON) return;
        righthitbox?.Close();
    }

    public void AEl_AttackStart()
    {
        if (equipItem.currentWeaponId != WeaponIds.UNKNOWN_WEAPON) return;
        Debug.Log("Left Attack Start");
        lefthitbox?.Open();
    }
    public void AEl_AttackEnd()
    {
        if (equipItem.currentWeaponId != WeaponIds.UNKNOWN_WEAPON) return;
        lefthitbox?.Close();
    }
    public void AE_DurianAttackStart()
    {
        if (equipItem.currentWeaponId != WeaponIds.Mace_Durian) return;
        Debug.Log("Durian Attack Start");
        DurianAttackbox?.Open();
    }
    public void AE_DurianAttackEnd()
    {
        if (equipItem.currentWeaponId != WeaponIds.Mace_Durian) return;
        Debug.Log("Durian Attack End");
        DurianAttackbox?.Close();
    }
    public void AE_PepperAttackStart()
    {
        if (equipItem.currentWeaponId != WeaponIds.Katana_Pepper) return;
        Debug.Log("Pepper Attack Start");
        PepperAttackbox?.Open();
    }
    public void AE_PepperAttackEnd()
    {
        if (equipItem.currentWeaponId != WeaponIds.Katana_Pepper) return;
        PepperAttackbox?.Close();
    }
    public void AE_CoconutAttackStart()
    {
        if (equipItem.currentWeaponId != WeaponIds.Bowling_Coconut) return;
        Debug.Log("Coconut Attack Start");  
        CoconutAttackbox?.Open();
    }
    public void AE_CoconutAttackEnd()
    {
        if (equipItem.currentWeaponId != WeaponIds.Bowling_Coconut) return;
        //CoconutAttackbox?.Close();
    }

}