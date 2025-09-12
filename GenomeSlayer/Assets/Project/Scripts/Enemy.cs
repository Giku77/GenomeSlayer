using System.Collections;
using System.Linq;
using UnityEngine;
using UnityEngine.AI;

public class Enemy : Entity
{
    private NavMeshAgent agent;
    private Animator animator;


    public enum State
    {
        Idle,
        Trace,
        Attack,
        Die
    }
    private enum Type
    {
        Default,
        Speed,
        Heavy
    }

    private static readonly int hashDie = Animator.StringToHash("Die");
    private static readonly int hashTarget = Animator.StringToHash("HasTarget");

    private State currentState;

    private Transform target;

    public float traceDist = 10.0f;
    public float attackDist = 2.0f;

    public ParticleSystem bloodE;

    public float lastAttackTime;
    public float attackDelay = 1.0f;
    public AudioClip zombieHit;
    public AudioClip zombieDie;

    private CapsuleCollider capsuleCollider;
    //private AudioSource audioSource;
    //public Slider healthSlider;

    private bool sinking = false;

    public State state
    {
        get { return currentState; }
        set
        {
            var prev = currentState;
            currentState = value;
            switch (currentState)
            {
                case State.Idle:
                    animator.SetBool(hashTarget, false);
                    agent.isStopped = true;
                    break;
                case State.Trace:
                    animator.SetBool(hashTarget, true);
                    agent.isStopped = false;
                    break;
                case State.Attack:
                    animator.SetBool(hashTarget, false);
                    agent.isStopped = true;
                    break;
                case State.Die:
                    animator.SetTrigger(hashDie);
                    agent.isStopped = true;
                    break;
            }
        }
    }

    public void SetZombieData(EnemyData data)
    {
        health = data.health;
        damage = data.damage;
        //attackDelay = data.attackDelay;
        //traceDist = data.traceDist;
        //attackDist = data.attackDist;
        //agent.speed = data.speed;
        agent.speed = speed;
    }


    public void Awake()
    {
        agent = GetComponent<NavMeshAgent>();
        animator = GetComponent<Animator>();
        capsuleCollider = GetComponent<CapsuleCollider>();
        //audioSource = GetComponent<AudioSource>();
    }

    //private IEnumerator bloodEffect(Vector3 hitpos)
    //{
    //    //audioSource.PlayOneShot(zombieHit, AudioManager.instance.sfxVolume);

    //    bloodE.transform.position = hitpos;
    //    bloodE.Play();
    //    yield return new WaitForSeconds(1.0f);
    //}

    private void Update()
    {
        if (sinking)
            transform.Translate(Vector3.down * 2f * Time.deltaTime, Space.World);
        switch (currentState)
        {
            case State.Idle:
                UpdateIdle();
                break;
            case State.Trace:
                UpdateTrace();
                break;
            case State.Attack:
                UpdateAttack();
                break;
            case State.Die:
                UpdateDie();
                break;
        }
    }

    private void UpdateDie()
    {
        //Debug.Log("Zombie is dead.");
    }

    private void UpdateAttack()
    {
        if (target == null || (target != null && Vector3.Distance(transform.position, target.position) > attackDist))
        {
            state = State.Trace;
            animator.SetBool("Attack", false);
            return;
        }
        //transform.LookAt(target);
        var lookPos = target.position;
        lookPos.y = transform.position.y;
        transform.LookAt(lookPos);

        if (Time.time - lastAttackTime > attackDelay)
        {
            lastAttackTime = Time.time;

            var damageable = target.GetComponent<Player>();
            if (damageable != null && !damageable.isDead)
            {
                animator.SetBool("Attack", true);
                damageable.OnDamage(damage);
            }
        }
    }

    private void UpdateTrace()
    {
        if (target != null && Vector3.Distance(transform.position, target.position) <= attackDist)
        {
            state = State.Attack;
            return;
        }
        if (target == null && Vector3.Distance(transform.position, target.position) > traceDist)
        {
            state = State.Idle;
            return;
        }
        //animator.SetBool("HasTarget", true);
        agent.SetDestination(target.position);
    }

    private void UpdateIdle()
    {
        if (target != null && Vector3.Distance(transform.position, target.position) <= traceDist)
        {
            state = State.Trace;
        }

        target = FindTarget(traceDist).transform;
    }

    //protected override void OnEnable()
    //{
    //    base.OnEnable();
    //}

    //protected override void Die()
    //{
    //    base.Die();
    //    EventBus.EnemyDied?.Invoke();
    //}


    public override void OnDamage(int damage)
    {
        base.OnDamage(damage);
        //healthSlider.value = Health / MaxHealth;
        //StartCoroutine(bloodEffect(hitPoint));
    }

    //public void StartSinking()
    //{
    //    if (agent) agent.enabled = false;

    //    var rb = GetComponent<Rigidbody>();
    //    if (rb)
    //    {
    //        rb.isKinematic = true;
    //        rb.detectCollisions = false;
    //    }

    //    sinking = true;
    //    Destroy(gameObject, 5f);
    //}

    protected override void Die()
    {
        //audioSource.PlayOneShot(zombieDie, AudioManager.instance.sfxVolume);
        //base.Die();
        capsuleCollider.enabled = false;

        state = State.Die;
        animator.SetTrigger(hashDie);
        EventBus.EnemyDied?.Invoke();
        EventBus.EnemyDropSeed?.Invoke(gameObject.transform.position);
        Destroy(gameObject, 5f);
        //StartCoroutine(onDead());
    }

    //private IEnumerator onDead()
    //{
    //    yield return new WaitForSeconds(5.0f);
    //    Destroy(gameObject);
    //    //gameObject.SetActive(false);
    //}

}
