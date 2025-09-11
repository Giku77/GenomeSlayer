using UnityEngine;
using UnityEngine.InputSystem.Android;
using UnityEngine.EventSystems;

public class VitualJoyStick : MonoBehaviour, IDragHandler, IBeginDragHandler, IEndDragHandler
{
    public RectTransform background;
    public RectTransform handle;
    private float radius = 0;
    public Vector2 Input { get; private set; }

    public void OnBeginDrag(PointerEventData eventData)
    {
        throw new System.NotImplementedException();
    }

    public void OnDrag(PointerEventData eventData)
    {
        var touchPosition = eventData.position;
        if (RectTransformUtility.ScreenPointToLocalPointInRectangle(background, touchPosition, eventData.pressEventCamera, out Vector2 position))
        {
            var delta = position;
            delta = Vector2.ClampMagnitude(delta, radius);
            handle.anchoredPosition = delta;
            Input = delta / radius;
            //position.x = (position.x / background.sizeDelta.x);
            //position.y = (position.y / background.sizeDelta.y);
            //Input = new Vector2(position.x * 2, position.y * 2);
            //Input = (Input.magnitude > 1) ? Input.normalized : Input;
            ////Move Handle
            //handle.anchoredPosition = new Vector2(Input.x * radius, Input.y * radius);
        }
    }

    public void OnEndDrag(PointerEventData eventData)
    {
        Input = Vector2.zero;
        handle.anchoredPosition = Vector2.zero;
        throw new System.NotImplementedException();
    }


    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        //AndroidJoystick
        radius = background.rect.width * 0.5f;

    }

    // Update is called once per frame
    void Update()
    {
        Debug.Log(Input);
    }
}