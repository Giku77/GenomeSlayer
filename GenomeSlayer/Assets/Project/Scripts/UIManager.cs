using TMPro;
using UnityEngine;
using UnityEngine.UI;
using static StateManager;

public class UIManager : MonoBehaviour
{
    public Slider HealthSilder;
    public TextMeshProUGUI CurrentWave;
    public TextMeshProUGUI WaveTimer;
    public Button WaveButton;
    public Button GenomButton;
    

    public GameObject InventoryUI;
    public GameObject StateUI;
    public TextMeshProUGUI StateNameText;

    public GameObject AcceptUI;
    public TextMeshProUGUI AcceptUIPoint;

    public StateManager stateManager;

    private InventorySlotUI[] SlotItems;
    private int currentIndex = 0;
    private static readonly string rStr = "레벨업을 하시겠습니까?";

    public GameObject TypingTextObject;
    private TypingText typingText;
    private int typingID = 1601001;

    public UIFocusHighlighter uIFocusHighlighter;
    public GameObject slideZone;
    public GameObject joystickZone;
    public GameObject AttackButton;
    public GameObject InteractButton;
    public GameObject HarvesButton;
    public GameObject ViewButton;
    public GameObject timerZone;

    public GameObject ActiveArmor;
    public InventorySlotUI ActiveArmorSlot { 
        get
        {
            return SlotItems[ActiveArmorIndex];
        }
    }
    public int ActiveArmorIndex { get; set; }

    public void SetActiveAromor(bool t) => ActiveArmor.SetActive(t);

    private void Awake()
    {
        AcceptUIPoint.text = stateManager.GenomePoint.ToString();
        typingText = GetComponent<TypingText>();
        WaveButton.interactable = false;
        GenomButton.interactable = false;
        ShowTypingText();
        SlotItems = InventoryUI.GetComponentsInChildren<InventorySlotUI>();
        for (int i = 0; i < SlotItems.Length; i++)
        {
            SlotItems[i].itemName.text = "";
            SlotItems[i].itemCount.text = "";
            SlotItems[i].slotIndex = i;
        }

       
        EventBus.UpdateSlot += UpdateInventory;

      
        SetupStateGrid();

     
        var acceptButtons = AcceptUI.GetComponentsInChildren<Button>();
      
        acceptButtons[1].onClick.AddListener(() =>
        {
            OnAcceptLevelUpClicked();
        });

        stateManager.OnGenomePointChanged += (pt) =>
        {
            AcceptUIPoint.text = pt.ToString();
        };
        stateManager.OnLevelChanged += (id, newLv) =>
        {
            RefreshStateLevels();
        };
    }

    public void ActiveFalseSlider()
    {
        for (int i = 0; i < SlotItems.Length; i++)
        {
            SlotItems[i].durSlider.gameObject.SetActive(false);
        }
    }

    private void SetupStateGrid()
    {
        var grid = StateUI.GetComponentInChildren<GridLayoutGroup>();
        var buttons = grid.GetComponentsInChildren<Button>();
        var levelTexts = grid.GetComponentsInChildren<TextMeshProUGUI>();

        var nameTable = DataTableManger.GeTable.GetAllItems();

        int count = Mathf.Min(buttons.Length, stateManager.RowCount);

        for (int i = 0; i < count; i++)
        {
            int index = i;
            var row = stateManager.GetRow(index);

            levelTexts[i].text = stateManager.GetLevelByIndex(index).ToString();

            buttons[i].onClick.RemoveAllListeners();
            buttons[i].onClick.AddListener(() =>
            {
                currentIndex = index;
                AcceptUI.SetActive(true);

                var txts = AcceptUI.GetComponentsInChildren<TextMeshProUGUI>();
   
                txts[1].text = rStr;
                txts[2].text = nameTable[index].upgradeName;

                StateNameText.text = nameTable[index].upgradeName;
            });
        }
    }

    private void RefreshStateLevels()
    {
        var grid = StateUI.GetComponentInChildren<GridLayoutGroup>();
        var levelTexts = grid.GetComponentsInChildren<TextMeshProUGUI>();

        int count = Mathf.Min(levelTexts.Length, stateManager.RowCount);
        for (int i = 0; i < count; i++)
        {
            levelTexts[i].text = stateManager.GetLevelByIndex(i).ToString();
        }

        AcceptUIPoint.text = stateManager.GenomePoint.ToString();
    }

    private void OnAcceptLevelUpClicked()
    {
        // int cost = stateManager.GetNextCostByIndex(currentIndex);
        var result = stateManager.TryLevelUpByIndexResult(currentIndex);

        switch (result)
        {
            case LevelUpResult.Ok:
                RefreshStateLevels();
                AcceptUI.SetActive(false);
                break;

            case LevelUpResult.NotEnoughPoint:
                AcceptUI.GetComponentsInChildren<TextMeshProUGUI>()[1].text = "포인트가 부족합니다.";
                break;

            case LevelUpResult.ReachedMaxLevel:
                AcceptUI.GetComponentsInChildren<TextMeshProUGUI>()[1].text = "이미 최대 레벨입니다.";
                break;

            case LevelUpResult.InvalidIndex:
                AcceptUI.GetComponentsInChildren<TextMeshProUGUI>()[1].text = "잘못된 선택입니다.";
                break;
        }
    }

    public void UpdateHealth(int health, int max)
    {
        HealthSilder.maxValue = max;
        HealthSilder.value = health;
    }

    public void ActiveWaveButton(bool t) => WaveButton.gameObject.SetActive(t);

    public void ActiveGenomButton(bool t) => GenomButton.gameObject.SetActive(t);

    public void UpdateWave(int wave) => CurrentWave.text = "CHAPTER: " + wave.ToString("D2");

    public void UpdateWaveTimer(float time) => WaveTimer.text = $"{time:F0}";

    public void UpdateInventory(int index, string name, string count, int dur)
    {
        if (index < 0 || index >= SlotItems.Length) return;
        if (name != "0") SlotItems[index].itemName.text = name;
        SlotItems[index].itemCount.text = count;
        if (dur > 0)
        {
            if (!SlotItems[index].durSlider) return;
            SlotItems[index].durSlider.gameObject.SetActive(true);
            SlotItems[index].durSlider.maxValue = dur;
            SlotItems[index].durSlider.value = dur;
        }
      
    }

    public void CloseButton() => StateUI.SetActive(false);
    public void CloseAccpet() => AcceptUI.SetActive(false);

    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.Tab))
            StateUI.SetActive(!StateUI.activeSelf);
        if (Input.GetKeyUp(KeyCode.F12))
        {
            //var weapon = DataTableManger.EquipmentTable.GetItem((int)WeaponIds.Watermelon_Armor);
            var player = GameObject.FindGameObjectWithTag("Player").GetComponent<Player>();
            player.quickSlotInventory.TryAddItem((int)ItemIds.Earthy_Fertilizer, 77);
            player.quickSlotInventory.TryAddItem((int)ItemIds.Watermelon_Seed, 10);
            player.quickSlotInventory.TryAddItem((int)ItemIds.Coconut_Seed, 10);
            player.quickSlotInventory.TryAddItem((int)ItemIds.Pepper_Seed, 10);
        }
    }

    public void OnStateUIButtonClicked()
    {
        StateUI.SetActive(!StateUI.activeSelf);
    }

    public void StopTypingText()
    {
        TypingTextObject.SetActive(false);
        uIFocusHighlighter.gameObject.SetActive(false);
        WaveButton.interactable = true;
        GenomButton.interactable = true;
    }

    public void ShowTypingText()
    {
        switch (typingID)
        {
            case 0:
                TypingTextObject.SetActive(false);
                WaveButton.interactable = true;
                GenomButton.interactable = true;
                return;
            case 1601004:
                uIFocusHighlighter.target = joystickZone.GetComponent<RectTransform>();
                uIFocusHighlighter.gameObject.SetActive(true);
                break;
            case 1601005:
                uIFocusHighlighter.target = slideZone.GetComponent<RectTransform>();
                uIFocusHighlighter.gameObject.SetActive(true);
                break;
            case 1601006:
                uIFocusHighlighter.target = InventoryUI.GetComponent<RectTransform>();
                uIFocusHighlighter.gameObject.SetActive(true);
                break;
            case 1601007:
                uIFocusHighlighter.target = SlotItems[0].GetComponent<RectTransform>();
                uIFocusHighlighter.gameObject.SetActive(true);
                break;
            case 1601008:
            case 1601009:
                uIFocusHighlighter.target = InteractButton.GetComponent<RectTransform>();
                uIFocusHighlighter.gameObject.SetActive(true);
                break;
            case 1601013:
                uIFocusHighlighter.target = HarvesButton.GetComponent<RectTransform>();
                uIFocusHighlighter.gameObject.SetActive(true);
                break;
            case 1601014:
                uIFocusHighlighter.target = AttackButton.GetComponent<RectTransform>();
                uIFocusHighlighter.gameObject.SetActive(true);
                break;
            case 1601015:
                uIFocusHighlighter.target = WaveButton.GetComponent<RectTransform>();
                uIFocusHighlighter.gameObject.SetActive(true);
                break;
            case 1601016:
                uIFocusHighlighter.target = ViewButton.GetComponent<RectTransform>();
                uIFocusHighlighter.gameObject.SetActive(true);
                break;
            case 1601017:
                uIFocusHighlighter.target = timerZone.GetComponent<RectTransform>();
                uIFocusHighlighter.gameObject.SetActive(true);
                break;
            case 1602001:
                uIFocusHighlighter.target = GenomButton.GetComponent<RectTransform>();
                uIFocusHighlighter.gameObject.SetActive(true);
                break;
            case 1601018:
                uIFocusHighlighter.target = HealthSilder.GetComponent<RectTransform>();
                uIFocusHighlighter.gameObject.SetActive(true);
                break;
            default:
                uIFocusHighlighter.gameObject.SetActive(false);
                break;
        }
        var message = DataTableManger.StringTable.GetItem(typingID);
        typingText.Play(message.toolTipText);
        typingID = message.nextToolTip;
    }
}
