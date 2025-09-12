using System.Collections;
using UnityEngine;
using static Unity.Burst.Intrinsics.X86.Avx;

public class PlayerMove : MonoBehaviour
{
    private static readonly int MoveHash = Animator.StringToHash("Move");
    private static readonly int JumpHash = Animator.StringToHash("Jump");
    private static readonly int GroundHash = Animator.StringToHash("IsGround");
    private static readonly int IdleHash = Animator.StringToHash("Idle");
    private static readonly int AttackHash = Animator.StringToHash("Attack");

    public float moveSpeed = 5f;
    public float rotationSpeed = 180f;
    public float jumpForce = 5f;

    public Hitbox hitbox;



    private AudioSource audioSource;

    //private Gun gun;
    //private PlayerHealth playerHealth;

    private PlayerInput playerInput;
    private Player player;
    private Rigidbody rb;
    private Animator animator;
    private CapsuleCollider cap;
    //private bool isJumping = false;

    private int groundMask = ~0;
    private float groundedSkin = 0.05f; 
    private bool IsGrounded()
    {
        Vector3 center = transform.TransformPoint(cap.center);
        float radius = Mathf.Max(0.01f, cap.radius * 0.95f);

        Vector3 up = transform.up;
        float half = Mathf.Max(0f, (cap.height * 0.5f) - radius);
        Vector3 p1 = center + up * (half - groundedSkin);
        Vector3 p2 = center - up * (half - groundedSkin);


        return Physics.CheckCapsule(p1, p2, radius, groundMask, QueryTriggerInteraction.Ignore);
    }

    private void Awake()
    {
        player = GetComponent<Player>();
        playerInput = GetComponent<PlayerInput>();
        rb = GetComponent<Rigidbody>();
        animator = GetComponent<Animator>();
        //playerHealth = GetComponent<PlayerHealth>();
        //gun = GetComponentInChildren<Gun>();
        audioSource = GetComponent<AudioSource>();
        cap = GetComponent<CapsuleCollider>();
    }

    private void FixedUpdate()
    {
        //회전
        Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);

        Plane plane = new Plane(Vector3.up, new Vector3(0, transform.position.y, 0));

        if (plane.Raycast(ray, out float enter))
        {
            Vector3 hitPoint = ray.GetPoint(enter);
            Vector3 dir = hitPoint - transform.position;
            dir.y = 0f;
            if (dir.sqrMagnitude >= 0.04f) 
            {
                Quaternion targetRot = Quaternion.LookRotation(dir);
                rb.MoveRotation(Quaternion.RotateTowards(rb.rotation, targetRot, 720f * Time.fixedDeltaTime));
            }
        }
        //Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
        //RaycastHit hit;

        //if (Physics.Raycast(ray, out hit))
        //{
        //    Vector3 target = hit.point;
        //    target.y = transform.position.y;

        //    transform.LookAt(target);
        //}

        //이동
        Vector3 camFwd = Camera.main.transform.forward;
        Vector3 camRight = Camera.main.transform.right;

        camFwd.y = 0f; camRight.y = 0f;
        camFwd.Normalize(); camRight.Normalize();


        Vector3 move = camRight * playerInput.MoveX + camFwd * playerInput.MoveZ;
        if (move.sqrMagnitude > 1f) move.Normalize();

        bool grounded = IsGrounded(); 
        bool isAttacking = animator.GetCurrentAnimatorStateInfo(0).IsName("Attack");
        float speedMul = isAttacking ? 0.7f : 1f;

        rb.MovePosition(rb.position + move * (moveSpeed * speedMul) * Time.fixedDeltaTime);
        //Vector3 worldDir = new Vector3(playerInput.MoveZ, 0f, playerInput.MoveX);  
        //if (worldDir.sqrMagnitude > 1f) worldDir.Normalize();

        //if(animator.isActiveAndEnabled && animator.GetCurrentAnimatorStateInfo(0).IsName("Attack"))
        //{
        //    worldDir = Vector3.zero;
        //}
        //rb.MovePosition(rb.position + worldDir * moveSpeed * Time.fixedDeltaTime);



        //점프
        //isJumping = rb.linearVelocity.y > 0.1f || rb.linearVelocity.y < -0.1f;

        if (playerInput.Jump /*&& !playerHealth.IsDead*/ && grounded)
        {
            rb.AddForce(Vector3.up * jumpForce, ForceMode.VelocityChange);
            //if (audioSource != null && audioSource.clip != null)
            //{
            //    audioSource.Play();
            //}
        }

        if (playerInput.Attack && !player.isDead && animator != null)
        {
            StartCoroutine(HitboxPulse(0.12f));
            animator.SetTrigger(AttackHash);
            //Debug.Log("Player Attack");
            //player.Attack();
        }



        //애니메이션 설정
        if (animator != null)
        {
            float moveV = new Vector3(playerInput.MoveX, 0f, playerInput.MoveZ).magnitude;
            animator.SetFloat(MoveHash, moveV);
            animator.SetBool(JumpHash, playerInput.Jump && !player.isDead);
            animator.SetBool(GroundHash, grounded);
            //animator.SetBool(AttackHash, playerInput.Attack && !player.isDead);
        }
    }

    private IEnumerator HitboxPulse(float t)
    {
        hitbox.Open();
        yield return new WaitForSeconds(t);
        hitbox.Close();
    }
}
