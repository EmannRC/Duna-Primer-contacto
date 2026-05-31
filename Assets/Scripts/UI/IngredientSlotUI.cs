using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class IngredientSlotUI : MonoBehaviour
{
    [SerializeField] private Image icon;
    [SerializeField] private TMP_Text amountText;

    private Ingredient ingredient;
    private Inventory inventory;

    //====================================================================//
    public void Setup(Ingredient ing, Inventory inv, ItemDatabase db)
    {
        ingredient = ing;
        inventory = inv;

        var item = db.GetByItemId(ing.itemId);

        if (item != null)
            icon.sprite = item.icon;

        Refresh();
    }

    public void Refresh()
    {
        int currentAmount = 0;

        foreach (var slot in inventory.items)
        {
            if (slot.itemId == ingredient.itemId)
            {
                currentAmount = slot.amount;
                break;
            }
        }

        amountText.text = $"{currentAmount} / {ingredient.amount}";

        amountText.color = currentAmount >= ingredient.amount
            ? Color.white
            : Color.red;
    }
}
