using UnityEngine;

public class PlayerMove : MonoBehaviour
{
    private static readonly int MoveHash = Animator.StringToHash("Move");
    private static readonly int IdleHash = Animator.StringToHash("Idle");

    public float moveSpeed = 5f;
    public float rotationSpeed = 180f;

    private AudioSource audioSource;

    //private Gun gun;
    //private PlayerHealth playerHealth;

    private PlayerInput playerInput;
    private Rigidbody rb;
    private Animator animator;
    private void Awake()
    {
        playerInput = GetComponent<PlayerInput>();
        rb = GetComponent<Rigidbody>();
        animator = GetComponent<Animator>();
        //playerHealth = GetComponent<PlayerHealth>();
        //gun = GetComponentInChildren<Gun>();
        audioSource = GetComponent<AudioSource>();
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
            if (dir.sqrMagnitude < 0.2f * 0.2f) return;

            Quaternion targetRot = Quaternion.LookRotation(dir);
            rb.MoveRotation(Quaternion.RotateTowards(rb.rotation, targetRot, 720f * Time.fixedDeltaTime));
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
        Vector3 worldDir = new Vector3(playerInput.MoveZ, 0f, playerInput.MoveX);  
        if (worldDir.sqrMagnitude > 1f) worldDir.Normalize();

        rb.MovePosition(rb.position + -worldDir * moveSpeed * Time.fixedDeltaTime);
        //rb.MovePosition(rb.position + transform.forward * playerInput.MoveX * moveSpeed * Time.fixedDeltaTime);
        //rb.MovePosition(transform.position + transform.forward * playerInput.Move * moveSpeed * Time.deltaTime);

        //애니메이션 설정
        if (animator != null)
        {
            float move = new Vector3(playerInput.MoveX, 0f, playerInput.MoveZ).magnitude;
            animator.SetFloat(MoveHash, move);
        }
    }
}
