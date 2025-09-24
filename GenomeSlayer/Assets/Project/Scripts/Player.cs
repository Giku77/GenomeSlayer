using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;

public class Player : Entity
{
    private Animator animator;
    private CapsuleCollider capsuleCollider;
    private static readonly int hashDie = Animator.StringToHash("Die");
    public UIManager uiManager;
    public QuickSlotInventory quickSlotInventory = new QuickSlotInventory(7);
    public float defense { get; set; }

private void Awake()
    {
        animator = GetComponent<Animator>();
        capsuleCollider = GetComponent<CapsuleCollider>();
    }

    private void Start()
    {
        EventBus.WireToEventBus(
            quickSlotInventory,
            itemId =>
            {
                if (DataTableManger.ItemTable.TryGetItem(itemId, out var item))
                    return item.itemName;

                if (DataTableManger.EquipmentTable.TryGetItem(itemId, out var equip))
                    return equip.equipName; 

                return string.Empty; 
            }
        );
    }

    public void Heal(int amount)
    {
        health += amount;
        if (health > maxhealth)
        {
            health = maxhealth;
        }
        uiManager.UpdateHealth(health, maxhealth);
    }
    protected override void OnEnable()
    {
        //base.OnEnable();
        health = 1000;
        maxhealth = 1000;
    }
    public override void OnDamage(int damage)
    {
        if (uiManager.ActiveArmor.activeSelf)
        {
            damage = Mathf.Max(0, damage - (int)defense);
            uiManager.ActiveArmorSlot.UpdateDur();
            if (uiManager.ActiveArmorSlot.durSlider.value <= 0)
            {
                uiManager.SetActiveAromor(false);
            }
        }
        base.OnDamage(damage);
        uiManager.UpdateHealth(health, maxhealth);
        //Debug.Log($"Player OnDamage {damage}, health {health}");
    }

    public void Attack()
    {
        var target = FindTarget(2.0f);
        if (target != null)
        {
            var entity = target.GetComponent<Enemy>();
            if (entity != null)
            {
                entity.OnDamage(damage);
            }
        }
    }

    protected override void Die()
    {
        capsuleCollider.enabled = false;
        var rb = GetComponent<Rigidbody>();
        if (rb)
        {
            rb.isKinematic = true;
            rb.detectCollisions = false;
        }
        animator.SetTrigger(hashDie);
        StartCoroutine(Restart());
        //Destroy(gameObject, 5f);
    }


    private IEnumerator Restart()
    {
        yield return new WaitForSeconds(5f);
        SceneManager.LoadScene(SceneManager.GetActiveScene().name);
    }

}
