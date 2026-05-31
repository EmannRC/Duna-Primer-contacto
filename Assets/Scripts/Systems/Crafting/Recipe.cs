using NUnit.Framework.Interfaces;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(menuName = "Crafting/Recipe")]
public class Recipe : ScriptableObject
{
    public List<Ingredient> ingredients;

    public string recipeId;
    public string resultItemId;
    public int resultAmount = 1;
}

[System.Serializable]
public class Ingredient
{
    public string itemId;
    public int amount;
}