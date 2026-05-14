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
    [SerializeField] private float pickupRange = 3f;

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

}
