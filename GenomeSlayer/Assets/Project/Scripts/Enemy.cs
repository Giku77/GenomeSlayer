using System.Collections;
using System.Linq;
using UnityEngine;
using UnityEngine.AI;
using UnityEngine.UI;

public class Enemy : Entity
{
    private NavMeshAgent agent;
    private Animator animator;
    private BuffController buffController;
    private Slider healthSlider;
    public EnemyData enemyData;
    private EnemyIds enemyId;


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
    private static readonly int hashAttackSpeed = Animator.StringToHash("mASpeed");

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
                    if(agent.isOnNavMesh)
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

    public void SetEnemyData(EnemyData data)
    {
        enemyId = data.enemyId;
        maxhealth = data.health;
        health = data.health;
        damage = data.damage;
        //attackDelay = data.attackDelay;
        //traceDist = data.traceDist;
        //attackDist = data.attackDist;
        agent.speed = data.speed;
        //Debug.Log($"SetEnemyData: {enemyId}, health {health}, speed {agent.speed}, damage {damage}, attackSpeed {data.attackSpeed}");
    }


    public void Awake()
    {
        healthSlider = GetComponentInChildren<Slider>();
        buffController = GetComponent<BuffController>();
        agent = GetComponent<NavMeshAgent>();
        animator = GetComponent<Animator>();
        capsuleCollider = GetComponent<CapsuleCollider>();
        if (enemyData != null)
            SetEnemyData(enemyData);
        healthSlider.maxValue = maxhealth;
        healthSlider.value = health;
        //Debug.Log($"Enemy Awake: {health}");
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
                animator.SetFloat(hashAttackSpeed, enemyData.attackSpeed);
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
        agent.speed = speed * buffController.MoveSpeedMul;
        agent.SetDestination(target.position);
    }

    private void UpdateIdle()
    {
        target = FindTargetT(traceDist);
        if (target != null && Vector3.Distance(transform.position, target.position) <= traceDist)
        {
            state = State.Trace;
        }
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
        //base.OnDamage(damage);
        //Debug.Log($"Enemy OnDamage {damage}, health {health}");
        //healthSlider.value = health;
        //StartCoroutine(bloodEffect(hitPoint));
        OnDamage(damage, transform.position + Vector3.up * 1.0f);
    }

    public void OnDamage(int damage, Vector3 hitPoint)
    {
        base.OnDamage(damage);
        Debug.Log($"Enemy OnDamage {damage}, health {health}");
        healthSlider.value = health;

        var hf = GetComponent<HitFlash>();
        if (hf) hf.Play();

        if (DamageTextManager.I != null)
            DamageTextManager.I.Show(damage, hitPoint);
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

    bool inKnockback;

    public void Knockback(Vector3 dir, float force, float duration = 0.18f)
    {
        if (!gameObject.activeInHierarchy) return;
        StartCoroutine(CoKnockback(dir, force, duration));
    }

    IEnumerator CoKnockback(Vector3 dir, float force, float duration)
    {
        if (inKnockback) yield break;
        inKnockback = true;

        if (agent)
        {
            agent.isStopped = true;
            agent.updatePosition = false;
            agent.updateRotation = false;
        }

        var rb = GetComponent<Rigidbody>();
        if (rb)
        {
            var prevKin = rb.isKinematic;
            var prevGrav = rb.useGravity;
            var prevCons = rb.constraints;
            var prevCd = rb.collisionDetectionMode;

            rb.isKinematic = false;                        
            rb.useGravity = false;                         
            rb.collisionDetectionMode = CollisionDetectionMode.ContinuousDynamic;
            rb.linearVelocity = Vector3.zero;
            rb.angularVelocity = Vector3.zero;

            rb.constraints = prevCons | RigidbodyConstraints.FreezePositionY;

            dir.y = 0f; dir.Normalize();

            rb.AddForce(dir * force, ForceMode.VelocityChange);

            float t = 0f;
            while (t < duration)
            {
                t += Time.fixedDeltaTime;
                yield return new WaitForFixedUpdate();
            }

            rb.linearVelocity = Vector3.zero;
            rb.constraints = prevCons;
            rb.useGravity = prevGrav;
            rb.isKinematic = prevKin;
            rb.collisionDetectionMode = prevCd;
        }

        if (agent)
        {
            agent.Warp(transform.position);
            agent.updatePosition = true;
            agent.updateRotation = true;
            agent.isStopped = false;
        }

        inKnockback = false;
    }


    protected override void Die()
    {
        //audioSource.PlayOneShot(zombieDie, AudioManager.instance.sfxVolume);
        //base.Die();
        healthSlider.gameObject.SetActive(false);
        capsuleCollider.enabled = false;
        var rb = GetComponent<Rigidbody>();
        if (rb)
        {
            rb.isKinematic = true;
            rb.detectCollisions = false;
        }

        state = State.Die;
        animator.SetTrigger(hashDie);
        EventBus.EnemyDied?.Invoke(gameObject);
        EventBus.EnemyDropSeed?.Invoke(gameObject.transform.position + Vector3.up);

        //Destroy(gameObject, 3f);
    }

    public void ResetEnemy()
    {
        healthSlider.gameObject.SetActive(true);
        capsuleCollider.enabled = true;
        var rb = GetComponent<Rigidbody>();
        if (rb)
        {
            rb.isKinematic = false;
            rb.detectCollisions = true;
        }
        health = maxhealth;
        healthSlider.value = health;
        animator.ResetTrigger(hashDie);
        state = State.Idle;
        //animator.Rebind();
        //animator.Update(0f);
    }


}
