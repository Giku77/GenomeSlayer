using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class UIManager : MonoBehaviour
{
    public Slider HealthSilder;
    public TextMeshProUGUI CurrentWave;
    public TextMeshProUGUI WaveTimer;
    public Button WaveButton;
    public GameObject InventoryUI;
    public GameObject StateUI;
    private InventorySlotUI[] SlotItems;
    public TextMeshProUGUI StateNameText;
    public GameObject AcceptUI;
    public TextMeshProUGUI AcceptUIPoint;

    public StateDef StateDefData;

    private int currentIndex = 0;


    private void Awake()
    {
        AcceptUIPoint.text = StateDefData.GenomePoint.ToString();
        SlotItems = InventoryUI.GetComponentsInChildren<InventorySlotUI>();
        for (int i = 0; i < SlotItems.Length; i++)
        {
            SlotItems[i].itemName.text = "";
            SlotItems[i].itemCount.text = "";
        }
        EventBus.UpdateSlot += UpdateInventory;
        var s = StateUI.GetComponentInChildren<GridLayoutGroup>().GetComponentsInChildren<Button>();
        var name = DataTableManger.GeTable.GetAllItems();
        StateDefData.id = new int[s.Length];
        StateDefData.lv = new int[s.Length];
        //foreach (var b in s)
        for (int i = 0; i < s.Length; i++)
        {
            var b = s[i];
            int index = i;
            StateDefData.id[index] = name[index].UpgradeId;
            b.onClick.AddListener(() =>
            {
                currentIndex = index;
                //var name = b.GetComponentsInChildren<TextMeshProUGUI>();
                AcceptUI.SetActive(true);
                AcceptUI.GetComponentsInChildren<TextMeshProUGUI>()[2].text = name[index].upgradeName;
                StateNameText.text = name[index].upgradeName;
            });
        }
        var AcceptButtons = AcceptUI.GetComponentsInChildren<Button>();
        AcceptButtons[1].onClick.AddListener(() =>
        {
            AcceptButtonClicked(currentIndex);
        });
    }

    public void AcceptButtonClicked(int index)
    {
        var lv = StateDefData.lv[index]++;
        StateDefData.GenomePoint -= DataTableManger.GeTable.GetItem(StateDefData.id[index]).genomePoint1;
        AcceptUIPoint.text = StateDefData.GenomePoint.ToString();
        //var t = StateUI.GetComponentInChildren<GridLayoutGroup>()
        var t = GameObject.FindGameObjectsWithTag("sLV")[index].GetComponent<TextMeshProUGUI>();
        t.text = lv.ToString();
        //EventBus.RaiseStateChanged?.Invoke(StateDefData.id[StateDefData.currentState], StateDefData.lv[StateDefData.currentState]);
        AcceptUI.SetActive(false);
    }

    public void UpdateHealth(int health, int max)
    {
        HealthSilder.maxValue = max;
        HealthSilder.value = health;
    }

    public void ActiveWaveButton(bool t)
    {
        WaveButton.gameObject.SetActive(t);
    }

    public void UpdateWave(int wave)
    {
        CurrentWave.text = "CHAPTER: " + wave.ToString("D2");
    }

    public void UpdateWaveTimer(float time)
    {
        WaveTimer.text = $"{time:F0}";
    }

    public void UpdateInventory(int index, string name, string count)
    {
        if (index < 0 || index >= SlotItems.Length) return;
        if (name != "0")
         SlotItems[index].itemName.text = name;
        SlotItems[index].itemCount.text = count;
    }

    public void CloseButton()
    {         
        StateUI.SetActive(false);
    }

    public void CloseAccpet()
    {
        AcceptUI.SetActive(false);
    }

    private void Update()
    {
        if(Input.GetKeyDown(KeyCode.Tab))
        {
            StateUI.SetActive(!StateUI.activeSelf);
        }
    }
}
