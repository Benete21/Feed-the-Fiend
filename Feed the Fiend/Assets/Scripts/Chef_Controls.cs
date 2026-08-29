using UnityEngine;
using UnityEngine.InputSystem;

public class Chef_Controls : MonoBehaviour
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
     public PrepFoodStation prepStation;


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
        if (!context.performed)
            return;

        if (heldObj == null)
        {
            TryPickup();
        }

        else
        {
            if (prepStation != null)
            {
                GameObject ingredientToPlace = heldObj;
                heldObj = null;
                heldRb = null;
                prepStation.AddIngredient(ingredientToPlace);
            }
        }
    }

    public void OnPrep(InputAction.CallbackContext context)
    {
        if (!context.performed) return;

        print("Preperaew");
        TryPrepare();         
    }


    void TryPickup()
    {
        Vector3 origin = transform.position + Vector3.up * 0.5f;

        Debug.DrawRay(origin, transform.forward * pickupRange, Color.red, 1f);

        if (Physics.SphereCast(origin, pickupRadius, transform.forward, out RaycastHit hit, pickupRange))
        {
            if (hit.collider.attachedRigidbody != null)
            {
                Pickup(hit.collider.gameObject);
            }
        }
    }
    void TryPrepare()
    {
        if (prepStation != null)
        {
            prepStation.StartPreparation();
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
        Physics.IgnoreCollision(heldRb.GetComponent<Collider>(), GetComponent<Collider>(), false);
        heldRb.useGravity = true;
        heldRb.linearDamping = 1f;
        heldRb.constraints = RigidbodyConstraints.None;

        heldRb.transform.SetParent(null);

        heldObj = null;
        heldRb = null;
    }

}
