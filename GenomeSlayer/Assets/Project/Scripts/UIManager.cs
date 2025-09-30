using NUnit.Framework;
using System.Collections.Generic;
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

    public GameObject SettingUI;
    

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
    public GameObject GuidScreen;
    private TypingText typingText;
    private int typingID = 1601001;
    public void SetTypingID(int id) => typingID = id;
    public int GetTypingID() => typingID;

    public UIFocusHighlighter uIFocusHighlighter;
    public GameObject slideZone;
    public GameObject joystickZone;
    public GameObject AttackButton;
    public GameObject InteractButton;
    public GameObject HarvesButton;
    public GameObject ViewButton;
    public GameObject timerZone;
    public GameObject showFPS;
    public GameObject SettingButton;
    public void ActiveShowFPS(bool t) => showFPS.SetActive(t);

    public GameObject ActiveArmor;
    public InventorySlotUI ActiveArmorSlot { 
        get
        {
            return SlotItems[ActiveArmorIndex];
        }
    }

    private void OnEnable()
    {
        EventBus.UpdateSlot += UpdateInventory;
    }

    private void OnDisable()
    {
        EventBus.UpdateSlot -= UpdateInventory;
    }

    public int ActiveArmorIndex { get; set; }

    public void SetActiveAromor(bool t) => ActiveArmor.SetActive(t);
    public bool GetActiveAromor() => ActiveArmor.activeSelf;

    private void Awake()
    {
        AcceptUIPoint.text = stateManager.GenomePoint.ToString();
        typingText = GetComponent<TypingText>();
        SlotItems = InventoryUI.GetComponentsInChildren<InventorySlotUI>();
        for (int i = 0; i < SlotItems.Length; i++)
        {
            SlotItems[i].itemName.text = "";
            SlotItems[i].itemCount.text = "";
            SlotItems[i].slotIndex = i;
        }


      
        SetupStateGrid();

     
        var acceptButtons = AcceptUI.GetComponentsInChildren<Button>();
      
        acceptButtons[1].onClick.AddListener(() =>
        {
            AudioManager.I.PlaySFX("UIClicked");
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

    private void Start()
    {
        var settings = FindFirstObjectByType<SettingsManager>();
        if (settings != null)
        {
            WaveButton.interactable = settings.tutorialCompleted == 0;
            GenomButton.interactable = settings.tutorialCompleted == 0;
            ShowTypingText();
        }
    }

    public void OnAbleButtons(bool s)
    {
        WaveButton.interactable = s;
        GenomButton.interactable = s;
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
                AudioManager.I.PlaySFX("UIClicked");
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
    public void CloseSetting() => SettingUI.SetActive(false);

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
            player.quickSlotInventory.TryAddItem((int)ItemIds.Durian_Seed, 10);
        }
    }

    public void OnStateUIButtonClicked()
    {
        AudioManager.I.PlaySFX("UIClicked");
        StateUI.SetActive(!StateUI.activeSelf);
    }

    public void OnSettingButtonClicked()
    {
        AudioManager.I.PlaySFX("UIClicked");
        SettingUI.SetActive(!SettingUI.activeSelf);
    }

    public void StopTypingText()
    {
        typingID = 1601001;
        //ShowTypingText();
        TypingTextObject.SetActive(false);
        GuidScreen.SetActive(false);
        uIFocusHighlighter.gameObject.SetActive(false);
        WaveButton.interactable = true;
        GenomButton.interactable = true;
        var settings = FindFirstObjectByType<SettingsManager>();
        settings.tutorialCompleted = 0;

        GenomButton.gameObject.SetActive(true);
        WaveButton.gameObject.SetActive(true);
        SettingButton.gameObject.SetActive(true);
    }

    public bool IsGetEquipItem { get; set; }
    public void ShowTypingText()
    {
        if (!TypingTextObject.activeSelf) return;

        var player = GameObject.FindGameObjectWithTag("Player").GetComponent<Player>();
        var e = player.GetComponent<EquipItem>();

        AudioManager.I.PlaySFX("UIClicked");

        int guard = 0; 
        while (guard++ < 10)
        {
            var msg = DataTableManger.StringTable.GetItem(typingID);

            if (typingID == 1601007)
            {
                bool equipped = (e != null && e.currentWeaponId != WeaponIds.UNKNOWN_WEAPON);
                if (equipped)
                {
                    typingID = msg.nextToolTip;
                    continue; 
                }
            }

            if (typingID == 1601008)
            {
                bool slotCheck = (player.quickSlotInventory.GetSlot(1).quantity < 10);
                if (slotCheck)
                {
                    typingID = msg.nextToolTip;
                    continue;
                }
            }

            if (typingID == 1601010)
            {
                bool slotCheck = (player.quickSlotInventory.GetSlot(0).quantity < 7);
                if (slotCheck)
                {
                    typingID = msg.nextToolTip;
                    continue;
                }
            }

            if (typingID == 1601013)
            {
                if (IsGetEquipItem)
                {
                    typingID = msg.nextToolTip;
                    continue;
                }
            }

            break; 
        }

        var message = DataTableManger.StringTable.GetItem(typingID);

        uIFocusHighlighter.gameObject.SetActive(false);

        switch (typingID)
        {
            case 0:
                StopTypingText();
                return;

            case 1601001:
                IsGetEquipItem = false;
                SettingButton.gameObject.SetActive(false);
                GenomButton.gameObject.SetActive(false);
                WaveButton.gameObject.SetActive(false);
                break;

            case 1601004:
                uIFocusHighlighter.target = joystickZone.GetComponent<RectTransform>();
                uIFocusHighlighter.gameObject.SetActive(true);
                break;

            case 1601005:
                uIFocusHighlighter.target = slideZone.GetComponent<RectTransform>();
                uIFocusHighlighter.gameObject.SetActive(true);
                break;

            case 1601006:
                player.quickSlotInventory.TryAddItem((int)ItemIds.Earthy_Fertilizer, 10);
                player.quickSlotInventory.TryAddItem((int)ItemIds.Durian_Seed, 10);
                uIFocusHighlighter.target = InventoryUI.GetComponent<RectTransform>();
                uIFocusHighlighter.gameObject.SetActive(true);
                break;

            case 1601007:
                {
                    if (player.quickSlotInventory.GetSlot(2).itemId == -1)
                    {
                        var weapon = DataTableManger.EquipmentTable.GetItem((int)WeaponIds.Mace_Durian);
                        player.quickSlotInventory.TryAddItem((int)WeaponIds.Mace_Durian, 1, weapon.equipDurability, weapon.equipQuantity);
                    }
                    GuidScreen.SetActive(false);
                    uIFocusHighlighter.target = SlotItems[2].GetComponent<RectTransform>();
                    uIFocusHighlighter.gameObject.SetActive(true);
                    break;
                }

            case 1601008:
                uIFocusHighlighter.target = SlotItems[1].GetComponent<RectTransform>();
                uIFocusHighlighter.gameObject.SetActive(true);
                break;
            case 1601009:
            case 1601010:
                uIFocusHighlighter.target = InteractButton.GetComponent<RectTransform>();
                uIFocusHighlighter.gameObject.SetActive(true);
                break;
            case 1601013:
                uIFocusHighlighter.target = HarvesButton.GetComponent<RectTransform>();
                uIFocusHighlighter.gameObject.SetActive(true);
                break;
            case 1601014:
                GuidScreen.SetActive(true);
                uIFocusHighlighter.target = AttackButton.GetComponent<RectTransform>();
                uIFocusHighlighter.gameObject.SetActive(true);
                break;
            case 1601015:
                WaveButton.gameObject.SetActive(true);
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
                GenomButton.gameObject.SetActive(true);
                uIFocusHighlighter.target = GenomButton.GetComponent<RectTransform>();
                uIFocusHighlighter.gameObject.SetActive(true);
                break;
            case 1602002:
                GuidScreen.SetActive(false);
                StateUI.SetActive(true);
                var p = StateUI.GetComponentsInChildren<TextMeshProUGUI>();
                var pc = p[0].GetComponentInChildren<RectTransform>();
                uIFocusHighlighter.target = pc.GetComponent<RectTransform>();
                uIFocusHighlighter.gameObject.SetActive(true);
                break;
            case 1601018:
                GuidScreen.SetActive(true);
                StateUI.SetActive(false);
                uIFocusHighlighter.target = HealthSilder.GetComponent<RectTransform>();
                uIFocusHighlighter.gameObject.SetActive(true);
                break;
        }

        typingText.Play(message.toolTipText);

        if (typingID == 1601007)
        {
            bool equipped = (e != null && e.currentWeaponId != WeaponIds.UNKNOWN_WEAPON);
            if (equipped)
            {
                typingID = message.nextToolTip;
            }
            else
            {
                return;
            }
        }
        else if (typingID == 1601008)
        {
            bool slotCheck = (player.quickSlotInventory.GetSlot(1).quantity < 10);
            if (slotCheck)
            {
                typingID = message.nextToolTip;
            }
            else
            {
                return;
            }
        }
        else if (typingID == 1601010)
        {
            bool slotCheck = (player.quickSlotInventory.GetSlot(0).quantity < 7);
            if (slotCheck)
            {
                typingID = message.nextToolTip;
            }
            else
            {
                return;
            }
        }
        else if (typingID == 1601013)
        {
            if (IsGetEquipItem)
            {
                typingID = message.nextToolTip;
            }
            else
            {
                return;
            }
        }
        else
        {
            typingID = message.nextToolTip;
        }
    }


    //public void ShowTypingText()
    //{
    //    if (!TypingTextObject.activeSelf) return;
    //    var player = GameObject.FindGameObjectWithTag("Player").GetComponent<Player>();
    //    var arm = DataTableManger.EquipmentTable.GetItem((int)WeaponIds.Watermelon_Armor);
    //    var weapon = DataTableManger.EquipmentTable.GetItem((int)WeaponIds.Mace_Durian);
    //    AudioManager.I.PlaySFX("UIClicked");
    //    switch (typingID)
    //    {
    //        case 0:
    //            StopTypingText();
    //            return;
    //        case 1601001:
    //            GenomButton.gameObject.SetActive(false);
    //            WaveButton.gameObject.SetActive(false);
    //            break;
    //        case 1601004:
    //            uIFocusHighlighter.target = joystickZone.GetComponent<RectTransform>();
    //            uIFocusHighlighter.gameObject.SetActive(true);
    //            break;
    //        case 1601005:
    //            uIFocusHighlighter.target = slideZone.GetComponent<RectTransform>();
    //            uIFocusHighlighter.gameObject.SetActive(true);
    //            break;
    //        case 1601006:
    //            player.quickSlotInventory.TryAddItem((int)ItemIds.Earthy_Fertilizer, 10);
    //            player.quickSlotInventory.TryAddItem((int)ItemIds.Durian_Seed, 10);
    //            uIFocusHighlighter.target = InventoryUI.GetComponent<RectTransform>();
    //            uIFocusHighlighter.gameObject.SetActive(true);
    //            break;
    //        case 1601007:
    //            if (player.quickSlotInventory.GetSlot(2).itemId == -1)
    //                player.quickSlotInventory.TryAddItem((int)WeaponIds.Mace_Durian, 1, weapon.equipDurability, weapon.equipQuantity);

    //            uIFocusHighlighter.target = SlotItems[2].GetComponent<RectTransform>();
    //            uIFocusHighlighter.gameObject.SetActive(true);
    //            break;
    //        case 1601008:
    //            uIFocusHighlighter.target = SlotItems[1].GetComponent<RectTransform>();
    //            uIFocusHighlighter.gameObject.SetActive(true);
    //            break;
    //        case 1601009:
    //            uIFocusHighlighter.target = InteractButton.GetComponent<RectTransform>();
    //            uIFocusHighlighter.gameObject.SetActive(true);
    //            break;
    //        case 1601013:
    //            uIFocusHighlighter.target = HarvesButton.GetComponent<RectTransform>();
    //            uIFocusHighlighter.gameObject.SetActive(true);
    //            break;
    //        case 1601014:
    //            uIFocusHighlighter.target = AttackButton.GetComponent<RectTransform>();
    //            uIFocusHighlighter.gameObject.SetActive(true);
    //            break;
    //        case 1601015:
    //            uIFocusHighlighter.target = WaveButton.GetComponent<RectTransform>();
    //            uIFocusHighlighter.gameObject.SetActive(true);
    //            break;
    //        case 1601016:
    //            WaveButton.gameObject.SetActive(true);
    //            uIFocusHighlighter.target = ViewButton.GetComponent<RectTransform>();
    //            uIFocusHighlighter.gameObject.SetActive(true);
    //            break;
    //        case 1601017:
    //            uIFocusHighlighter.target = timerZone.GetComponent<RectTransform>();
    //            uIFocusHighlighter.gameObject.SetActive(true);
    //            break;
    //        case 1602001:
    //            uIFocusHighlighter.target = GenomButton.GetComponent<RectTransform>();
    //            uIFocusHighlighter.gameObject.SetActive(true);
    //            break;
    //        case 1601018:
    //            uIFocusHighlighter.target = HealthSilder.GetComponent<RectTransform>();
    //            uIFocusHighlighter.gameObject.SetActive(true);
    //            break;
    //        default:
    //            uIFocusHighlighter.gameObject.SetActive(false);
    //            break;
    //    }
    //    var message = DataTableManger.StringTable.GetItem(typingID);
    //    typingText.Play(message.toolTipText);
    //    if (typingID == 1601007)
    //    {
    //        GuidScreen.SetActive(false);
    //        var e = player.GetComponent<EquipItem>();
    //        if (e != null && e.currentWeaponId == WeaponIds.UNKNOWN_WEAPON)
    //        {
    //            return;
    //        }
    //        else 
    //        {
    //            GuidScreen.SetActive(true);
    //        }
    //    }
    //    typingID = message.nextToolTip;
    //}
}
