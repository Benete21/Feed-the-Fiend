using UnityEngine;
using UnityEngine.InputSystem;
using System.Collections;

public class Waiter_Controls : MonoBehaviour
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

    [Header("Order Slip")]
    [SerializeField] private Transform slipSpawnPoint;
    [SerializeField] private GameObject physicalOrderSlipPrefab;

    [Header("Order Slip UI")]
    [SerializeField] private Transform slipUIHolder;
    [SerializeField] private Order_Slip orderSlipUIPrefab;

    [Header("Interaction Prompt")]
    [SerializeField] private InteractionUI interactionPrompt;
    [SerializeField] private float interactionRange = 2.5f;


    private Order_Slip currentSlip;
    private PlayerStatus playerStatus;

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
            transform.rotation = Quaternion.Slerp(transform.rotation, targetRotation, rotateSpeed * Time.deltaTime);
        }
        if (heldObj != null)
        {
            heldObj.transform.position = hold.position;
            heldObj.transform.rotation = hold.rotation;
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

        if (heldObj != null)
        {
            PhysicalOrderSlip slip =
                heldObj.GetComponent<PhysicalOrderSlip>();

            if (slip != null)
            {
                TryDropSlipAtCounter(slip);
                return;
            }

            Drop();
            return;
        }


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
            // Check for customer interaction first
            IInteractable interactable =
                hit.GetComponentInParent<IInteractable>();

            if (interactable != null)
            {
                interactable.Interact(this);
                return;
            }

            // Check for physical objects
            if (hit.CompareTag("Item"))
            {
                Rigidbody rb = hit.attachedRigidbody;

                if (rb != null)
                {
                    Pickup(rb.gameObject);
                    return;
                }
            }
        }
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


    private void SetItemPlayerCollision(GameObject item, bool ignore)
    {
        if (item == null || playerColliders == null)
            return;

        Collider[] itemColliders = item.GetComponentsInChildren<Collider>();

        foreach (Collider itemCol in itemColliders)
        {
            if (itemCol == null) continue;

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

        Collider[] itemColliders = item.GetComponentsInChildren<Collider>();
        StartCoroutine(RestorePlayerCollisionWhenSeparated(itemColliders));
    }

    private IEnumerator RestorePlayerCollisionWhenSeparated(Collider[] itemColliders)
    {
        // Keep collisions ignored until the object is no longer intersecting the player.
        while (true)
        {
            yield return new WaitForFixedUpdate();
            Physics.SyncTransforms();

            bool stillOverlapping = false;

            foreach (Collider itemCol in itemColliders)
            {
                if (itemCol == null || !itemCol.enabled || !itemCol.gameObject.activeInHierarchy)
                    continue;

                foreach (Collider playerCol in playerColliders)
                {
                    if (playerCol == null || !playerCol.enabled || !playerCol.gameObject.activeInHierarchy)
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
            if (itemCol == null) continue;

            foreach (Collider playerCol in playerColliders)
            {
                if (playerCol != null)
                    Physics.IgnoreCollision(itemCol, playerCol, false);
            }
        }
    }

    void Pickup(GameObject obj)
    {
        heldObj = obj;
        heldRb = obj.GetComponent<Rigidbody>();

        if (heldRb == null)
        {
            heldObj = null;
            return;
        }

        heldRb.useGravity = false;
        heldRb.linearDamping = 10f;
        heldRb.constraints = RigidbodyConstraints.FreezeRotation;

        heldRb.transform.SetParent(hold);
        heldRb.transform.localPosition = Vector3.zero;
        heldRb.transform.localRotation = Quaternion.identity;

        SetItemPlayerCollision(obj, true);

        // Check if the object is an order slip
        PhysicalOrderSlip physicalSlip =
            obj.GetComponent<PhysicalOrderSlip>();

        if (physicalSlip != null)
        {
            ShowOrderSlip(physicalSlip.GetOrder());
        }
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

        PhysicalOrderSlip orderSlip = droppedObject.GetComponent<PhysicalOrderSlip>();
        if (orderSlip != null)
            RemoveOrderSlip();

        heldObj = null;
        heldRb = null;

        RestoreCollisionAfterRelease(droppedObject);
    }

    public GameObject GetHeldObject()
    {
        return heldObj;
    }

    public void RemoveHeldObject()
    {
        if (heldObj != null)
        {
            // Destroy the actual object being held
            Destroy(heldObj);
        }

        heldObj = null;
        heldRb = null;
    }

    public void GiveOrderSlip(
        Food_Types[] order,
        int tableNumber)
    {
        GameObject slip = Instantiate(
            physicalOrderSlipPrefab,
            slipSpawnPoint.position,
            slipSpawnPoint.rotation
        );

        PhysicalOrderSlip physicalSlip =
            slip.GetComponent<PhysicalOrderSlip>();

        if (physicalSlip != null)
        {
            physicalSlip.SetOrderDetails(
                order,
                tableNumber
            );
        }
    }


    public void GiveOrderDirectlyToHand(
        Food_Types[] order,
        int tableNumber)
    {
        if (order == null || order.Length == 0)
            return;

        if (heldObj != null)
        {
            Debug.LogWarning(
                "Waiter is already holding an object."
            );
            return;
        }

        GameObject slip = Instantiate(
            physicalOrderSlipPrefab,
            hold.position,
            hold.rotation
        );

        PhysicalOrderSlip physicalSlip =
            slip.GetComponent<PhysicalOrderSlip>();

        if (physicalSlip != null)
        {
            physicalSlip.SetOrderDetails(
                order,
                tableNumber
            );
        }

        heldObj = slip;
        heldRb = slip.GetComponent<Rigidbody>();

        if (heldRb != null)
        {
            heldRb.useGravity = false;
            heldRb.linearDamping = 10f;
            heldRb.constraints =
                RigidbodyConstraints.FreezeRotation;

            heldRb.transform.SetParent(hold);
            heldRb.transform.localPosition = Vector3.zero;
            heldRb.transform.localRotation = Quaternion.identity;
        }
        else
        {
            slip.transform.SetParent(hold);
            slip.transform.localPosition = Vector3.zero;
            slip.transform.localRotation = Quaternion.identity;
        }

        SetItemPlayerCollision(slip, true);

        ShowOrderSlip(order);

        Debug.Log(
            "Order slip received for table " + tableNumber
        );
    }



    private void ShowOrderSlip(Food_Types[] order)
    {
        if (currentSlip != null)
        {
            Destroy(currentSlip.gameObject);
        }

        currentSlip = Instantiate(orderSlipUIPrefab, slipUIHolder);

        currentSlip.SetOrder(order);
    }


    public void RemoveOrderSlip()
    {
        if (currentSlip != null)
        {
            Destroy(currentSlip.gameObject);
            currentSlip = null;
        }
    }



    private void CheckForInteraction()
    {
        if (interactionPrompt == null)
            return;

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

        // Check whether the waiter is holding an order slip.
        if (heldObj != null)
        {
            PhysicalOrderSlip heldSlip =
                heldObj.GetComponent<PhysicalOrderSlip>();

            if (heldSlip != null)
            {
                // Only show the prompt when near an order drop-off counter.
                foreach (Collider hit in hits)
                {
                    OrderSlipDropOff dropoff =
                        hit.GetComponentInParent<OrderSlipDropOff>();

                    if (dropoff != null)
                    {
                        interactionPrompt.Show("B  DROP OFF SLIP");
                        return;
                    }
                }

                interactionPrompt.Hide();
                return;
            }
        }

        foreach (Collider hit in hits)
        {
            CustomerOrder customer =
                hit.GetComponentInParent<CustomerOrder>();

            if (customer != null)
            {
                if (!customer.HasOrdered() && customer.waiting)
                {
                    interactionPrompt.Show("B  INTERACT");
                    return;
                }

                if (customer.HasOrdered() && customer.waiting)
                {
                    if (heldObj != null &&
                        heldObj.GetComponentInChildren<FoodItems>() != null)
                    {
                        interactionPrompt.Show("B  GIVE ORDER");
                        return;
                    }

                    continue;
                }

                continue;
            }

            // Ordinary items can only be picked up with empty hands.
            if (heldObj == null && hit.CompareTag("Item"))
            {
                interactionPrompt.Show("B  PICK UP");
                return;
            }

            IInteractable interactable =
                hit.GetComponentInParent<IInteractable>();

            if (interactable != null)
            {
                interactionPrompt.Show("B  INTERACT");
                return;
            }
        }

        interactionPrompt.Hide();
    }



    private bool TryDropSlipAtCounter(PhysicalOrderSlip slip)
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
            OrderSlipDropOff dropoff =
                hit.GetComponentInParent<OrderSlipDropOff>();

            if (dropoff == null)
                continue;

            if (!dropoff.DepositSlip(slip))
                return false;

            // The slip now belongs to the drop-off area.
            GameObject depositedSlip = heldObj;
            heldObj = null;
            heldRb = null;

            RestoreCollisionAfterRelease(depositedSlip);
            RemoveOrderSlip();

            Debug.Log("Waiter dropped the slip at the counter.");
            return true;
        }

        Debug.Log("Move closer to the order drop-off counter.");
        return false;
    }

}