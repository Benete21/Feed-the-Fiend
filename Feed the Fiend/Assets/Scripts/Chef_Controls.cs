using UnityEngine;
using UnityEngine.InputSystem;

public class Chef_Controls : MonoBehaviour
{
    [Header("Movment")]
    [SerializeField] private float moveSpeed = 5f;
    [SerializeField] private float jumpStep = 1f;
    [SerializeField] private float rotateSpeed = 180f;
    private Vector2 lookInput;
    private Vector2 moveInput;

    [Header("Pickup")]
    [SerializeField] Transform hold;
    private GameObject heldObj;
    private Rigidbody heldrb;

    [SerializeField] private float pickupRange = 3f;
    [SerializeField] private float pickupForce = 150.0f;

    private void Update()
    {
        float yaw = lookInput.x * rotateSpeed * Time.deltaTime;
        transform.Rotate(0f, yaw, 0f, Space.World);

        Vector3 move3 = new Vector3(moveInput.x, 0f, moveInput.y) * moveSpeed;
        transform.position += move3;

    }

    public void OnMovment(InputAction.CallbackContext context)
    {
        moveInput = context.ReadValue<Vector2>();
    }

    public void OnLook(InputAction.CallbackContext context)
    {
        lookInput = context.ReadValue<Vector2>();
    }

    public void OnPickUp(InputAction.CallbackContext context)
    {
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
            heldrb.drag = 10;
            heldrb.constraints = RigidbodyConstraints.FreezeRotation;

            heldrb.transform.parent = hold;
            heldObj = pick;

        }
    }
    public void DropObj()
    {
        heldrb.useGravity = true;
        heldrb.drag = 1;
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
