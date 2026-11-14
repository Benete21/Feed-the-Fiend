
using UnityEngine;
using UnityEngine.InputSystem;
using System.Collections;
using System.Collections.Generic;

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
    private Collider[] playerColliders;

    [Header("Prep Station")]
    [SerializeField] private PrepFoodStation prepStation;
    [SerializeField] private float prepStationRange = 2f;

    [Header("Cooking Station")]
    [SerializeField] private Cooking_Station cookingStation;
    [SerializeField] private float cookingStationRange = 2f;

    [Header("Ingredient Station")]
    private List<Ingredient_Spawner> nearbySpawners =
        new List<Ingredient_Spawner>();

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

        playerColliders = GetComponentsInChildren<Collider>();
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

            transform.rotation = Quaternion.Slerp(
                transform.rotation,
                targetRotation,
                rotateSpeed * Time.deltaTime
            );
        }

        if (heldObj != null && hold != null)
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
                Vector3.Distance(
                    transform.position,
                    cookingStation.transform.position
                ) <= cookingStationRange)
            {
                if (cookingStation.TryPlaceIngredient(heldObj))
                {
                    ReleaseHeldObjectForStation();
                    return;
                }
            }

            // Try placing the ingredient on the prep station.
            if (prepStation != null &&
                Vector3.Distance(
                    transform.position,
                    prepStation.transform.position
                ) <= prepStationRange)
            {
                GameObject ingredientToPlace = heldObj;

                prepStation.AddIngredient(ingredientToPlace);

                RestoreCollisionAfterRelease(ingredientToPlace);

                heldObj = null;
                heldRb = null;
                return;
            }

            Drop();
            return;
        }

        // NEW: Try taking an ingredient out of the prep station first.
        if (TryTakeIngredientFromPrepStation())
            return;

        // Collect finished food from the stove.
        if (cookingStation != null &&
            Vector3.Distance(
                transform.position,
                cookingStation.transform.position
            ) <= cookingStationRange)
        {
            GameObject cookedFood = cookingStation.TryTakeFood();

            if (cookedFood != null)
            {
                Pickup(cookedFood);
                return;
            }
        }

        // Ingredient-spawner pickup.
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

        // World-item pickup.
        TryPickup();
    }

    private bool TryTakeIngredientFromPrepStation()
    {
        if (prepStation == null)
            return false;

        // Make sure the chef is close enough.
        if (Vector3.Distance(
            transform.position,
            prepStation.transform.position
        ) > prepStationRange)
            return false;

        // Do not remove ingredients during preparation.
        if (prepStation.IsPreparing())
            return false;

        Collider[] hits = Physics.OverlapSphere(
            prepStation.transform.position,
            1.5f
        );

        foreach (Collider hit in hits)
        {
            Ingredient_Item ingredient =
                hit.GetComponentInParent<Ingredient_Item>();

            if (ingredient == null)
                continue;

            GameObject ingredientObject = ingredient.gameObject;

            // Only allow actual ingredient objects.
            if (!ingredientObject.CompareTag("Ingredient"))
                continue;

            Rigidbody ingredientRb =
                ingredientObject.GetComponent<Rigidbody>();

            if (ingredientRb == null)
            {
                Debug.LogWarning(
                    "Ingredient needs a Rigidbody to be picked up!"
                );
                continue;
            }

            // Remove the ingredient from the station.
            prepStation.RemoveIngredient(ingredientObject);

            // Pick it up.
            Pickup(ingredientObject);

            Debug.Log("Ingredient removed from prep station.");

            return true;
        }

        return false;
    }

    private void ReleaseHeldObjectForStation()
    {
        GameObject releasedObject = heldObj;

        heldObj = null;
        heldRb = null;

        RestoreCollisionAfterRelease(releasedObject);
    }

    public void OnPrep(InputAction.CallbackContext context)
    {
        if (!context.performed)
            return;

        if (cookingStation != null &&
            Vector3.Distance(
                transform.position,
                cookingStation.transform.position
            ) <= cookingStationRange)
        {
            cookingStation.HandlePrepButton();
            return;
        }

        if (prepStation != null &&
            Vector3.Distance(
                transform.position,
                prepStation.transform.position
            ) <= prepStationRange)
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
        if (!context.performed)
            return;

        Collider[] hits = Physics.OverlapSphere(
            transform.position,
            interactionRange
        );

        foreach (Collider hit in hits)
        {
            OrderSlipDropOff dropoff =
                hit.GetComponentInParent<OrderSlipDropOff>();

            if (dropoff == null)
                continue;

            if (orderInventory == null)
            {
                Debug.LogError(
                    "Chef_Order_Inventory is not assigned in the Inspector!"
                );
                return;
            }

            orderInventory.ToggleInventory();
            return;
        }

        Debug.Log("Move closer to the order counter to view orders.");
    }

    public void OnPause(InputAction.CallbackContext context)
    {
        if (!context.performed)
            return;

        PlayerInput playerInput = GetComponent<PlayerInput>();

        if (playerInput == null)
            playerInput = GetComponentInParent<PlayerInput>();

        if (playerInput == null)
        {
            Debug.LogError("PlayerInput component not found!");
            return;
        }

        Pause_Menu.Instance.TogglePause(playerInput);
    }

    void TryPickup()
    {
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

    private void SetItemPlayerCollision(GameObject item, bool ignore)
    {
        if (item == null || playerColliders == null)
            return;

        Collider[] itemColliders =
            item.GetComponentsInChildren<Collider>();

        foreach (Collider itemCol in itemColliders)
        {
            if (itemCol == null)
                continue;

            foreach (Collider playerCol in playerColliders)
            {
                if (playerCol != null)
                    Physics.IgnoreCollision(itemCol, playerCol, ignore);
            }
        }
    }

    private void RestoreCollisionAfterRelease(GameObject item)
    {
        if (item == null)
            return;

        Collider[] itemColliders =
            item.GetComponentsInChildren<Collider>();

        StartCoroutine(
            RestorePlayerCollisionWhenSeparated(itemColliders)
        );
    }

    private IEnumerator RestorePlayerCollisionWhenSeparated(
        Collider[] itemColliders)
    {
        while (true)
        {
            yield return new WaitForFixedUpdate();
            Physics.SyncTransforms();

            bool stillOverlapping = false;

            foreach (Collider itemCol in itemColliders)
            {
                if (itemCol == null ||
                    !itemCol.enabled ||
                    !itemCol.gameObject.activeInHierarchy)
                    continue;

                foreach (Collider playerCol in playerColliders)
                {
                    if (playerCol == null ||
                        !playerCol.enabled ||
                        !playerCol.gameObject.activeInHierarchy)
                        continue;

                    if (Physics.ComputePenetration(
                        itemCol,
                        itemCol.transform.position,
                        itemCol.transform.rotation,
                        playerCol,
                        playerCol.transform.position,
                        playerCol.transform.rotation,
                        out _,
                        out _))
                    {
                        stillOverlapping = true;
                        break;
                    }
                }

                if (stillOverlapping)
                    break;
            }

            if (!stillOverlapping)
                break;
        }

        foreach (Collider itemCol in itemColliders)
        {
            if (itemCol == null)
                continue;

            foreach (Collider playerCol in playerColliders)
            {
                if (playerCol != null)
                    Physics.IgnoreCollision(itemCol, playerCol, false);
            }
        }
    }

    void Pickup(GameObject obj)
    {
        if (obj == null)
            return;

        Rigidbody rb = obj.GetComponent<Rigidbody>();

        if (rb == null)
        {
            Debug.LogWarning(
                "Cannot pick up " + obj.name +
                " because it has no Rigidbody."
            );
            return;
        }

        if (hold == null)
        {
            Debug.LogError("Hold transform is not assigned!");
            return;
        }

        heldObj = obj;
        heldRb = rb;

        heldRb.useGravity = false;
        heldRb.linearDamping = 10f;
        heldRb.constraints = RigidbodyConstraints.FreezeRotation;

        heldRb.transform.SetParent(hold);
        heldRb.transform.localPosition = Vector3.zero;
        heldRb.transform.localRotation = Quaternion.identity;

        SetItemPlayerCollision(obj, true);
    }

    void Drop()
    {
        if (heldRb == null)
            return;

        GameObject droppedObject = heldObj;

        heldRb.useGravity = true;
        heldRb.linearDamping = 1f;
        heldRb.constraints = RigidbodyConstraints.None;
        heldRb.transform.SetParent(null);

        heldObj = null;
        heldRb = null;

        RestoreCollisionAfterRelease(droppedObject);
    }

    private void CheckForInteraction()
    {
        if (interactionPrompt == null)
            return;

        if (heldObj != null)
        {
            interactionPrompt.Hide();
            return;
        }

        Collider[] boardHits = Physics.OverlapSphere(
            transform.position,
            interactionRange
        );

        foreach (Collider hit in boardHits)
        {
            OrderSlipDropOff dropoff =
                hit.GetComponentInParent<OrderSlipDropOff>();

            if (dropoff != null)
            {
                interactionPrompt.Show("Y  VIEW ORDERS");
                return;
            }
        }

        // Cooking station.
        if (cookingStation != null &&
            Vector3.Distance(
                transform.position,
                cookingStation.transform.position
            ) <= cookingStationRange)
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
            Vector3.Distance(
                transform.position,
                prepStation.transform.position
            ) <= prepStationRange)
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
            interactionPrompt.Show("B  PICK UP");
            return;
        }

        // World items and customers.
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
                interactionPrompt.Show("B  PICK UP");
                return;
            }

            if (hit.CompareTag("Customer"))
            {
                interactionPrompt.Show("B  INTERACT");
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
