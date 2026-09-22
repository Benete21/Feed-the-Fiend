using System;
using Unity.VisualScripting;
using UnityEngine;
using static UnityEngine.Rendering.DebugUI.Table;

public class FoodSnapPoint : MonoBehaviour
{
    [Header("Snap Settings")]
    [SerializeField] private Transform snapPoint;

    private GameObject currentFood;


    public bool HasFood()
    {
        return currentFood != null;
    }


    public GameObject GetFood()
    {
        return currentFood;
    }


    private void OnTriggerEnter(Collider other)
    {
        TrySnapFood(other);
    }


    private void TrySnapFood(Collider other)
    {
        // Already holding food
        if (currentFood != null)
            return;

        // Look for FoodItems on the object that entered
        FoodItems food = other.GetComponent<FoodItems>();

        // Check parent
        if (food == null)
        {
            food = other.GetComponentInParent<FoodItems>();
        }

        // Check children
        if (food == null)
        {
            food = other.GetComponentInChildren<FoodItems>();
        }

        // IMPORTANT:
        // If there is no FoodItems component, DO NOTHING.
        if (food == null)
        {
            Debug.Log(
                "FoodSnapPoint ignored: " +
                other.gameObject.name +
                " is not food."
            );

            return;
        }

        // Get the actual food object.
        GameObject foodObject = food.gameObject;

        // If FoodItems is on a child of the food prefab,
        // find the Rigidbody/root containing the food.
        Rigidbody foodRb = food.GetComponentInParent<Rigidbody>();

        if (foodRb != null)
        {
            foodObject = foodRb.gameObject;
        }

        SnapFood(foodObject);
    }


    private void SnapFood(GameObject food)
    {
        if (currentFood != null)
            return;

        // Double-check that this object really is food.
        FoodItems foodItem = food.GetComponentInChildren<FoodItems>();

        if (foodItem == null)
        {
            Debug.LogWarning(
                "Tried to snap an object without FoodItems: " +
                food.name
            );

            return;
        }

        currentFood = food;

        Transform target =
            snapPoint != null
                ? snapPoint
                : transform;


        // Get Rigidbody
        Rigidbody rb = food.GetComponent<Rigidbody>();

        if (rb != null)
        {
            rb.linearVelocity = Vector3.zero;
            rb.angularVelocity = Vector3.zero;

            rb.isKinematic = true;
            rb.useGravity = false;
        }


        // Disable all food colliders
        Collider[] colliders =
            food.GetComponentsInChildren<Collider>();

        foreach (Collider col in colliders)
        {
            col.enabled = false;
        }


        // Parent to snap point
        food.transform.SetParent(target);


        // Snap exactly to the position
        food.transform.localPosition = Vector3.zero;
        food.transform.localRotation = Quaternion.identity;


        Debug.Log(
            "FOOD SNAPPED: " +
            food.name +
            " -> " +
            target.name
        );
    }


    public GameObject TakeFood()
    {
        if (currentFood == null)
            return null;

        GameObject food = currentFood;

        currentFood = null;


        // Remove from snap point
        food.transform.SetParent(null);


        // Restore Rigidbody
        Rigidbody rb = food.GetComponent<Rigidbody>();

        if (rb != null)
        {
            rb.isKinematic = false;
            rb.useGravity = true;
        }


        // Re-enable colliders
        Collider[] colliders =
            food.GetComponentsInChildren<Collider>();

        foreach (Collider col in colliders)
        {
            col.enabled = true;
        }


        Debug.Log(
            "FOOD TAKEN FROM HANDOFF: " +
        food.name
        );


        return food;
    }
}
