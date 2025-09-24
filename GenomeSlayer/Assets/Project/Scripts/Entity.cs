using System.Linq;
using UnityEngine;

public class Entity : MonoBehaviour
{
    protected int maxhealth = 100;
    protected int health = 100;
    protected int damage = 10;
    protected float speed = 5f;

    public bool isDead => health <= 0;

    public LayerMask targetPlayer;

    protected Collider FindTarget(float radius)
    {
        var colliders = Physics.OverlapSphere(transform.position, radius, targetPlayer.value);
        if (colliders == null || colliders.Length == 0)
        {
            return null;
        }

        return colliders.OrderBy(x => Vector3.Distance(x.transform.position, transform.position)).First();

    }

    protected Transform FindTargetT(float radius)
    {
        var cols = Physics.OverlapSphere(transform.position, radius, targetPlayer);
        if (cols == null || cols.Length == 0) return null;
        return cols.OrderBy(c => Vector3.Distance(c.transform.position, transform.position))
                   .First().transform;
    }

    protected virtual void Die()
    {
        Destroy(gameObject);
    }

    protected virtual void OnEnable()
    {
        
    }

    public virtual void OnDamage(int damage)
    {
        health -= damage;
        //Debug.Log($"Entity OnDamage {damage}, health {health}");
        if (isDead)
        {
            Die();
        }
    }   


}
