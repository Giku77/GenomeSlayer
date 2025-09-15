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
    private InventorySlotUI[] SlotItems;


    private void Awake()
    {
        SlotItems = InventoryUI.GetComponentsInChildren<InventorySlotUI>();
        for (int i = 0; i < SlotItems.Length; i++)
        {
            SlotItems[i].itemName.text = "";
            SlotItems[i].itemCount.text = "";
        }
        EventBus.UpdateSlot += UpdateInventory;
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
}
