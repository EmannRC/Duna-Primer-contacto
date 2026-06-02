using Unity.Netcode;
using UnityEngine;

public class PlayerCrafting : NetworkBehaviour
{
    [SerializeField] private RecipeDataBase recipeDatabase;
    [SerializeField] private Inventory inventory;

    private CraftingSystem craftingSystem = new CraftingSystem();

    // ========================================================//
    public bool CanCraft(Recipe recipe)
    {
        return craftingSystem.CanCraft(recipe, inventory);
    }

    // ========================================================//
    public void RequestCraft(string recipeId)
    {
        CraftServerRpc(recipeId);
    }

    // ========================================================//
    [ServerRpc]
    private void CraftServerRpc(string recipeId)
    {
        Recipe recipe = recipeDatabase.Get(recipeId);
        if (recipe == null) return;

        if (!craftingSystem.CanCraft(recipe, inventory))
            return;

        craftingSystem.Craft(recipe, inventory);

        Debug.Log("CRAFTED: " + recipe.resultItemId);
    }
}
