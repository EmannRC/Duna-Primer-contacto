using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class InventorySlotUI : MonoBehaviour
{
    public Image icon;
    public TextMeshProUGUI amountText;

    private Item item;
    private Inventory inventory;
    private ItemContextMenuUI contextMenu;
    private PlayerContext ctx;

    //============================================//
    public void Setup(
        Inventory inv,
        ItemContextMenuUI menu,
        PlayerContext playerCtx)
    {
        inventory = inv;
        contextMenu = menu;
        ctx = playerCtx;
    }

    //============================================//
    public void SetItem(Item newItem, int amount)
    {
        item = newItem;

        if (item != null)
        {
            icon.sprite = item.icon;
            icon.enabled = true;
            amountText.text = amount > 1 ? amount.ToString() : "";
        }
        else
        {
            icon.enabled = false;
            amountText.text = "";
        }
    }

    //============================================//
    public void OnClick()
    {
        if (item == null)
            return;

        contextMenu.Show(
            item,
            inventory,
            ctx,
            transform.position
        );
    }
}
