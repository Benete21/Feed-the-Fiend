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
    [SerializeField] private float pickupForce = 150.0f;


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

            Vector3 rayStart = transform.position + Vector3.up;
            Debug.DrawRay(rayStart, transform.forward * pickupRange, Color.red);

            if (Physics.Raycast(transform.position, transform.TransformDirection(Vector3.forward), out hit, pickupRange))
            {
                PickUp(hit.transform.gameObject);
            }
        }
        else
        {
            DropObj();
        }
        if (heldObj != null)
        {
            MoveObj();
        }
    }

    public void PickUp(GameObject pick)
    {
        if (pick.GetComponent<Rigidbody>())
        {
            heldrb = pick.GetComponent<Rigidbody>();
            heldrb.useGravity = false;
            heldrb.linearDamping = 10;
            heldrb.constraints = RigidbodyConstraints.FreezeRotation;

            heldrb.transform.parent = hold;
            heldObj = pick;

        }
    }
    public void DropObj()
    {
        heldrb.useGravity = true;
        heldrb.linearDamping = 1;
        heldrb.constraints = RigidbodyConstraints.FreezeRotation;

        heldrb.transform.parent = null;
        heldObj = null;
    }
    public void MoveObj()
    {
        if (Vector3.Distance(heldObj.transform.position, hold.position) > 0.1f)
        {
            Vector3 moveDir = (hold.position - heldObj.transform.position).normalized;
            heldrb.AddForce(moveDir * pickupForce);
        }
    }

}
