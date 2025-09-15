using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.UI;

public class PlayerInput : MonoBehaviour
{
    public static readonly string verticalAxis = "Vertical";
    public static readonly string horizontalAxis = "Horizontal";
    public static readonly string AttackButton = "Fire1";
    public static readonly string reloadButton = "Reload";

    private PlayerInteractor playerInteractor;

    public VitualJoyStick moveJoystick;
    public Button interactButton;


    public float MoveX { get; private set; }
    public float MoveZ { get; private set; }
    public float Roatate { get; private set; }
    public bool Attack { get; private set; }
    public bool Jump { get; private set; }

    //public GameObject PauseUI;
    //public bool Reload { get; private set; }

    private void Awake()
    {
        playerInteractor = GetComponent<PlayerInteractor>();
        interactButton.onClick.AddListener(() =>
        {
            playerInteractor.IsInteracting = true;
        });
    }

    private void Update()
    {
        MoveX = Input.GetAxisRaw(horizontalAxis);
        MoveZ = Input.GetAxisRaw(verticalAxis);
#if UNITY_ANDROID && !UNITY_EDITOR
        if (moveJoystick != null)
        {
            moveJoystick.gameObject.SetActive(true);    
            MoveX = moveJoystick.Input.x;
            MoveZ = moveJoystick.Input.y;
        }
        if (interactButton != null)
        {
            interactButton.gameObject.SetActive(true);    
        }
#endif

        Attack = Input.GetButtonDown(AttackButton);
        Jump = Input.GetButtonDown("Jump");

        //if (Input.GetKeyDown(KeyCode.Escape))
        //{
        //    if (PauseUI.activeSelf)
        //    {
        //        PauseUI.SetActive(false);
        //        Time.timeScale = 1f;
        //        //Cursor.lockState = CursorLockMode.Locked;
        //        //Cursor.visible = false;
        //    }
        //    else
        //    {
        //        PauseUI.SetActive(true);
        //        Time.timeScale = 0f;
        //        //Cursor.lockState = CursorLockMode.None;
        //        //Cursor.visible = true;
        //    }
        //}
        //Reload = Input.GetButtonDown(reloadButton);
    }



}