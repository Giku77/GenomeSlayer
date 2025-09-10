using UnityEngine;
using UnityEngine.Rendering;

public class PlayerInput : MonoBehaviour
{
    public static readonly string verticalAxis = "Vertical";
    public static readonly string horizontalAxis = "Horizontal";
    public static readonly string fireButton = "Fire1";
    public static readonly string reloadButton = "Reload";


    public float MoveX { get; private set; }
    public float MoveZ { get; private set; }
    public float Roatate { get; private set; }
    public bool Fire { get; private set; }

    //public GameObject PauseUI;
    //public bool Reload { get; private set; }

    private void Update()
    {
        MoveX = Input.GetAxisRaw(verticalAxis);
        MoveZ = Input.GetAxisRaw(horizontalAxis);
        //Roatate = Input.GetAxis(horizontalAxis);

        Fire = Input.GetButton(fireButton);

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