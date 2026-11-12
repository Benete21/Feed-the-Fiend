
using UnityEngine;

public class TutorialArrow : MonoBehaviour
{
    [Header("Arrow")]
    [SerializeField] private GameObject arrowObject;

    [Header("Position")]
    [SerializeField] private Vector3 targetOffset = new Vector3(0f, 2f, 0f);
    [SerializeField] private bool followTarget = true;

    [Header("Rotation")]
    [SerializeField] private bool faceCamera = true;
    [SerializeField] private float rotationSpeed = 8f;

    private Transform target;
    private Camera mainCamera;

    private void Awake()
    {
        mainCamera = Camera.main;

        if (arrowObject != null)
            arrowObject.SetActive(false);
    }

    private void LateUpdate()
    {
        if (target == null || arrowObject == null)
            return;

        if (followTarget)
        {
            arrowObject.transform.position =
                target.position + targetOffset;
        }

        if (faceCamera)
        {
            if (mainCamera == null)
                mainCamera = Camera.main;

            if (mainCamera != null)
            {
                Vector3 direction =
                    mainCamera.transform.position -
                    arrowObject.transform.position;

                if (direction.sqrMagnitude > 0.001f)
                {
                    Quaternion targetRotation =
                        Quaternion.LookRotation(direction);

                    arrowObject.transform.rotation =
                        Quaternion.Slerp(
                            arrowObject.transform.rotation,
                            targetRotation,
                            rotationSpeed * Time.deltaTime
                        );
                }
            }
        }
    }

    public void ShowArrow(Transform newTarget)
    {
        target = newTarget;

        if (arrowObject == null)
            return;

        if (target == null)
        {
            HideArrow();
            return;
        }

        arrowObject.transform.position =
            target.position + targetOffset;

        arrowObject.SetActive(true);
    }

    public void HideArrow()
    {
        target = null;

        if (arrowObject != null)
            arrowObject.SetActive(false);
    }
}
