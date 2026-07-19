using System.Collections.Generic;
using UnityEngine;

public class PrepFoodStation : MonoBehaviour
{
    public Recipe[] recipes;

    private List<Ingredient_Type> currentIngredients = new List<Ingredient_Type>();

    public Transform spawnPoint;

    public void AddIngredient(GameObject I)
    {
        Ingredient_Item ingredient = I.GetComponent<Ingredient_Item>();

        if (ingredient == null)
            return;

        currentIngredients.Add(ingredient.ingredientType);

        Destroy(I);

        CheckRecipes();
    }

    void CheckRecipes()
    {
        foreach (Recipe recipe in recipes)
        {
            if (RecipeMatches(recipe))
            {
                Instantiate(recipe.finishedFoodPrefab,
                            spawnPoint.position,
                            Quaternion.identity);

                currentIngredients.Clear();

                Debug.Log("Made " + recipe.food);

                return;
            }
        }
    }

    bool RecipeMatches(Recipe recipe)
    {
        if (recipe.ingredients.Length != currentIngredients.Count)
            return false;

        foreach (Ingredient_Type ingredient in recipe.ingredients)
        {
            if (!currentIngredients.Contains(ingredient))
                return false;
        }

        return true;
    }
}

