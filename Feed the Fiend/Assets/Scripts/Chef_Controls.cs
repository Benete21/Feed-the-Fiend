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
    [SerializeField] private PrepFoodStation prepStation;

    [Header("Prep Station")]
    [SerializeField] private float prepStationRange = 2f;

    [Header("Ingredient Station")]
    private System.Collections.Generic.List<Ingredient_Spawner> nearbySpawners =
    new System.Collections.Generic.List<Ingredient_Spawner>();



    [Header("Interaction Prompt")]
    [SerializeField] private InteractionUI interactionPrompt;
    [SerializeField] private float interactionRange = 2.5f;

    [Header("Recipe")]
    public GameObject recipe_Book;

    private void Update()
    {

        CheckForInteraction();
        Vector3 move = new Vector3(moveInput.x, 0, moveInput.y);

        transform.position += move * moveSpeed * Time.deltaTime;

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

        // Already holding something
        if (heldObj != null)
        {
            if (prepStation != null)
            {
                float distance = Vector3.Distance(
                    transform.position,
                    prepStation.transform.position
                );

                if (distance <= prepStationRange)
                {
                    GameObject ingredientToPlace = heldObj;

                    heldObj = null;
                    heldRb = null;

                    prepStation.AddIngredient(ingredientToPlace);
                    return;
                }
            }

            Drop();
            return;
        }

        // Check for an ingredient station
        Ingredient_Spawner spawner =
            GetClosestIngredientSpawner();

        if (spawner != null)
        {
            GameObject ingredient =
                spawner.TakeIngredient();

            if (ingredient != null)
            {
                Pickup(ingredient);
                return;
            }
        }

        // Otherwise pick up an existing item
        TryPickup();
    }



    public void OnPrep(InputAction.CallbackContext context)
    {
        if (!context.performed) return;

        print("Preperaew");
        TryPrepare();         
    }

    public void OnRecipe(InputAction.CallbackContext context)
    {

        if (recipe_Book != null)
        {
            recipe_Book.SetActive(!recipe_Book.activeSelf);
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
        if (heldObj != null)
        {
            interactionPrompt.Hide();
            return;
        }

        // Check if we are inside an ingredient station
        if (nearbySpawners.Count > 0)
        {
            interactionPrompt.Show("A  PICK UP");
            return;
        }

        // Check normal items
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

}
