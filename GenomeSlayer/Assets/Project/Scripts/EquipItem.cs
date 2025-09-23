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

    private EquipController equipController;

    private int index = -1;

    public int SelectedIndex { get; set; }

    private void Awake()
    {
        equipController = GetComponent<EquipController>();
        inventory = GetComponent<Player>().quickSlotInventory;
        buttons = slots.GetComponentsInChildren<Button>();
        for (int i = 0; i < buttons.Length; i++)
        {
            int closureIndex = i;
            buttons[i].onClick.AddListener(() =>
            {
                EventSystem.current.SetSelectedGameObject(buttons[closureIndex].gameObject);
                SelectedIndex = closureIndex;
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
        int BaseId = ToBaseId(EquipId);
        //int enh = GetEnhanceLevel(EquipId);
        var HandHitbox = GetComponentsInChildren<Hitbox>();
        Debug.Log($"EquipId: {EquipId}");   
        switch (BaseId)
        {
            case (int)WeaponIds.Mace_Durian:
                PoolWeapons[0].SetActive(true);
                PoolWeapons[1].SetActive(false);
                PoolWeapons[2].SetActive(false);
                HandHitbox[0].enabled = false;
                HandHitbox[1].enabled = false;
                HandHitbox[0].weaponDef.weaponId = WeaponIds.Mace_Durian;
                currentWeapon = PoolWeapons[0];
                equipController.SetEquipped(WeaponStance.TwoHand);
                break;
            case (int)WeaponIds.Katana_Pepper:
                PoolWeapons[0].SetActive(false);
                PoolWeapons[1].SetActive(true);
                PoolWeapons[2].SetActive(false);
                HandHitbox[0].enabled = false;
                HandHitbox[1].enabled = false;
                HandHitbox[0].weaponDef.weaponId = WeaponIds.Katana_Pepper;
                currentWeapon = PoolWeapons[1];
                equipController.SetEquipped(WeaponStance.OneHand);
                break;
            case (int)WeaponIds.Bowling_Coconut:
                PoolWeapons[0].SetActive(false);
                PoolWeapons[1].SetActive(false);
                PoolWeapons[2].SetActive(true);
                HandHitbox[0].enabled = false;
                HandHitbox[1].enabled = false;
                HandHitbox[0].weaponDef.weaponId = WeaponIds.Bowling_Coconut;
                currentWeapon = PoolWeapons[2];
                equipController.SetEquipped(WeaponStance.OneHand);
                break;
            default:
                PoolWeapons[0].gameObject.SetActive(false);
                PoolWeapons[1].gameObject.SetActive(false);
                PoolWeapons[2].gameObject.SetActive(false);
                HandHitbox[0].enabled = true;
                HandHitbox[1].enabled = true;
                HandHitbox[0].weaponDef.weaponId = WeaponIds.UNKNOWN_WEAPON;
                currentWeapon = null;
                equipController.SetEquipped(WeaponStance.None);
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

    static int GetEnhanceLevel(int id) => (id / 1000) % 10;
    static int ToBaseId(int id) => id - GetEnhanceLevel(id) * 1000;

    public void UnEquipItem()
    {
        var HandHitbox = GetComponentsInChildren<Hitbox>();
        //var weaponId = GetComponentInChildren<Hitbox>();
        HandHitbox[0].weaponDef.weaponId = WeaponIds.UNKNOWN_WEAPON;
        for (int i = 0; i < PoolWeapons.Length; i++)
        {
            PoolWeapons[i].gameObject.SetActive(false);
            HandHitbox[0].enabled = true;
            currentWeapon = null;
        }
        equipController.SetEquipped(WeaponStance.None);
    }

    public bool IsEquipped() => currentWeapon != null;

}
