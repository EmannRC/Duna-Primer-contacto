using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class RecipeButtonUI : MonoBehaviour
{
    [SerializeField] private Image icon;
    [SerializeField] private TMP_Text recipeNameText;
    [SerializeField] private Button button;

    private Recipe recipe;
    private CraftingUI craftingUI;

    public void Setup(Recipe recipe, CraftingUI ui)
    {
        this.recipe = recipe;
        this.craftingUI = ui;

        Item item = ui.GetItemDatabase().GetByItemId(recipe.resultItemId);

        if (item != null)
        {
            icon.sprite = item.icon;
            recipeNameText.text = item.itemName;
        }

        button.onClick.AddListener(() =>{
            Debug.Log("CLICK RECIPE: " + recipe.recipeId);
            craftingUI.SelectRecipe(recipe);
        });
    }
}
