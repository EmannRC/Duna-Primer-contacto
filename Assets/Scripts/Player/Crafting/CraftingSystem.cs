using Unity.Netcode;
using UnityEngine;

public class CraftingSystem
{
    public bool CanCraft(Recipe recipe, Inventory inventory)
    {
        foreach (var ing in recipe.ingredients)
        {
            if (!inventory.HasItem(ing.itemId, ing.amount))
                return false;
        }

        return true;
    }

    public void Craft(Recipe recipe, Inventory inventory)
    {
        foreach (var ing in recipe.ingredients)
        {
            inventory.RemoveItem(ing.itemId, ing.amount);
        }

        inventory.AddItem(recipe.resultItemId, recipe.resultAmount);
    }
}
