using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using System.Collections;

public class PrepFoodStation : MonoBehaviour
{
    public Recipe[] recipes;

    private List<Ingredient_Type> currentIngredients = new List<Ingredient_Type>();

    private List<GameObject> placedIngredients = new List<GameObject>();

    [Header("Preparation")]
    [SerializeField] private float preparationTime = 5f;

    [Header("Ingredient Placement")]
    [SerializeField] private Transform snapPoint1;
    [SerializeField] private Transform snapPoint2;
    [SerializeField] private Transform snapPoint3;

    [Header("UI")]
    [SerializeField] private GameObject progressBarObject;
    [SerializeField] private Slider progressBar;


    private bool isPreparing;
    public void StartPreparation()
    {
        if (isPreparing)
            return;

        if (currentIngredients.Count == 0)
            return;

        StartCoroutine(PrepareIngredient());
    }

    private IEnumerator PrepareIngredient()
    {
            isPreparing = true;

            progressBarObject.SetActive(true);
            progressBar.value = 0f;

            float timer = 0f;

            while (timer < preparationTime)
            {
                timer += Time.deltaTime;

                progressBar.value = Mathf.Clamp01(timer / preparationTime);

                yield return null;
            }

            CheckRecipes();

            progressBarObject.SetActive(false);
            isPreparing = false;
        }

    public void AddIngredient(GameObject I)
    {
        if (isPreparing)
            return;

        Ingredient_Item ingredient = I.GetComponent<Ingredient_Item>();

        if (ingredient == null)
            return;

        currentIngredients.Add(ingredient.ingredientType);
        placedIngredients.Add(I);

        Rigidbody rb = I.GetComponent<Rigidbody>();

        if (rb != null)
        {
            rb.linearVelocity = Vector3.zero;
            rb.angularVelocity = Vector3.zero;

            rb.useGravity = false;
            rb.isKinematic = true;
        }

        // Snap to prep station
        I.transform.SetParent(snapPoint1);
        I.transform.localPosition = Vector3.zero;
        I.transform.localRotation = Quaternion.identity;

        Debug.Log("Added ingredient: " + ingredient.ingredientType);
    }


    void CheckRecipes()
    {
        foreach (Recipe recipe in recipes)
        {
            if (RecipeMatches(recipe))
            {
                MakeFood(recipe);

                return;
            }
        }
        ClearIngredients();
    }
    private void MakeFood(Recipe recipe)
    {
        Debug.Log("Made " + recipe.food);

        currentIngredients.Clear();

        Food_Spawner foodSpawner = GetComponent<Food_Spawner>();

        if (foodSpawner != null)
        {
            foodSpawner.SpawnFood(recipe.finishedFoodPrefab);
        }
    }

    private void ClearIngredients()
    {
        foreach (GameObject ingredient in placedIngredients)
        {
            if (ingredient != null)
            {
                Destroy(ingredient);
            }
        }

        placedIngredients.Clear();
        currentIngredients.Clear();
    }

    private bool RecipeMatches(Recipe recipe)
    {
        if (recipe.ingredients.Length != currentIngredients.Count)
            return false;

        List<Ingredient_Type> remainingIngredients = new List<Ingredient_Type>(currentIngredients);

        foreach (Ingredient_Type requiredIngredient in recipe.ingredients)
        {
            if (!remainingIngredients.Remove(requiredIngredient))
                return false;
        }

        return true;
    }
}

