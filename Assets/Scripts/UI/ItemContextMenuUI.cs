using UnityEngine;
using UnityEngine.UI;

public class ItemContextMenuUI : MonoBehaviour
{
    [SerializeField] GameObject panel;

    [SerializeField] Button equipButton;
    [SerializeField] Button consumeButton;
    [SerializeField] Button dropButton;
    [SerializeField] Button inspectButton;

    private Item currentItem;
    private Inventory currentInventory;
    private PlayerContext ctx;

    //====================================================//
    public void Show(
        Item item,
        Inventory inventory,
        PlayerContext playerContext,
        Vector3 pos)
    {
        ctx = playerContext;

        currentItem = item;
        currentInventory = inventory;

        panel.SetActive(true);
        panel.transform.position = pos;

        equipButton.gameObject.SetActive(item.CanEquip);
        consumeButton.gameObject.SetActive(item.CanConsume);
        dropButton.gameObject.SetActive(item.CanDrop);
        inspectButton.gameObject.SetActive(item.CanInspect);
    }

    public void Hide()
    {
        panel.SetActive(false);
    }

    //====================================================//
    public void OnEquip()
    {
        if (currentItem is not EquipableItem equipable)
            return;

        ctx.equipment.Equip(equipable);
        Hide();
    }

    //====================================================//
    public void OnConsume()
    {
        ctx.actions.RequestConsumeItemServerRpc(currentItem.itemId);
        Hide();
    }

    //====================================================//
    public void OnDrop()
    {
        ctx.actions.RequestDropItemServerRpc(currentItem.itemId);
        Hide();
    }
}
