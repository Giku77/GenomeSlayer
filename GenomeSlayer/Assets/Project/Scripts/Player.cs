using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;

public class Player : Entity
{
    private Animator animator;
    private CapsuleCollider capsuleCollider;
    private static readonly int hashDie = Animator.StringToHash("Die");

    private void Awake()
    {
        animator = GetComponent<Animator>();
        capsuleCollider = GetComponent<CapsuleCollider>();
    }
    protected override void OnEnable()
    {
        //base.OnEnable();
        health = 100;

    }
    public override void OnDamage(int damage)
    {
        base.OnDamage(damage);
        Debug.Log($"Player OnDamage {damage}, health {health}");
    }

    public void Attack()
    {
        var target = FindTarget(2.0f);
        if (target != null)
        {
            var entity = target.GetComponent<Enemy>();
            if (entity != null)
            {
                entity.OnDamage(damage);
            }
        }
    }

    protected override void Die()
    {
        Debug.Log("Player is dead.");
        //capsuleCollider.enabled = false;
        animator.SetTrigger(hashDie);
        StartCoroutine(Restart());
        Destroy(gameObject, 5f);
    }


    private IEnumerator Restart()
    {
        yield return new WaitForSeconds(7f);
        SceneManager.LoadScene(SceneManager.GetActiveScene().name);
    }

}
