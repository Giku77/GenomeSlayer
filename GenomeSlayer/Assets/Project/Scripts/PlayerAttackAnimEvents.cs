using UnityEngine;

public class PlayerAttackAnimEvents : MonoBehaviour
{
    public Hitbox hitbox;
    public void AE_AttackStart() => hitbox?.Open();
    public void AE_AttackEnd() => hitbox?.Close();
}