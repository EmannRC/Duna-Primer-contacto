using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(menuName = "Crafting/Recipe Database")]
public class RecipeDataBase : ScriptableObject
{
    public List<Recipe> recipes;

    private Dictionary<string, Recipe> recipeById;

    // =========================
    // INIT (opcional pero recomendado)
    // =========================
    public void Init()
    {
        recipeById = new Dictionary<string, Recipe>();

        foreach (var recipe in recipes)
        {
            if (recipe == null) continue;

            recipeById[recipe.recipeId] = recipe;
        }
    }

    // =========================
    // GET BY ID
    // =========================
    public Recipe Get(string recipeId)
    {
        if (recipeById == null)
            Init();

        recipeById.TryGetValue(recipeId, out var recipe);
        return recipe;
    }

    // =========================
    // DEBUG / SAFETY
    // =========================
    public bool Exists(string recipeId)
    {
        if (recipeById == null)
            Init();

        return recipeById.ContainsKey(recipeId);
    }
}
