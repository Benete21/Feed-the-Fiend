using UnityEngine;

public class FoodSnapPoint : MonoBehaviour
{
    [Header("Snap Point")]
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
        // Already have food
        if (currentFood != null)
            return;

        // ONLY accept FoodItems
        FoodItems food = other.GetComponentInParent<FoodItems>();

        if (food == null)
        {
            Debug.Log("Not food: " + other.gameObject.name);
            return;
        }

        SnapFood(food.gameObject);
    }

    private void SnapFood(GameObject food)
    {
        if (currentFood != null)
            return;

        currentFood = food;

        Transform target = snapPoint != null
            ? snapPoint
            : transform;

        // Stop physics
        Rigidbody rb = food.GetComponent<Rigidbody>();

        if (rb != null)
        {
            rb.linearVelocity = Vector3.zero;
            rb.angularVelocity = Vector3.zero;

            rb.isKinematic = true;
            rb.useGravity = false;
        }

        // Parent to snap point
        food.transform.SetParent(target);

        // EXACT position
        food.transform.localPosition = Vector3.zero;

        // EXACT rotation
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

        Rigidbody rb = food.GetComponent<Rigidbody>();

        if (rb != null)
        {
            rb.isKinematic = false;
            rb.useGravity = true;
        }

        Debug.Log(
            "FOOD TAKEN: " +
            food.name
        );

        return food;
    }
}