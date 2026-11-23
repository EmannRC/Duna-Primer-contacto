using System;
using System.Collections;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class CraftingUI : MonoBehaviour
{
    private Inventory playerInventory;
    private PlayerCrafting playerCrafting;

    [Header("Data")]
    [SerializeField] private List<Recipe> recipes;
    [SerializeField] private ItemDatabase itemDatabase;

    [Header("UI")]
    [SerializeField] private Transform recipeContainer;
    [SerializeField] private GameObject recipeButtonPrefab;

    [SerializeField] private Image resultIcon;
    [SerializeField] private TMP_Text resultName;

    [SerializeField] private Transform ingredientContainer;
    [SerializeField] private GameObject ingredientSlotPrefab;

    [SerializeField] private Button craftButton;

    private Recipe selectedRecipe;

    //====================================================//
    private void Start()
    {
        craftButton.onClick.AddListener(CraftSelected);
    }

    //====================================================//
    public void Bind(Inventory inv, PlayerCrafting crafting)
    {
        // limpiar eventos viejos
        if (playerInventory != null)
            playerInventory.OnInventoryChanged -= RefreshUI;

        playerInventory = inv;
        playerCrafting = crafting;

        playerInventory.OnInventoryChanged += RefreshUI;

        GenerateRecipeList();
        RefreshUI();
    }

    private void OnDisable()
    {
        if (playerInventory != null)
            playerInventory.OnInventoryChanged -= RefreshUI;
    }

    //====================================================//
    void GenerateRecipeList()
    {
        foreach (Transform child in recipeContainer)
            Destroy(child.gameObject);

        foreach (var recipe in recipes)
        {
            var go = Instantiate(recipeButtonPrefab, recipeContainer);
            go.GetComponent<RecipeButtonUI>().Setup(recipe, this);
        }
    }

    //====================================================//
    public void SelectRecipe(Recipe recipe)
    {
        if (playerInventory == null || itemDatabase == null)
        {
            Debug.LogError("CraftingUI no está inicializado correctamente (inventory o itemDatabase null)");
            return;
        }

        selectedRecipe = recipe;

        var resultItem = itemDatabase.GetByItemId(recipe.resultItemId);

        if (resultItem != null)
        {
            resultIcon.sprite = resultItem.icon;
            resultName.text = resultItem.itemName;
        }

        GenerateIngredients(recipe);
        RefreshUI();
    }

    //====================================================//
    void GenerateIngredients(Recipe recipe)
    {
        foreach (Transform child in ingredientContainer)
            Destroy(child.gameObject);

        foreach (var ing in recipe.ingredients)
        {
            var go = Instantiate(ingredientSlotPrefab, ingredientContainer);
            go.GetComponent<IngredientSlotUI>().Setup(ing, playerInventory, itemDatabase);
        }
    }

    //====================================================//
    void RefreshUI()
    {
        if (selectedRecipe == null || playerCrafting == null)
            return;

        craftButton.interactable = playerCrafting.CanCraft(selectedRecipe);

        foreach (Transform child in ingredientContainer)
        {
            var slot = child.GetComponent<IngredientSlotUI>();
            if (slot != null)
                slot.Refresh();
        }
    }

    //====================================================//
    void CraftSelected()
    {
        if (selectedRecipe == null) return;

        playerCrafting.RequestCraft(selectedRecipe.recipeId);
    }

    public ItemDatabase GetItemDatabase()
    {
        return itemDatabase;
    }
}

