using System.Collections;
using UnityEngine;

public class TutorialArrow : MonoBehaviour
{
    [Header("Arrow")]
    [SerializeField] private GameObject arrowObject;
    [SerializeField] private float showDuration = 6f;

    private Transform target;
    private Camera mainCamera;
    private Coroutine hideCoroutine;

    private void Awake()
    {
        mainCamera = Camera.main;

        if (arrowObject != null)
            arrowObject.SetActive(false);
    }

    private void Update()
    {
        if (target == null)
            return;

        if (mainCamera == null)
            mainCamera = Camera.main;

        if (mainCamera == null)
            return;

        Vector3 screenPosition = mainCamera.WorldToScreenPoint(target.position);

        if (screenPosition.z < 0f)
        {
            if (arrowObject != null)
                arrowObject.SetActive(false);
            return;
        }

        if (arrowObject != null && !arrowObject.activeSelf)
            arrowObject.SetActive(true);

        transform.position = screenPosition;
    }

    public void ShowArrow(Transform newTarget)
    {
        target = newTarget;

        if (hideCoroutine != null)
            StopCoroutine(hideCoroutine);

        if (arrowObject != null)
            arrowObject.SetActive(target != null);

        if (target != null && showDuration > 0f)
            hideCoroutine = StartCoroutine(HideAfterDelay());
    }

    private IEnumerator HideAfterDelay()
    {
        yield return new WaitForSeconds(showDuration);
        HideArrow();
    }

    public void HideArrow()
    {
        target = null;

        if (hideCoroutine != null)
        {
            StopCoroutine(hideCoroutine);
            hideCoroutine = null;
        }

        if (arrowObject != null)
            arrowObject.SetActive(false);
    }
}
