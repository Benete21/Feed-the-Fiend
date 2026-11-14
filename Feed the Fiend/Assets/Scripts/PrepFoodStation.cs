
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class PrepFoodStation : MonoBehaviour
{
    private AudioManager audioManager;

    public Recipe[] recipes;

    private List<Ingredient_Type> currentIngredients =
        new List<Ingredient_Type>();

    private List<GameObject> placedIngredients =
        new List<GameObject>();

    [Header("Preparation")]
    [SerializeField] private float preparationTime = 5f;

    [Header("Ingredient Placement")]
    [SerializeField] private Transform snapPoint1;
    [SerializeField] private Transform snapPoint2;
    [SerializeField] private Transform snapPoint3;

    [Header("UI")]
    [SerializeField] private GameObject progressBarObject;
    [SerializeField] private Slider progressBar;

    [Header("Ingredient Settings")]
    [SerializeField] private string ingredientTag = "Ingredient";

    private bool isPreparing;

    private bool tutorialFoodFinished = false;
    private bool foodAtHandoffPoint = false;

    [SerializeField] private Transform tutorialHandoffPoint;

    public bool HasRequiredTutorialIngredients()
    {
        return currentIngredients.Count >= 2;
    }

    public bool HasFinishedTutorialFood()
    {
        return tutorialFoodFinished;
    }

    public Transform GetTutorialHandoffPoint()
    {
        return tutorialHandoffPoint;
    }

    private void Awake()
    {
        GameObject audioObject =
            GameObject.FindGameObjectWithTag("Audio");

        if (audioObject != null)
            audioManager = audioObject.GetComponent<AudioManager>();
    }

    public void StartPreparationPress()
    {
        if (isPreparing)
            return;

        if (currentIngredients.Count < 2)
        {
            Debug.Log("Need at least 2 ingredients to prepare.");
            return;
        }

        StartCoroutine(PrepareIngredient());
    }

    private IEnumerator PrepareIngredient()
    {
        isPreparing = true;

        if (progressBarObject != null)
            progressBarObject.SetActive(true);

        if (progressBar != null)
            progressBar.value = 0f;

        if (audioManager != null)
            audioManager.PlayPreparingFood();

        float timer = 0f;

        while (timer < preparationTime)
        {
            timer += Time.deltaTime;

            if (progressBar != null)
                progressBar.value =
                    Mathf.Clamp01(timer / preparationTime);

            yield return null;
        }

        if (progressBar != null)
            progressBar.value = 1f;

        if (audioManager != null)
            audioManager.StopPreparingFood();

        CheckRecipes();

        if (progressBarObject != null)
            progressBarObject.SetActive(false);

        isPreparing = false;
    }

    public void AddIngredient(GameObject ingredientObject)
    {
        if (isPreparing)
        {
            Debug.Log("Station is currently preparing.");
            return;
        }

        if (ingredientObject == null)
            return;

        // Only allow objects with the Ingredient tag.
        if (!ingredientObject.CompareTag(ingredientTag))
        {
            Debug.Log("Only Ingredient objects can be placed here.");
            return;
        }

        // Make sure it has the correct component.
        Ingredient_Item ingredient =
            ingredientObject.GetComponent<Ingredient_Item>();

        if (ingredient == null)
        {
            Debug.LogWarning(
                ingredientObject.name +
                " has the Ingredient tag but no Ingredient_Item component."
            );
            return;
        }

        if (placedIngredients.Contains(ingredientObject))
        {
            Debug.Log("This ingredient is already on the station.");
            return;
        }

        if (currentIngredients.Count >= 3)
        {
            Debug.Log("Station already has 3 ingredients.");
            return;
        }

        Transform snapPoint = GetNextSnapPoint();

        if (snapPoint == null)
        {
            Debug.LogError("No snap point available!");
            return;
        }

        Rigidbody rb =
            ingredientObject.GetComponent<Rigidbody>();

        if (rb != null)
        {
            rb.linearVelocity = Vector3.zero;
            rb.angularVelocity = Vector3.zero;
            rb.isKinematic = true;
            rb.useGravity = false;
        }

        ingredientObject.transform.SetParent(snapPoint);
        ingredientObject.transform.localPosition = Vector3.zero;
        ingredientObject.transform.localRotation = Quaternion.identity;

        currentIngredients.Add(ingredient.ingredientType);
        placedIngredients.Add(ingredientObject);

        Debug.Log("Added ingredient: " + ingredient.ingredientType);
    }

    // Called when a player takes an ingredient off the station.
    public void RemoveIngredient(GameObject ingredientObject)
    {
        if (ingredientObject == null)
            return;

        int index = placedIngredients.IndexOf(ingredientObject);

        if (index == -1)
            return;

        if (isPreparing)
        {
            Debug.Log("Cannot remove ingredients while preparing.");
            return;
        }

        placedIngredients.RemoveAt(index);
        currentIngredients.RemoveAt(index);

        Debug.Log("Removed ingredient from preparation station.");

        // Reposition remaining ingredients into the correct slots.
        RefreshIngredientPositions();
    }

    private void RefreshIngredientPositions()
    {
        Transform[] snapPoints =
        {
            snapPoint1,
            snapPoint2,
            snapPoint3
        };

        for (int i = 0; i < placedIngredients.Count; i++)
        {
            if (placedIngredients[i] == null)
                continue;

            Transform snapPoint = snapPoints[i];

            if (snapPoint == null)
                continue;

            placedIngredients[i].transform.SetParent(snapPoint);
            placedIngredients[i].transform.localPosition = Vector3.zero;
            placedIngredients[i].transform.localRotation = Quaternion.identity;
        }
    }

    private Transform GetNextSnapPoint()
    {
        switch (currentIngredients.Count)
        {
            case 0:
                return snapPoint1;
            case 1:
                return snapPoint2;
            case 2:
                return snapPoint3;
            default:
                return null;
        }
    }

    private void CheckRecipes()
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

        tutorialFoodFinished = true;

        Food_Spawner foodSpawner =
            GetComponent<Food_Spawner>();

        if (foodSpawner != null)
            foodSpawner.SpawnFood(recipe.finishedFoodPrefab);

        ClearIngredients();
    }

    private void ClearIngredients()
    {
        foreach (GameObject ingredient in placedIngredients)
        {
            if (ingredient != null)
                Destroy(ingredient);
        }

        placedIngredients.Clear();
        currentIngredients.Clear();
    }

    private bool RecipeMatches(Recipe recipe)
    {
        if (recipe.ingredients.Length != currentIngredients.Count)
            return false;

        List<Ingredient_Type> remainingIngredients =
            new List<Ingredient_Type>(currentIngredients);

        foreach (Ingredient_Type requiredIngredient in recipe.ingredients)
        {
            if (!remainingIngredients.Remove(requiredIngredient))
                return false;
        }

        return true;
    }

    public bool HasIngredients()
    {
        return currentIngredients.Count > 0;
    }

    public bool IsPreparing()
    {
        return isPreparing;
    }
}
