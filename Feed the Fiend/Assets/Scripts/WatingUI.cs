using UnityEngine;

public class WatingUI : MonoBehaviour
{
    private Camera targetCamera;

    void Start()
    {
        targetCamera = Camera.main;
    }

    void LateUpdate()
    {
        if (targetCamera == null)
            return;

        Vector3 direction = targetCamera.transform.position - transform.position;

        // Keep the health bar upright.
        direction.y = 0f;

        if (direction.sqrMagnitude > 0.001f)
        {
            transform.rotation = Quaternion.LookRotation(direction);
        }
    }
}


