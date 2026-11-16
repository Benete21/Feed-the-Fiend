
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class Cooking_Station : MonoBehaviour
{
    [System.Serializable]
    public class CookableFood
    {
        [Header("Input Ingredient")]
        public Ingredient_Type rawIngredeitnsType;

        [Header("Finished Food")]
        public GameObject cookedFoodPrefab;

        [Header("Burnt Food")]
        public GameObject burntFoodPrefab;
    }

    [Header("Food Recipes")]
    public CookableFood[] cookableFoods;

    [Header("Cooking Times")]
    public float perfectCookTime = 5f;
    public float burnTime = 8f;

    [Header("Food Placement")]
    public Transform foodPoint;

    [Header("UI")]
    public GameObject cookingUI;
    public Slider cookingBar;
    public TMP_Text cookingText;

    private GameObject foodOnStove;
    private bool isCooking;
    private bool playerNearby;
    private bool hasBecomeCookedFood;
    private CookableFood currentRecipe;

    private float cookingTimer;

    public bool HasFoodOnStove => foodOnStove != null;
    public bool IsCooking => isCooking;

    private AudioManager audioManager;

    private void Awake()
    {
        GameObject audioObject =
            GameObject.FindGameObjectWithTag("Audio");

        if (audioObject != null)
            audioManager = audioObject.GetComponent<AudioManager>();
    }

    private void Start()
    {
        if (cookingUI != null)
            cookingUI.SetActive(false);
    }

    private void Update()
    {
        if (isCooking)
            UpdateCookingTimer();
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Chef"))
            playerNearby = true;
    }

    private void OnTriggerExit(Collider other)
    {
        if (other.CompareTag("Chef"))
            playerNearby = false;
    }

    public bool IsChefNearby(Vector3 chefPosition)
    {
        return playerNearby;
    }

    public bool TryPlaceIngredient(GameObject ingredientObject)
    {
        if (ingredientObject == null || isCooking ||
            foodOnStove != null)
            return false;

        Ingredient_Item ingredient =
            ingredientObject.GetComponentInChildren<Ingredient_Item>();

        if (ingredient == null)
        {
            Debug.Log("Only ingredients can be placed on this stove.");
            return false;
        }

        CookableFood matchingRecipe = null;

        foreach (CookableFood recipe in cookableFoods)
        {
            if (recipe.rawIngredeitnsType == ingredient.ingredientType)
            {
                matchingRecipe = recipe;
                break;
            }
        }

        if (matchingRecipe == null)
        {
            Debug.LogWarning(
                "No cooking recipe for " + ingredient.ingredientType
            );
            return false;
        }

        foodOnStove = ingredientObject;
        currentRecipe = matchingRecipe;

        if (foodPoint != null)
        {
            foodOnStove.transform.SetParent(foodPoint);
            foodOnStove.transform.localPosition = Vector3.zero;
            foodOnStove.transform.localRotation = Quaternion.identity;
        }

        Rigidbody rb = foodOnStove.GetComponent<Rigidbody>();

        if (rb != null)
        {
            rb.linearVelocity = Vector3.zero;
            rb.angularVelocity = Vector3.zero;
            rb.isKinematic = true;
            rb.useGravity = false;
        }

        hasBecomeCookedFood = false;
        cookingTimer = 0f;

        Debug.Log("Ingredient placed on stove: " +
                  ingredient.ingredientType);

        return true;
    }

    public void HandlePrepButton()
    {
        if (!playerNearby)
            return;

        if (foodOnStove == null)
        {
            Debug.Log("Place an ingredient on the stove first.");
            return;
        }

        if (!isCooking)
        {
            StartCooking();
        }
        else
        {
            TryCollectFood();
        }
    }

    private void StartCooking()
    {
        Ingredient_Item ingredient =
            foodOnStove.GetComponentInChildren<Ingredient_Item>();

        if (ingredient == null)
        {
            Debug.LogWarning("Ingredient_Item component is missing.");
            return;
        }

        isCooking = true;
        cookingTimer = 0f;
        hasBecomeCookedFood = false;

        if (cookingUI != null)
            cookingUI.SetActive(true);

        UpdateCookingUI();

        Debug.Log("Cooking started: " + ingredient.ingredientType);

        if (audioManager != null)
            audioManager.PlayCookingFood();
    }

    private void TryCollectFood()
    {
        if (cookingTimer < perfectCookTime)
        {
            Debug.Log("Not ready yet!");
            return;
        }

        // Keep cooking and the sound running.
        // The chef must pick up the food to stop it.
        Debug.Log("Food is ready to be collected!");
    }

    private void UpdateCookingTimer()
    {
        cookingTimer += Time.deltaTime;

        // Replace raw ingredient with cooked food.
        if (!hasBecomeCookedFood &&
            cookingTimer >= perfectCookTime)
        {
            CreateFinishedFood();

            if (foodOnStove == null)
                return;
        }

        UpdateCookingUI();

        // Replace cooked food with burnt food.
        if (cookingTimer >= burnTime)
        {
            cookingTimer = burnTime;
            isCooking = false;

            CreateBurntFood();

            if (cookingUI != null)
                cookingUI.SetActive(false);

            // Stop sound because food has burnt.
            if (audioManager != null)
                audioManager.StopCookingFood();

            Debug.Log("Food has burnt!");
        }
    }

    private CookableFood GetCurrentRecipe()
    {
        return currentRecipe;
    }

    private void CreateFinishedFood()
    {
        CookableFood recipe = GetCurrentRecipe();

        if (recipe == null || recipe.cookedFoodPrefab == null)
        {
            Debug.LogWarning("Cooked food prefab is missing.");
            ResetStation();
            return;
        }

        ReplaceFoodPrefab(recipe.cookedFoodPrefab);

        hasBecomeCookedFood = true;

        // Do not stop the cooking sound here.
        // It continues until collection or burning.

        Debug.Log("Food is perfectly cooked!");
    }

    private void CreateBurntFood()
    {
        CookableFood recipe = GetCurrentRecipe();

        if (recipe == null)
        {
            Debug.LogWarning("Cannot burn food: no cooking recipe is assigned.");
            return;
        }

        if (recipe.burntFoodPrefab == null)
        {
            Debug.LogWarning(
                "Burnt food prefab is missing for recipe: " +
                recipe.rawIngredeitnsType
            );
            return;
        }

        ReplaceFoodPrefab(recipe.burntFoodPrefab);

        hasBecomeCookedFood = false;

        Debug.Log("Food replaced with burnt prefab!");
    }

    private void ReplaceFoodPrefab(GameObject newPrefab)
    {
        if (foodOnStove == null || newPrefab == null)
            return;

        Vector3 position = foodOnStove.transform.position;
        Quaternion rotation = foodOnStove.transform.rotation;

        Destroy(foodOnStove);

        foodOnStove = Instantiate(
            newPrefab,
            position,
            rotation
        );

        if (foodPoint != null)
        {
            foodOnStove.transform.SetParent(foodPoint);
            foodOnStove.transform.localPosition = Vector3.zero;
            foodOnStove.transform.localRotation = Quaternion.identity;
        }

        Rigidbody rb = foodOnStove.GetComponent<Rigidbody>();

        if (rb != null)
        {
            rb.linearVelocity = Vector3.zero;
            rb.angularVelocity = Vector3.zero;
            rb.isKinematic = true;
            rb.useGravity = false;
        }
    }

    private void UpdateCookingUI()
    {
        if (cookingBar != null)
        {
            cookingBar.value = burnTime > 0f
                ? Mathf.Clamp01(cookingTimer / burnTime)
                : 1f;
        }

        if (cookingText != null)
        {
            if (cookingTimer < perfectCookTime)
                cookingText.text = "Cooking...";
            else if (cookingTimer < burnTime)
                cookingText.text = "PERFECT! Press B to collect!";
            else
                cookingText.text = "BURNT!";
        }
    }

    public GameObject TryTakeFood()
    {
        if (!playerNearby ||
            foodOnStove == null ||
            !hasBecomeCookedFood)
            return null;

        // Stop cooking when chef collects the food.
        isCooking = false;

        if (audioManager != null)
            audioManager.StopCookingFood();

        if (cookingUI != null)
            cookingUI.SetActive(false);

        GameObject result = foodOnStove;

        // Clear the station.
        foodOnStove = null;
        currentRecipe = null;
        hasBecomeCookedFood = false;
        cookingTimer = 0f;

        result.transform.SetParent(null);

        Rigidbody rb = result.GetComponent<Rigidbody>();

        if (rb != null)
        {
            rb.isKinematic = false;
            rb.useGravity = true;
            rb.linearVelocity = Vector3.zero;
            rb.angularVelocity = Vector3.zero;
        }

        Debug.Log("Perfectly cooked food collected!");

        return result;
    }

    private void ResetStation()
    {
        isCooking = false;
        cookingTimer = 0f;
        foodOnStove = null;
        currentRecipe = null;
        hasBecomeCookedFood = false;

        if (audioManager != null)
            audioManager.StopCookingFood();

        if (cookingUI != null)
            cookingUI.SetActive(false);
    }

    public bool HasIngredients()
    {
        return foodOnStove != null &&
               !isCooking &&
               !hasBecomeCookedFood;
    }

    public bool HasCookedFood()
    {
        return foodOnStove != null &&
               hasBecomeCookedFood;
    }

    public bool IsReadyToCook()
    {
        return foodOnStove != null &&
               !isCooking &&
               !hasBecomeCookedFood &&
               foodOnStove.GetComponentInChildren<Ingredient_Item>() != null;
    }

    public bool IsReadyToCollect()
    {
        return foodOnStove != null &&
               hasBecomeCookedFood;
    }
}
