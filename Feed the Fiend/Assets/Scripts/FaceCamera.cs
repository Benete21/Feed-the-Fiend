using UnityEngine;

public class FaceCamera : MonoBehaviour
{
    private Camera mainCamera;

    private void Start()
    {
        mainCamera = Camera.main;
    }

    private void LateUpdate()
    {
        if (mainCamera == null)
        {
            mainCamera = Camera.main;
            return;
        }

        // Face the camera
        transform.LookAt(mainCamera.transform);

        // Keep the UI from appearing backwards
        transform.Rotate(0f, 180f, 0f);
    }
}

