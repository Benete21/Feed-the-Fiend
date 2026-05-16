using UnityEngine;
using UnityEngine.InputSystem;

public class Waiter_Controls : MonoBehaviour
{
    [Header("Movment")]
    [SerializeField] private float moveSpeed = 5f;
    [SerializeField] private float rotateSpeed = 180f;
    private Vector2 moveInput;

    [Header("Pickup")]
    [SerializeField] Transform hold;
    private GameObject heldObj;
    private Rigidbody heldrb;

    [SerializeField] private float pickupRange = 3f;


    private void Update()
    {
        Vector3 move = new Vector3(moveInput.x, 0, moveInput.y);

        transform.position += move * moveSpeed * Time.deltaTime;

        if (move != Vector3.zero)
        {
            Quaternion targetRotation = Quaternion.LookRotation(move);
            transform.rotation = Quaternion.Slerp(transform.rotation, targetRotation, rotateSpeed * Time.deltaTime);
        }
    }

    public void OnMovement(InputAction.CallbackContext context)
    {
        moveInput = context.ReadValue<Vector2>();
        print("Move Wokr");
    }
    public void OnPickup(InputAction.CallbackContext context)
    {
        print("P Wokr");
        if (!context.performed) return;
        if (heldObj == null)
        {
            RaycastHit hit;
            if (Physics.Raycast(transform.position, transform.TransformDirection(Vector3.forward), out hit, pickupRange))
            {
                PickUp(hit.transform.gameObject);
            }
        }
        else
        {
            DropObj();
        }
    }

    public void PickUp(GameObject pick)
    {
        if (pick.TryGetComponent(out Rigidbody rb))
        {
            heldrb = rb;
            heldObj = pick;

            heldrb.useGravity = false;
            heldrb.linearDamping = 10f;
            heldrb.constraints = RigidbodyConstraints.FreezeRotation;

            heldObj.transform.SetParent(hold);

            heldObj.transform.localPosition = Vector3.zero;
            heldObj.transform.localRotation = Quaternion.identity;
        }
    }
    public void DropObj()
    {
        heldrb.useGravity = true;
        heldrb.linearDamping = 1f;
        heldrb.constraints = RigidbodyConstraints.None;

        heldObj.transform.SetParent(null);

        heldObj = null;
        heldrb = null;
    }
}
