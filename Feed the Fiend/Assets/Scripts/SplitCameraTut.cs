using UnityEngine;

public class SplitCameraTut : MonoBehaviour
{

    [Header("Cameras")]
    public Camera cam1;
    public Camera cam2;

    [Header("Players")]
    public Transform player1;
    public Transform player2;

    [Header("Camera Follow")]
    public Vector3 cameraOffset = new Vector3(0f, 10f, -10f);
    public float followSpeed = 10f;

    private void Start()
    {
        // Always enable both cameras
        cam1.enabled = true;
        cam2.enabled = true;

        // Permanent split screen
        cam1.rect = new Rect(0f, 0f, 0.5f, 1f);
        cam2.rect = new Rect(0.5f, 0f, 0.5f, 1f);
    }

    private void LateUpdate()
    {
        if (player1 != null)
        {
            Vector3 targetPosition = player1.position + cameraOffset;

            cam1.transform.position = Vector3.Lerp(
                cam1.transform.position,
                targetPosition,
                followSpeed * Time.deltaTime
            );
        }

        if (player2 != null)
        {
            Vector3 targetPosition = player2.position + cameraOffset;

            cam2.transform.position = Vector3.Lerp(
                cam2.transform.position,
                targetPosition,
                followSpeed * Time.deltaTime
            );
        }
    }
}

