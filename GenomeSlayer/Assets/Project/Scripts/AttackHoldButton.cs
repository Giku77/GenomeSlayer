using UnityEngine;
using UnityEngine.EventSystems;

public class AttackHoldButton : MonoBehaviour, IPointerDownHandler, IPointerUpHandler, IPointerExitHandler
{
    public PlayerMove playerMove;       
    public float initialDelay = 0.0f;      // 최초 지연(원하면 0)
    public float repeatInterval = 0.06f;   // 반복 간격(60FPS쯤: 0.016~0.06 권장)

    Coroutine loop;

    public void OnPointerDown(PointerEventData eventData)
    {
        if (loop == null) loop = StartCoroutine(RepeatLoop());
    }

    public void OnPointerUp(PointerEventData eventData) => StopLoop();
    public void OnPointerExit(PointerEventData eventData) => StopLoop();

    void StopLoop()
    {
        if (loop != null) { StopCoroutine(loop); loop = null; }
    }

    System.Collections.IEnumerator RepeatLoop()
    {
        if (initialDelay > 0f) yield return new WaitForSeconds(initialDelay);

        playerMove.OnAttackButton();

        while (true)
        {
            playerMove.OnAttackButton();
            yield return new WaitForSeconds(repeatInterval);
        }
    }
}
