using UnityEngine;

public class StateManager : MonoBehaviour
{
    public float DamageP { get; private set; } 
    public int Health { get; private set; }

    public void UpdateDamage(int id)
    {
        DamageP = DataTableManger.GeTable.GetItem(id).upgradeStatAmount;
    }

}
