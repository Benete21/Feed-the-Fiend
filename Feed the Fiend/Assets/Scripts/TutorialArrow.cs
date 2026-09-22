using NUnit.Framework;
using System;
using TMPro;
using Unity.VisualScripting;
using UnityEngine;

public class TutorialArrow : MonoBehaviour
{
    [SerializeField] private GameObject arrowObject;
    [SerializeField] private Transform target;

    private Camera mainCamera;

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

        Vector3 screenPosition =
            mainCamera.WorldToScreenPoint(target.position);

        // Hide arrow if target is behind camera
        if (screenPosition.z < 0)
        {
            arrowObject.SetActive(false);
            return;
        }

        arrowObject.SetActive(true);

        transform.position = screenPosition;
    }

    public void ShowArrow(Transform newTarget)
    {
        target = newTarget;

        if (arrowObject != null)
            arrowObject.SetActive(true);
    }

    public void HideArrow()
    {
        target = null;

        if (arrowObject != null)
            arrowObject.SetActive(false);
    }
}

