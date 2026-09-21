using UnityEngine;
using UnityEngine.InputSystem;

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

        Collider heldCollider = heldRb.GetComponent<Collider>();
        Collider playerCollider = GetComponent<Collider>();

        if (heldCollider != null && playerCollider != null)
        {
            Physics.IgnoreCollision(
                heldCollider,
                playerCollider,
                true
            );
        }

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
        Collider heldCollider = heldRb.GetComponent<Collider>();
        Collider playerCollider = GetComponent<Collider>();

        if (heldCollider != null && playerCollider != null)
        {
            Physics.IgnoreCollision(
                heldCollider,
                playerCollider,
                false
            );
        }
        heldRb.useGravity = true;
        heldRb.linearDamping = 1f;
        heldRb.constraints = RigidbodyConstraints.None;

        heldRb.transform.SetParent(null);

        // Check if the object being dropped is an order slip
        PhysicalOrderSlip orderSlip = heldObj.GetComponent<PhysicalOrderSlip>();

        if (orderSlip != null)
        {
            RemoveOrderSlip();
        }

        heldObj = null;
        heldRb = null;
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



    public void GiveOrderSlip(Food_Types[] order)
    {
        GameObject slip = Instantiate(physicalOrderSlipPrefab, slipSpawnPoint.position, slipSpawnPoint.rotation);

        PhysicalOrderSlip physicalSlip = slip.GetComponent<PhysicalOrderSlip>();

        if (physicalSlip != null)
        {
            physicalSlip.SetOrder(order);
        }
    }
    private void ShowOrderSlip(Food_Types[] order)
    {
        if (currentSlip != null)
        {
            Destroy(currentSlip.gameObject);
        }

        currentSlip = Instantiate(orderSlipUIPrefab,slipUIHolder);

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
        Vector3 origin = transform.position + Vector3.up * 0.5f;

        if (Physics.Raycast(
            origin,
            transform.forward,
            out RaycastHit hit,
            interactionRange))
        {
            if (hit.collider.CompareTag("Item"))
            {
                interactionPrompt.Show("A  PICK UP");
                return;
            }

            if (hit.collider.CompareTag("Customer"))
            {
                interactionPrompt.Show("A  INTERACT");
                return;
            }
        }

        interactionPrompt.Hide();
    }

}
