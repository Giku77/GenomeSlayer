using System.Collections;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

[RequireComponent(typeof(RectTransform))]
public class QuickSlotDiscardHandler : MonoBehaviour,
    IPointerDownHandler, IPointerUpHandler,
    IBeginDragHandler, IDragHandler, IEndDragHandler, ICancelHandler
{
    [Header("Refs")]
    public int slotIndex;                        // 슬롯 인덱스
    public RectTransform quickbarArea;           // 퀵슬롯 바 전체 영역(슬롯들의 부모 RectTransform)
    public Canvas canvas;                        // UI Canvas (Screen Space - Overlay/Camera 모두 OK)
    public Image ghostPrefab;                    // 드래그 고스트(선택)

    [Header("Tuning")]
    public float longPressTime = 0.35f;          // 롱프레스 임계값(초)

    // 내부 상태
    private bool longPressed;
    private bool dragging;
    private Coroutine longPressCo;
    private Image ghost;
    private QuickSlotInventory inventory;
    private EquipItem equipItem;
    private Button button;                       // 원래 버튼(클릭 막기 용도)

    void Awake()
    {
        if (!canvas) canvas = GetComponentInParent<Canvas>();
        button = GetComponent<Button>();
        equipItem = GameObject.FindGameObjectWithTag("Player").GetComponent<EquipItem>();
        inventory = GameObject.FindGameObjectWithTag("Player").GetComponent<Player>().quickSlotInventory;
        if (!quickbarArea) quickbarArea = transform.parent as RectTransform; 
    }

    public void OnPointerDown(PointerEventData eventData)
    {
        longPressCo = StartCoroutine(LongPressTimer());
    }

    public void OnPointerUp(PointerEventData eventData)
    {
        StopLongPress();

    }

    public void OnBeginDrag(PointerEventData eventData)
    {
        if (!longPressed)
        {
            eventData.pointerDrag = null; // 드래그 취소
            return;
        }

        dragging = true;

        if (button) button.interactable = false;
        EventSystem.current.SetSelectedGameObject(null);

        if (ghostPrefab)
        {
            var canvasRect = canvas.transform as RectTransform;

            ghost = Instantiate(ghostPrefab, canvasRect);
            ghost.gameObject.SetActive(true);
            ghost.raycastTarget = false;

            // 맨 위로
            ghost.rectTransform.SetAsLastSibling();

            Vector2 local;
            RectTransformUtility.ScreenPointToLocalPointInRectangle(
                canvasRect,
                eventData.position,
                canvas.renderMode == RenderMode.ScreenSpaceOverlay ? null : canvas.worldCamera,
                out local
            );
            ghost.rectTransform.anchoredPosition = local;

            // 필요시 크기 조정
            // ghost.rectTransform.sizeDelta = new Vector2(80, 80);
        }


        //if (ghostPrefab)
        //{
        //    ghost = Instantiate(ghostPrefab, canvas.transform);
        //    ghost.raycastTarget = false;
        //    UpdateGhostPosition(eventData);
        //}
    }

    public void OnDrag(PointerEventData eventData)
    {
        if (!dragging) return;
        UpdateGhostPosition(eventData);
    }

    public void OnEndDrag(PointerEventData eventData)
    {
        if (!dragging) return;
        bool shouldDiscard = false;

        bool insideQuickBar = RectTransformUtility.RectangleContainsScreenPoint(
            quickbarArea, eventData.position, canvas.renderMode == RenderMode.ScreenSpaceOverlay ? null : canvas.worldCamera);

        shouldDiscard = !insideQuickBar;

        // TrashDropZone 위로 드랍했는지 체크하고 싶다면
        //    eventData.pointerCurrentRaycast.gameObject?.GetComponent<TrashDropZone>() != null 같은 방식으로 추가 판정 가능

        if (shouldDiscard)
        {
            if (equipItem && equipItem.SelectedIndex == slotIndex)
            {
                var slider = GetComponentInChildren<Slider>();
                if (slider) slider.gameObject.SetActive(false);
                equipItem.UnEquipItem();
            }

            inventory.RemoveItem(slotIndex);
            AudioManager.I?.PlaySFX("Drop");
        }

        CleanupDrag();
    }

    public void OnCancel(BaseEventData eventData)
    {
        CleanupDrag();
    }

    private IEnumerator LongPressTimer()
    {
        longPressed = false;
        yield return new WaitForSeconds(longPressTime);
        longPressed = true; // 이때부터 드래그 허용
    }

    private void StopLongPress()
    {
        if (longPressCo != null) StopCoroutine(longPressCo);
        longPressCo = null;
    }

    private void UpdateGhostPosition(PointerEventData e)
    {
        if (!ghost) return;
        RectTransformUtility.ScreenPointToLocalPointInRectangle(
            canvas.transform as RectTransform, e.position,
            canvas.renderMode == RenderMode.ScreenSpaceOverlay ? null : canvas.worldCamera,
            out var localPos);
        (ghost.transform as RectTransform).anchoredPosition = localPos;
    }

    private void CleanupDrag()
    {
        StopLongPress();
        dragging = false;
        longPressed = false;

        if (ghost) Destroy(ghost.gameObject);
        ghost = null;

        if (button) button.interactable = true;
    }
}
