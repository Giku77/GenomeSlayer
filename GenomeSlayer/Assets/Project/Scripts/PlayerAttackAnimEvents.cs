using UnityEngine;

public class PlayerAttackAnimEvents : MonoBehaviour
{
    public Hitbox righthitbox;
    public Hitbox lefthitbox;
    public void AEr_AttackStart() => righthitbox?.Open();
    public void AEr_AttackEnd() => righthitbox?.Close();
    public void AEl_AttackStart() => lefthitbox?.Open();
    public void AEl_AttackEnd() => lefthitbox?.Close();
}