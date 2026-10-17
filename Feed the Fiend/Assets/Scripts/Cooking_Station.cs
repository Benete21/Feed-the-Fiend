
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

        [Header("Cooking Results")]
        public GameObject perfectFoodPrefab;
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
    private float cookingTimer;

    public bool HasFoodOnStove => foodOnStove != null;
    public bool IsCooking => isCooking;

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

    // The trigger detects the chef only.
    // It never automatically picks up or places food.
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

    // Called by Chef_Controls when the chef deliberately
    // places a held ingredient on the stove.
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

        bool hasRecipe = false;

        foreach (CookableFood recipe in cookableFoods)
        {
            if (recipe.rawIngredeitnsType == ingredient.ingredientType)
            {
                hasRecipe = true;
                break;
            }
        }

        if (!hasRecipe)
        {
            Debug.LogWarning(
                "No cooking recipe for " + ingredient.ingredientType
            );
            return false;
        }

        foodOnStove = ingredientObject;

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

        Debug.Log("Ingredient placed on stove: " +
                  ingredient.ingredientType);

        return true;
    }

    // Called by the controller's Prep button.
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

        if (cookingUI != null)
            cookingUI.SetActive(true);

        UpdateCookingUI();

        Debug.Log("Cooking started: " + ingredient.ingredientType);
    }

    private void TryCollectFood()
    {
        if (cookingTimer < perfectCookTime)
        {
            Debug.Log("Not ready yet!");
            return;
        }

        if (cookingTimer < burnTime)
            SpawnCookingResult(true);
    }

    private void UpdateCookingTimer()
    {
        cookingTimer += Time.deltaTime;

        UpdateCookingUI();

        if (cookingTimer >= burnTime)
            SpawnCookingResult(false);
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
                cookingText.text = "PERFECT! Press Prep!";
            else
                cookingText.text = "BURNT!";
        }
    }

    private void SpawnCookingResult(bool perfect)
    {
        if (foodOnStove == null)
            return;

        Ingredient_Item ingredient =
            foodOnStove.GetComponentInChildren<Ingredient_Item>();

        if (ingredient == null)
        {
            Debug.LogWarning("Ingredient_Item component is missing.");
            ResetStation();
            return;
        }

        CookableFood recipe = null;

        foreach (CookableFood food in cookableFoods)
        {
            if (food.rawIngredeitnsType == ingredient.ingredientType)
            {
                recipe = food;
                break;
            }
        }

        if (recipe == null)
        {
            ResetStation();
            return;
        }

        GameObject resultPrefab = perfect
            ? recipe.perfectFoodPrefab
            : recipe.burntFoodPrefab;

        if (resultPrefab == null)
        {
            Debug.LogWarning("Cooking result prefab is missing!");
            ResetStation();
            return;
        }

        Vector3 position = foodOnStove.transform.position;
        Quaternion rotation = foodOnStove.transform.rotation;

        Destroy(foodOnStove);

        foodOnStove = Instantiate(resultPrefab, position, rotation);

        if (foodPoint != null)
        {
            foodOnStove.transform.SetParent(foodPoint);
            foodOnStove.transform.localPosition = Vector3.zero;
            foodOnStove.transform.localRotation = Quaternion.identity;
        }

        isCooking = false;
        cookingTimer = 0f;

        if (cookingUI != null)
            cookingUI.SetActive(false);

        Debug.Log(perfect ? "Perfect food cooked!" : "Food burnt!");
    }

    // Called by Chef_Controls to take the cooked food.
    public GameObject TryTakeFood()
    {
        if (!playerNearby || isCooking || foodOnStove == null)
            return null;

        GameObject result = foodOnStove;
        foodOnStove = null;

        result.transform.SetParent(null);

        Rigidbody rb = result.GetComponent<Rigidbody>();

        if (rb != null)
        {
            rb.isKinematic = false;
            rb.useGravity = true;
        }

        Debug.Log("Food collected from cooking station.");

        return result;
    }

    private void ResetStation()
    {
        isCooking = false;
        cookingTimer = 0f;
        foodOnStove = null;

        if (cookingUI != null)
            cookingUI.SetActive(false);
    }
    public bool HasIngredients()
    {
        return foodOnStove != null && !isCooking;
    }

    public bool HasCookedFood()
    {
        return foodOnStove != null && !isCooking
            && cookingTimer == 0f
            && foodOnStove.GetComponentInChildren<Ingredient_Item>() == null;
    }

    public bool IsReadyToCook()
    {
        return foodOnStove != null && !isCooking
            && foodOnStove.GetComponentInChildren<Ingredient_Item>() != null;
    }

    public bool IsReadyToCollect()
    {
        return foodOnStove != null && !isCooking
            && foodOnStove.GetComponentInChildren<Ingredient_Item>() == null;
    }
}
