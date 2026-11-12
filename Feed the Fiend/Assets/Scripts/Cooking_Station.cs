
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
        public GameObject finishedFoodPrefab;
    }

    [Header("Food Recipes")]
    public CookableFood[] cookableFoods;

    [Header("Cooking Times")]
    public float perfectCookTime = 5f;
    public float burnTime = 8f;

    [Header("Burnt Appearance")]
    [Range(0f, 1f)]
    public float burntDarkness = 0.7f;

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

    private float cookingTimer;

    private Renderer[] foodRenderers;

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

        hasBecomeCookedFood = false;
        foodRenderers = null;

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
    }

    private void TryCollectFood()
    {
        if (cookingTimer < perfectCookTime)
        {
            Debug.Log("Not ready yet!");
            return;
        }

        // Stop cooking when the player chooses to collect
        // the food after it has reached perfect cook time.
        isCooking = false;

        if (cookingUI != null)
            cookingUI.SetActive(false);

        Debug.Log("Food collected at its current cookedness.");
    }

    private void UpdateCookingTimer()
    {
        cookingTimer += Time.deltaTime;

        // Replace the raw ingredient with the finished food
        // as soon as it reaches perfect cook time.
        if (!hasBecomeCookedFood &&
            cookingTimer >= perfectCookTime)
        {
            CreateFinishedFood();

            if (foodOnStove == null)
                return;
        }

        // Gradually darken the finished food as it burns.
        if (hasBecomeCookedFood)
        {
            float burnProgress = Mathf.InverseLerp(
                perfectCookTime,
                burnTime,
                cookingTimer
            );

            ApplyCookedness(burnProgress * burntDarkness);
        }

        UpdateCookingUI();

        // The food becomes fully burnt if the player
        // leaves it cooking until burn time.
        if (cookingTimer >= burnTime)
        {
            cookingTimer = burnTime;
            isCooking = false;

            ApplyCookedness(burntDarkness);

            if (cookingUI != null)
                cookingUI.SetActive(false);

            Debug.Log("Food has burnt!");
        }
    }

    private void CreateFinishedFood()
    {
        if (foodOnStove == null)
            return;

        Ingredient_Item ingredient =
            foodOnStove.GetComponentInChildren<Ingredient_Item>();

        if (ingredient == null)
            return;

        CookableFood recipe = null;

        foreach (CookableFood food in cookableFoods)
        {
            if (food.rawIngredeitnsType == ingredient.ingredientType)
            {
                recipe = food;
                break;
            }
        }

        if (recipe == null || recipe.finishedFoodPrefab == null)
        {
            Debug.LogWarning(
                "Finished food prefab is missing for " +
                ingredient.ingredientType
            );

            ResetStation();
            return;
        }

        Vector3 position = foodOnStove.transform.position;
        Quaternion rotation = foodOnStove.transform.rotation;

        Destroy(foodOnStove);

        foodOnStove = Instantiate(
            recipe.finishedFoodPrefab,
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

        hasBecomeCookedFood = true;

        foodRenderers =
            foodOnStove.GetComponentsInChildren<Renderer>();

        // Start with the normal, perfectly cooked appearance.
        ApplyCookedness(0f);

        Debug.Log("Food is perfectly cooked!");
    }

    private void ApplyCookedness(float darkness)
    {
        if (foodRenderers == null)
            return;

        darkness = Mathf.Clamp01(darkness);

        // White leaves the original texture and colour unchanged.
        // Black darkens the original appearance.
        Color darkeningTint = Color.Lerp(
            Color.white,
            Color.black,
            darkness
        );

        foreach (Renderer rend in foodRenderers)
        {
            if (rend == null)
                continue;

            MaterialPropertyBlock block =
                new MaterialPropertyBlock();

            rend.GetPropertyBlock(block);

            block.SetColor("_Color", darkeningTint);
            block.SetColor("_BaseColor", darkeningTint);

            rend.SetPropertyBlock(block);
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
                cookingText.text = "PERFECT! Press Prep!";
            else
                cookingText.text = "BURNT!";
        }
    }

    public GameObject TryTakeFood()
    {
        if (!playerNearby || isCooking || foodOnStove == null)
            return null;

        GameObject result = foodOnStove;

        foodOnStove = null;
        foodRenderers = null;
        hasBecomeCookedFood = false;
        cookingTimer = 0f;

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
        foodRenderers = null;
        hasBecomeCookedFood = false;

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
               !isCooking &&
               hasBecomeCookedFood;
    }
}
