using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class EquipItem : MonoBehaviour
{
    public Transform[] parts;
    public GameObject[] weapons;
    public GameObject slots;

    private Button[] buttons;
    private QuickSlotInventory inventory;

    private GameObject[] PoolWeapons;
    private GameObject currentWeapon;

    private int index = -1;

    private void Awake()
    {
        inventory = GetComponent<Player>().quickSlotInventory;
        buttons = slots.GetComponentsInChildren<Button>();
        for (int i = 0; i < buttons.Length; i++)
        {
            int closureIndex = i;
            buttons[i].onClick.AddListener(() =>
            {
                EventSystem.current.SetSelectedGameObject(buttons[closureIndex].gameObject);
            });
        }
        PoolWeapons = new GameObject[weapons.Length];

        for (int i = 0; i < parts.Length; i++)
        {
            var e = Instantiate(weapons[i], parts[i]);
            PoolWeapons[i] = e;
            e.gameObject.SetActive(false);
        }
    }

    public int GetSelectedIndex()
    {
        var selected = EventSystem.current.currentSelectedGameObject;
        if (selected == null) return -1;

        for (int i = 0; i < buttons.Length; i++)
        { 
            if (buttons[i].gameObject == selected)
                return i;
        }
        return -1;
    }

    private void Update()
    {
        int newIndex = GetSelectedIndex();
        if (newIndex != index)
        {
            index = newIndex;
            UpdateEquipItem();
        }
    }

    private void UpdateEquipItem()
    {
        if (index < 0 || index >= buttons.Length) return;
        var EquipId = inventory.GetSlot(GetSelectedIndex()).itemId;
        var HandHitbox = GetComponentsInChildren<Hitbox>();
        Debug.Log($"EquipId: {EquipId}");   
        switch (EquipId)
        {
            case (int)ItemIds.Mace_Durian:
                PoolWeapons[0].gameObject.SetActive(true);
                HandHitbox[0].enabled = false;
                currentWeapon = PoolWeapons[0];
                break;
            default:
                PoolWeapons[0].gameObject.SetActive(false);
                HandHitbox[0].enabled = true;
                currentWeapon = null;
                break;
        }
        //for (int i = 0; i < parts.Length; i++)
        //{
        //    if (i == index)
        //        parts[i].gameObject.SetActive(true);
        //    else
        //        parts[i].gameObject.SetActive(false);
        //}
    }

}
