using System.Collections;
using System.Collections.Generic;
using Unity.Cinemachine;
using UnityEngine;
using UnityEngine.InputSystem;
using static Unity.Burst.Intrinsics.X86.Avx;

public class PlayerMove : MonoBehaviour
{
    private static readonly int MoveHash = Animator.StringToHash("Move");
    private static readonly int JumpHash = Animator.StringToHash("Jump");
    private static readonly int GroundHash = Animator.StringToHash("IsGround");
    private static readonly int IdleHash = Animator.StringToHash("Idle");
    private static readonly int AttackHash = Animator.StringToHash("Attack");
    private static readonly int AttackSpeed = Animator.StringToHash("AttackSpeed");
    private static readonly int HashDoNext = Animator.StringToHash("Combo");

    private BuffController buffController;

    private int BASE = 0;
    private int EQUIP;

    private float moveSpeed = 5f;
    private float rotationSpeed = 180f;
    public float jumpForce = 5f;

    public Hitbox hitbox;

    public GameObject TopCamera;
    public GameObject PlayerCamera;
    private bool isViewTop = false;

    public bool IsMobileVeiwTopClicked { get; set; } = false;


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

    Vector3? attackDesiredForward;     
    float attackTurnSpeed = 540f;       
    float attackSnapAngle = 10f;        

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

    private void Start()
    {
        moveSpeed = player.moveSpeed;
        rotationSpeed = player.rotateSpeed;
    }

    private void Awake()
    {
        buffController = GetComponent<BuffController>();
        player = GetComponent<Player>();
        playerInput = GetComponent<PlayerInput>();
        rb = GetComponent<Rigidbody>();
        animator = GetComponent<Animator>();
        EQUIP = animator.GetLayerIndex("Equip Layer");
        //playerHealth = GetComponent<PlayerHealth>();
        //gun = GetComponentInChildren<Gun>();
        audioSource = GetComponent<AudioSource>();
        cap = GetComponent<CapsuleCollider>();
    }

    private int ActiveAttackLayer()
    {
        var st0 = animator.GetCurrentAnimatorStateInfo(BASE);
        if (st0.tagHash == Animator.StringToHash("Attack")) return BASE;

        if (EQUIP >= 0)
        {
            var st1 = animator.GetCurrentAnimatorStateInfo(EQUIP);
            if (st1.tagHash == Animator.StringToHash("Attack")) return EQUIP;
        }
        return BASE; 
    }

    //[SerializeField, Range(0f, 1f)] float comboWindowOpen = 0.5f;
    //[SerializeField, Range(0f, 1f)] float comboWindowClose = 0.85f;

    [SerializeField] int weaponMaxCombo = 3;
    int currentCombo = 0;
    bool canQueue = false;   // 창 열림

    bool queuedCombo;  

    public void OnAttackButton()  
    {
        if (player.isDead || animator == null) return;

        int layer = ActiveAttackLayer();
        var st = animator.GetCurrentAnimatorStateInfo(layer);
        bool inAttack = st.IsTag("Attack");
        if (!inAttack && animator.IsInTransition(layer))
        {
            var nt = animator.GetNextAnimatorStateInfo(layer);
            if (nt.IsTag("Attack")) inAttack = true;
        }
        var equipItem = GetComponent<EquipItem>();
        var smgr = GameObject.FindGameObjectWithTag("Ges").GetComponent<StateManager>();
        switch (equipItem.currentWeaponId)
        {
            case WeaponIds.Katana_Pepper:
                var upKa = smgr.GetUpgradeStatAmount((int)GenomIds.KatanaPepperAtkSpeedUp);
                var sKa = upKa == 0 ? 1f : upKa + 1f;
                sKa = sKa * buffController.AttackSpeed;
                animator.SetFloat(AttackSpeed, sKa);
                break;
            case WeaponIds.Bowling_Coconut:
                var upCo = smgr.GetUpgradeStatAmount((int)GenomIds.BowlingCoconutAtkSpeedUp);
                var sCo = upCo == 0 ? 1f : upCo + 1f;
                sCo = sCo * buffController.AttackSpeed;
                animator.SetFloat(AttackSpeed, sCo);
                break;
            default:
                //var s = 2.2f + (smgr.GetUpgradeStatAmount((int)GenomIds.BowlingCoconutAtkSpeedUp) * 2.2f);
                var up = smgr.GetUpgradeStatAmount((int)GenomIds.PlayerAttackSpeedUp);
                var s = up == 0 ? 1f : up + 1f;
                s = s * buffController.AttackSpeed;
                animator.SetFloat(AttackSpeed, s);
                break;
        }

        if (!inAttack)
        {
            currentCombo = 0;
            animator.ResetTrigger(AttackHash);
            animator.SetBool(HashDoNext, false);
            animator.SetTrigger(AttackHash);
            queuedCombo = false;
        }
        else
        {
            var equip = GetComponent<EquipItem>();
            //if (equip.IsEquipped())
            //{
            //    StartCoroutine(HitboxPulse(0.3f));
            //    animator.SetTrigger(AttackHash);
            //}
            if (equipItem.currentWeaponId != WeaponIds.Bowling_Coconut)
                queuedCombo = true;
            //if (equipItem.currentWeaponId == WeaponIds.UNKNOWN_WEAPON)
            //{
            //    queuedCombo = true;
            //    //animator.SetBool(HashDoNext, true);
            //}
        }
    }

    public void Ev_ComboOpen()
    {
        //Debug.Log("Combo Open");
        canQueue = true;
        if (queuedCombo) Ev_ComboConsume();
    }

    public void Ev_ComboClose()
    {
        //Debug.Log("Combo Close");
        canQueue = false;
        //StartCoroutine(ClearComboNextFrame());
        animator.SetBool(HashDoNext, false);
    }

    IEnumerator ClearComboNextFrame()
    {
        yield return null; 
        animator.SetBool(HashDoNext, false);
    }

    public void Ev_ComboConsume()
    {
        //Debug.Log("Combo Consume");
        if (!canQueue) return;
        if (currentCombo >= weaponMaxCombo - 1)
        {
            queuedCombo = false;
            animator.SetBool(HashDoNext, false);
            return;
        }
        animator.SetBool(HashDoNext, true);
        queuedCombo = false;
        currentCombo++;
    }


    private void Update()
    {
        if (player.isDead || animator == null) return;

        int layer = ActiveAttackLayer();
        var st = animator.GetCurrentAnimatorStateInfo(layer);

        if (st.IsTag("Attack"))
        {
            float t = st.normalizedTime % 1f;

            Vector3 camFwd = Camera.main.transform.forward;
            Vector3 camRight = Camera.main.transform.right;
            camFwd.y = 0f; camRight.y = 0f;
            camFwd.Normalize(); camRight.Normalize();
            Vector3 desiredMove = camRight * playerInput.MoveX + camFwd * playerInput.MoveZ;
            bool hasDirInput = desiredMove.sqrMagnitude > 0.001f;

            //if (queuedCombo && t >= comboWindowOpen && t <= comboWindowClose)
            if (queuedCombo && canQueue)
            {
                animator.SetBool(HashDoNext, true);

                if (hasDirInput)
                {
                    attackDesiredForward = desiredMove.normalized;
                }

                queuedCombo = false;
            }

            //if (t > comboWindowClose)
            //{
            //    animator.SetBool(HashDoNext, false);
            //    queuedCombo = false;
            //}
        }
        else
        {
            animator.SetBool(HashDoNext, false);
            queuedCombo = false;
            attackDesiredForward = null; 
        }
    }
    private bool IsAttackingOrTransitioning()
    {
        for (int layer = 0; layer < animator.layerCount; layer++)
        {
            var st = animator.GetCurrentAnimatorStateInfo(layer);
            if (st.IsTag("Attack")) return true;
            if (animator.IsInTransition(layer))
            {
                var nt = animator.GetNextAnimatorStateInfo(layer);
                if (nt.IsTag("Attack") || st.IsTag("Attack")) return true;
            }
        }
        return false;
    }

    bool IsAttackHardLocked(out float t01)
    {
        t01 = 0f;
        if (animator == null) return false;

        var st = animator.GetCurrentAnimatorStateInfo(0);

        //if (animator.IsInTransition(0))
        //{
        //    var nt = animator.GetNextAnimatorStateInfo(0);
        //    if (nt.IsTag("Attack")) return true;
        //}

        if (!st.IsTag("Attack")) return false;

        t01 = st.normalizedTime % 1f;

        return t01 < 0.9f;
    }

    private void FixedUpdate()
    {
        if (player.isDead) return;

        var attackLockMove = IsAttackingOrTransitioning();
        //var attackLockMove = IsAttackHardLocked(out float atkT);
        //var attackLockMove = false;

        //bool inAttackTag = animator.GetCurrentAnimatorStateInfo(0).IsTag("Attack");
        //회전

        //Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);

        //Plane plane = new Plane(Vector3.up, new Vector3(0, transform.position.y, 0));

        //if (plane.Raycast(ray, out float enter))
        //{
        //    Vector3 hitPoint = ray.GetPoint(enter);
        //    Vector3 dir = hitPoint - transform.position;
        //    dir.y = 0f;
        //    if (dir.sqrMagnitude >= 0.04f) 
        //    {
        //        Quaternion targetRot = Quaternion.LookRotation(dir);
        //        rb.MoveRotation(Quaternion.RotateTowards(rb.rotation, targetRot, 720f * Time.fixedDeltaTime));
        //    }
        //}
        //Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
        //RaycastHit hit;

        //if (Physics.Raycast(ray, out hit))
        //{
        //    Vector3 target = hit.point;
        //    target.y = transform.position.y;

        //    transform.LookAt(target);
        //}

        if (attackLockMove)
        {
            var v = rb.linearVelocity;
            rb.linearVelocity = new Vector3(0f, v.y, 0f);
            animator.SetFloat(MoveHash, 0f);
            //var equipItem = GetComponent<EquipItem>();
            //if (equipItem.currentWeaponId == WeaponIds.UNKNOWN_WEAPON)
            //    animator.SetFloat(MoveHash, 0f);
            //else animator.SetFloat(MoveHash, 0.1f);

            Vector3 camFwd2 = Camera.main.transform.forward;
            Vector3 camRight2 = Camera.main.transform.right;
            camFwd2.y = 0f; camRight2.y = 0f;
            camFwd2.Normalize(); camRight2.Normalize();

            Vector3 desiredMove = camRight2 * playerInput.MoveX + camFwd2 * playerInput.MoveZ;
            bool hasDirInputNow = desiredMove.sqrMagnitude > 0.001f;

            Vector3 targetForward =
                attackDesiredForward.HasValue ? attackDesiredForward.Value :
                hasDirInputNow ? desiredMove.normalized :
                transform.forward; 

            float dt = Time.fixedDeltaTime;
            Quaternion targetRot = Quaternion.LookRotation(targetForward, Vector3.up);
            rb.MoveRotation(Quaternion.RotateTowards(rb.rotation, targetRot, attackTurnSpeed * dt));

            float ang = Quaternion.Angle(rb.rotation, targetRot);
            if (attackDesiredForward.HasValue && ang <= attackSnapAngle)
                attackDesiredForward = null;

            //Vector3 step = transform.forward * (0.12f * dt);
            //rb.MovePosition(rb.position + step);

            return;
        }

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

        var g = GameObject.FindGameObjectWithTag("Ges").GetComponent<StateManager>();

        rb.MovePosition(rb.position + move * ((moveSpeed + g.GetUpgradeStatAmount((int)GenomIds.PlayerMoveSpeedUp))* speedMul) * Time.fixedDeltaTime);

        //bool hasMoveInput = (new Vector2(playerInput.MoveX, playerInput.MoveZ).sqrMagnitude > 0.0001f);

        //if (!hasMoveInput)
        //{
        //    camFwd = Camera.main.transform.forward; camFwd.y = 0; camFwd.Normalize();
        //    if (camFwd.sqrMagnitude > 0.001f)
        //    {
        //        Quaternion target = Quaternion.LookRotation(camFwd, Vector3.up);
        //        rb.MoveRotation(Quaternion.Slerp(rb.rotation, target, 8f * Time.deltaTime));
        //    }
        //}

        if (move.sqrMagnitude > 0.001f)
        {
            Quaternion targetRot = Quaternion.LookRotation(move, Vector3.up);
            rb.MoveRotation(Quaternion.RotateTowards(rb.rotation, targetRot, rotationSpeed * Time.fixedDeltaTime));
        }

        //Vector3 worldDir = new Vector3(playerInput.MoveZ, 0f, playerInput.MoveX);  
        //if (worldDir.sqrMagnitude > 1f) worldDir.Normalize();

        //if(animator.isActiveAndEnabled && animator.GetCurrentAnimatorStateInfo(0).IsName("Attack"))
        //{
        //    worldDir = Vector3.zero;
        //}
        //rb.MovePosition(rb.position + worldDir * moveSpeed * Time.fixedDeltaTime);



        //점프
        //isJumping = rb.linearVelocity.y > 0.1f || rb.linearVelocity.y < -0.1f;

        //if (playerInput.Jump /*&& !playerHealth.IsDead*/ && grounded)
        //{
        //    rb.AddForce(Vector3.up * jumpForce, ForceMode.VelocityChange);
        //    //if (audioSource != null && audioSource.clip != null)
        //    //{
        //    //    audioSource.Play();
        //    //}
        //}

        //if (playerInput.Attack && !player.isDead && animator != null)
        //{
        //    playerInput.Attack = false;
        //    StartCoroutine(HitboxPulse(0.3f));
        //    animator.SetTrigger(AttackHash);
        //    //Debug.Log("Player Attack");
        //    //player.Attack();
        //}

        if (Input.GetKeyDown(KeyCode.V) || IsMobileVeiwTopClicked)
        {
            isViewTop = !isViewTop;
            TopCamera.SetActive(isViewTop);
            PlayerCamera.SetActive(!isViewTop);
            IsMobileVeiwTopClicked = false;
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
        Debug.Log("Hitbox Pulse");
        hitbox.Open();
        yield return new WaitForSeconds(t);
        hitbox.Close();
    }
}
