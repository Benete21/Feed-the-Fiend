using UnityEngine;
using UnityEngine.InputSystem;

public class Waiter_Controls : MonoBehaviour
{
    [Header("Movment")]
    [SerializeField] private float moveSpeed = 5f;
    [SerializeField] private float rotateSpeed = 180f;
    private Vector2 moveInput;

    [Header("Pickup")]
    public float pickupRange = 2f;
    public float pickupRadius = 1f;
    [SerializeField] Transform hold;
    private GameObject heldObj;
    private Rigidbody heldRb;

    [Header("Order Slip")]
    [SerializeField] private Transform slipHolder;
    [SerializeField] private Order_Slip orderSlipPrefab;

    private Order_Slip currentSlip;

    private void Update()
    {
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
        if (!context.performed) return;

        Vector3 origin = transform.position + Vector3.up * 0.5f;

        if (Physics.SphereCast(origin, pickupRadius, transform.forward,out RaycastHit hit, pickupRange))
        {
            IInteractable interactable = hit.collider.GetComponent<IInteractable>();

            if (interactable != null)
            {
                interactable.Interact(this);
                return;
            }

            if (hit.collider.attachedRigidbody != null)
            {
                Pickup(hit.collider.gameObject);
            }
        }
        else
        {
            if (heldObj != null)
            {
                Drop();
                return;
            }
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
        Physics.IgnoreCollision( heldRb.GetComponent<Collider>(),GetComponent<Collider>(),false);
        heldRb.useGravity = true;
        heldRb.linearDamping = 1f;
        heldRb.constraints = RigidbodyConstraints.None;

        heldRb.transform.SetParent(null);

        heldObj = null;
        heldRb = null;
    }
    public GameObject GetHeldObject()
    {
        return heldObj;
    }

    public void RemoveHeldObject()
    {
        heldObj = null;
        heldRb = null;
    }
    public void GiveOrderSlip(Food_Types[] order)
    {
        if (currentSlip != null)
        {
            Destroy(currentSlip.gameObject);
        }

        currentSlip = Instantiate(orderSlipPrefab, slipHolder);
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
}
