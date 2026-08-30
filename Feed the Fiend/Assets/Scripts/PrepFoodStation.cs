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

        if (currentIngredients.Count < 2)
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
        progressBar.value = 1f;

        CheckRecipes();

        progressBarObject.SetActive(false);

        isPreparing = false;
    }

    public void AddIngredient(GameObject I)
    {
        if (isPreparing)
        {
            Debug.Log("Station is currently preparing.");
            return;
        }

        if (currentIngredients.Count >= 3)
        {
            Debug.Log("Station already has 3 ingredients.");
            return;
        }

        Ingredient_Item ingredient = I.GetComponent<Ingredient_Item>();

        if (ingredient == null)
        {
            Debug.LogError(
                I.name + " does not have an Ingredient_Item component!"
            );
            return;
        }

        Transform snapPoint = GetNextSnapPoint();

        if (snapPoint == null)
        {
            Debug.LogError("No snap point available!");
            return;
        }

        Debug.Log(
            "SNAPPING " + I.name +
            " TO " + snapPoint.name
        );

        // Remove from previous parent
        I.transform.SetParent(null);

        Rigidbody rb = I.GetComponent<Rigidbody>();

        if (rb != null)
        {
            rb.linearVelocity = Vector3.zero;
            rb.angularVelocity = Vector3.zero;

            rb.isKinematic = true;
            rb.useGravity = false;
        }

        // Snap
        I.transform.SetParent(snapPoint);

        I.transform.localPosition = Vector3.zero;
        I.transform.localRotation = Quaternion.identity;

        // Add to lists AFTER successful snap
        currentIngredients.Add(ingredient.ingredientType);
        placedIngredients.Add(I);

        Debug.Log(
            "Added ingredient: " +
            ingredient.ingredientType
        );

        if (currentIngredients.Count >= 2)
        {
            StartPreparation();
        }
    }


    private Transform GetNextSnapPoint()
    {
        switch (currentIngredients.Count)
        {
            case 1:
                return snapPoint1;

            case 2:
                return snapPoint2;

            case 3:
                return snapPoint3;

            default:
                return null;
        }
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
        ClearIngredients();
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

