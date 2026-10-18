using UnityEngine;
using UnityEngine.InputSystem;

public class Chef_Controls : MonoBehaviour
{
    [Header("Movment")]
    [SerializeField] private float moveSpeed = 5f;
    [SerializeField] private float rotateSpeed = 180f;
    private Vector2 moveInput;

    [Header("Pickup")]
    [SerializeField] private float pickupRange = 1.2f;
    [SerializeField] private float pickupWidth = 0.7f;
    [SerializeField] private float pickupHeight = 1.2f;

    [SerializeField] Transform hold;
    private GameObject heldObj;
    private Rigidbody heldRb;

    [Header("Prep Station")]
    [SerializeField] private PrepFoodStation prepStation;
    [SerializeField] private float prepStationRange = 2f;

    [Header("Cooking Station")]
    [SerializeField] private Cooking_Station cookingStation;
    [SerializeField] private float cookingStationRange = 2f;

    [Header("Ingredient Station")]
    private System.Collections.Generic.List<Ingredient_Spawner> nearbySpawners =
    new System.Collections.Generic.List<Ingredient_Spawner>();

    [Header("Interaction Prompt")]
    [SerializeField] private InteractionUI interactionPrompt;
    [SerializeField] private float interactionRange = 2.5f;

    [Header("Recipe")]
    public GameObject recipe_Book;
    private PlayerStatus playerStatus;

    [Header("Order Inventory")]
    [SerializeField] private Chef_Order_Inventory orderInventory;

    private void Awake()
    {
        playerStatus = GetComponent<PlayerStatus>();

        if (playerStatus == null)
            playerStatus = GetComponentInParent<PlayerStatus>();
    }
    private void Update()
    {

        CheckForInteraction();
        Vector3 move = new Vector3(moveInput.x, 0, moveInput.y);

        float currentSpeed = moveSpeed;

        if (playerStatus != null)
        {
            currentSpeed *= playerStatus.SlowMultiplier;
        }

        transform.position += move * currentSpeed * Time.deltaTime;

        if (move != Vector3.zero)
        {
            Quaternion targetRotation = Quaternion.LookRotation(move);
            transform.rotation = Quaternion.Slerp(transform.rotation, targetRotation, rotateSpeed * Time.deltaTime);
        }
        if (heldObj != null)
        {
            heldObj.transform.position = hold.position;
            heldObj.transform.rotation = hold.rotation;
        }
    }

    private void OnTriggerEnter(Collider other)
    {
        Ingredient_Spawner spawner =
            other.GetComponent<Ingredient_Spawner>();

        if (spawner != null && !nearbySpawners.Contains(spawner))
        {
            nearbySpawners.Add(spawner);
        }
    }

    private void OnTriggerExit(Collider other)
    {
        Ingredient_Spawner spawner =
            other.GetComponent<Ingredient_Spawner>();

        if (spawner != null)
        {
            nearbySpawners.Remove(spawner);
        }
    }

    public void OnMovement(InputAction.CallbackContext context)
    {
        moveInput = context.ReadValue<Vector2>();
        print("Move Wokr");
    }

    public void OnPickup(InputAction.CallbackContext context)
    {
        if (!context.performed)
            return;

        // If holding an item, try placing it on a nearby station.
        if (heldObj != null)
        {
            if (cookingStation != null &&
                Vector3.Distance(transform.position,
                    cookingStation.transform.position) <= cookingStationRange)
            {
                if (cookingStation.TryPlaceIngredient(heldObj))
                {
                    ReleaseHeldObjectForStation();
                    return;
                }
            }

            // Otherwise, try the existing food preparation station.
            if (prepStation != null &&
                Vector3.Distance(transform.position,
                    prepStation.transform.position) <= prepStationRange)
            {
                GameObject ingredientToPlace = heldObj;

                heldObj = null;
                heldRb = null;

                prepStation.AddIngredient(ingredientToPlace);
                return;
            }

            Drop();
            return;
        }

        // If not holding anything, collect finished food from the stove.
        if (cookingStation != null &&
            Vector3.Distance(transform.position,
                cookingStation.transform.position) <= cookingStationRange)
        {
            GameObject cookedFood = cookingStation.TryTakeFood();

            if (cookedFood != null)
            {
                Pickup(cookedFood);
                return;
            }
        }

        // Existing ingredient-spawner pickup.
        Ingredient_Spawner spawner = GetClosestIngredientSpawner();

        if (spawner != null)
        {
            GameObject ingredient = spawner.TakeIngredient();

            if (ingredient != null)
            {
                Pickup(ingredient);
                return;
            }
        }

        // Existing world-item pickup.
        TryPickup();
    }

    private void ReleaseHeldObjectForStation()
    {
        if (heldRb != null)
        {
            Collider itemCollider = heldRb.GetComponent<Collider>();
            Collider chefCollider = GetComponent<Collider>();

            if (itemCollider != null && chefCollider != null)
                Physics.IgnoreCollision(itemCollider, chefCollider, false);
        }

        heldObj = null;
        heldRb = null;
    }

    public void OnPrep(InputAction.CallbackContext context)
    {
        if (!context.performed)
            return;

        if (cookingStation != null &&
            Vector3.Distance(transform.position,
                cookingStation.transform.position) <= cookingStationRange)
        {
            cookingStation.HandlePrepButton();
            return;
        }

        if (prepStation != null &&
            Vector3.Distance(transform.position,
                prepStation.transform.position) <= prepStationRange)
        {
            prepStation.StartPreparationPress();
        }
    }

    public void OnRecipe(InputAction.CallbackContext context)
    {

        if (recipe_Book != null)
        {
            recipe_Book.SetActive(!recipe_Book.activeSelf);
        }
    }

    public void OnInventory(InputAction.CallbackContext context)
    {
        if (!context.performed || orderInventory == null)
            return;

        orderInventory.ToggleInventory();
    }

    public void OnOrderBoard(InputAction.CallbackContext context)
    {
        if (!context.performed || orderInventory == null)
            return;

        Collider[] hits = Physics.OverlapSphere(
            transform.position,
            2f
        );

        foreach (Collider hit in hits)
        {
            Chef_order_Board board =
                hit.GetComponentInParent<Chef_order_Board>();

            if (board != null)
            {
                board.OpenBoard(orderInventory);
                return;
            }
        }
    }

    void TryPickup()
    {
        // Position the pickup area in front of the Chef
        Vector3 center =
            transform.position +
            transform.forward * (pickupRange * 0.5f) +
            Vector3.up * 0.5f;

        // Half extents of the pickup box
        Vector3 halfExtents = new Vector3(
            pickupWidth * 0.5f,
            pickupHeight * 0.5f,
            pickupRange * 0.5f
        );

        // Show pickup area in Scene view
        Collider[] hits = Physics.OverlapBox(
            center,
            halfExtents,
            transform.rotation
        );

        foreach (Collider hit in hits)
        {
            if (!hit.CompareTag("Item"))
                continue;

            Rigidbody rb = hit.attachedRigidbody;

            if (rb != null)
            {
                Pickup(rb.gameObject);
                return;
            }
        }
    }

    void TryPrepare()
    {
        if (prepStation != null)
        {
            prepStation.StartPreparationPress();
        }
    }

    void Pickup(GameObject obj)
    {
        heldObj = obj;
        heldRb = obj.GetComponent<Rigidbody>();

        heldRb.useGravity = false;
        heldRb.linearDamping = 10f;
        heldRb.constraints = RigidbodyConstraints.FreezeRotation;

        heldRb.transform.SetParent(hold);
        heldRb.transform.localPosition = Vector3.zero;
        heldRb.transform.localRotation = Quaternion.identity;
        Physics.IgnoreCollision(heldRb.GetComponent<Collider>(), GetComponent<Collider>(), true);
    }

    void Drop()
    {
        if (heldRb == null)
            return;

        Collider heldCollider = heldRb.GetComponent<Collider>();
        Collider playerCollider = GetComponent<Collider>();

        if (heldCollider != null && playerCollider != null)
        {
            Physics.IgnoreCollision(heldCollider, playerCollider, false);
        }

        heldRb.useGravity = true;
        heldRb.linearDamping = 1f;
        heldRb.constraints = RigidbodyConstraints.None;

        heldRb.transform.SetParent(null);

        heldObj = null;
        heldRb = null;
    }


    private void CheckForInteraction()
    {
        if (interactionPrompt == null)
            return;

        // If holding an item, hide the prompt.
        if (heldObj != null)
        {
            interactionPrompt.Hide();
            return;
        }

        // Cooking station
        if (cookingStation != null &&
            Vector3.Distance(transform.position,
                cookingStation.transform.position) <= cookingStationRange)
        {
            if (cookingStation.IsReadyToCollect())
            {
                interactionPrompt.Show("A  PICK UP");
                return;
            }

            if (cookingStation.IsCooking)
            {
                interactionPrompt.Hide();
                return;
            }

            if (cookingStation.IsReadyToCook())
            {
                interactionPrompt.Show("B  COOK");
                return;
            }
        }

        // Prep station.
        if (prepStation != null &&
            Vector3.Distance(transform.position,
                prepStation.transform.position) <= prepStationRange)
        {
            if (prepStation.HasIngredients())
            {
                interactionPrompt.Show("B  PREPARE");
                return;
            }
        }

        // Ingredient spawner.
        if (nearbySpawners.Count > 0)
        {
            interactionPrompt.Show("A  PICK UP");
            return;
        }

        // Check ordinary world items and customers.
        Vector3 center =
            transform.position +
            transform.forward * (pickupRange * 0.5f) +
            Vector3.up * 0.5f;

        Vector3 halfExtents = new Vector3(
            pickupWidth * 0.5f,
            pickupHeight * 0.5f,
            pickupRange * 0.5f
        );

        Collider[] hits = Physics.OverlapBox(
            center,
            halfExtents,
            transform.rotation
        );

        foreach (Collider hit in hits)
        {
            if (hit.CompareTag("Item"))
            {
                interactionPrompt.Show("A  PICK UP");
                return;
            }

            if (hit.CompareTag("Customer"))
            {
                interactionPrompt.Show("A  INTERACT");
                return;
            }
        }

        interactionPrompt.Hide();
    }



    private Ingredient_Spawner GetClosestIngredientSpawner()
    {
        Ingredient_Spawner closest = null;
        float closestDistance = Mathf.Infinity;

        foreach (Ingredient_Spawner spawner in nearbySpawners)
        {
            if (spawner == null)
                continue;

            float distance = Vector3.Distance(
                transform.position,
                spawner.transform.position
            );

            if (distance < closestDistance)
            {
                closestDistance = distance;
                closest = spawner;
            }
        }

        return closest;
    }
    public GameObject GetHeldObject()
    {
        return heldObj;
    }


    public bool ReceiveOrderSlip(PhysicalOrderSlip slip)
    {
        if (slip == null)
        {
            Debug.LogWarning("Cannot receive a null order slip.");
            return false;
        }

        if (orderInventory == null)
        {
            Debug.LogError(
                "Chef_Order_Inventory is not assigned in the Inspector!"
            );
            return false;
        }

        if (!orderInventory.HasSpace())
        {
            Debug.LogWarning("Chef's inventory is full!");
            return false;
        }

        Food_Types[] order = slip.GetOrder();

        if (order == null || order.Length == 0)
        {
            Debug.LogWarning("This order slip contains no food order.");
            return false;
        }

        bool received = orderInventory.ReceiveOrder(
            order,
            slip.TableNumber
        );

        if (received)
        {
            Debug.Log(
                "Chef received order for Table " + slip.TableNumber
            );
        }

        return received;
    }


}
