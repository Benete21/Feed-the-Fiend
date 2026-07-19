using UnityEngine;

[System.Serializable]
public class Recipe
{
    public Food_Types food;

    public Ingredient_Type[] ingredients;

    public GameObject finishedFoodPrefab;
}