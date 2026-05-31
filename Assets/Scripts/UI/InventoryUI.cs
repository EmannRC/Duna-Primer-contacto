using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.UI;

public class InventoryUI : MonoBehaviour
{
    [Header("Inventory")]
    public Inventory inventory;
    public InventorySlotUI[] slots;
    [SerializeField] private ItemDatabase itemDatabase;

    [Header("Stats")]
    public PlayerStatsManager stats;

    [SerializeField] private StatRowUI powerRow;
    [SerializeField] private StatRowUI armorRow;
    [SerializeField] private StatRowUI moveSpeedRow;
    [SerializeField] private StatRowUI manaRow;

    [Header("Equip")]
    public PlayerEquipment equipment;
    [SerializeField] private Image weaponIcon;
    [SerializeField] private Image armorIcon;

    [SerializeField] private ItemContextMenuUI contextMenu;

    private PlayerContext ctx;

    //===========================================================//
    public void Bind(
        Inventory inv,
        PlayerStatsManager st,
        PlayerEquipment eq,
        PlayerContext playerCtx)
    {
        inventory = inv;
        stats = st;
        equipment = eq;
        ctx = playerCtx;

        foreach (var slot in slots)
        {
            slot.Setup(
                inventory,
                contextMenu,
                ctx
            );
        }

        inventory.OnInventoryChanged += UpdateUI;
        inventory.OnInventoryChanged += UpdateStatsUI;

        stats.OnStatsChanged += UpdateStatsUI;
        equipment.OnEquipmentChanged += UpdateStatsUI;

        UpdateUI();
        UpdateStatsUI();
    }

    //===========================================================//
    void UpdateUI()
    {
        for (int i = 0; i < slots.Length; i++)
        {
            if (i < inventory.items.Count)
            {
                Item item = itemDatabase.GetByItemId(inventory.items[i].itemId.ToString());

                slots[i].SetItem(
                    item,
                    inventory.items[i].amount
                );
            }
            else
            {
                slots[i].SetItem(null, 0);
            }
        }
    }

    //===========================================================//
    void UpdateStatsUI()
    {
        
        // Stats
        powerRow.SetValue(stats.GetStat(StatType.Power), 50);
        armorRow.SetValue(stats.GetStat(StatType.Armor), 50);
        moveSpeedRow.SetValue(stats.GetStat(StatType.MoveSpeed), 15);
        manaRow.SetValue(stats.GetStat(StatType.Mana), 200);

        //  ARMA 
        if (equipment.weapon != null)
        {
            weaponIcon.sprite = equipment.weapon.icon;
            weaponIcon.enabled = true;
        }
        else
        {
            weaponIcon.enabled = false;
        }

        // PETO
        if (equipment.armor != null)
        {
            armorIcon.sprite = equipment.armor.icon;
            armorIcon.enabled = true;
        }
        else
        {
            armorIcon.enabled = false;
        }
    }

    //===========================================================//
    void OnDestroy()
    {
        inventory.OnInventoryChanged -= UpdateUI;
        inventory.OnInventoryChanged -= UpdateStatsUI;

        stats.OnStatsChanged -= UpdateStatsUI;
        equipment.OnEquipmentChanged -= UpdateStatsUI;
    }

    
}
