
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

    public bool TrySnapFood(GameObject food)
    {
        if (food == null || currentFood != null)
            return false;

        if (!food.CompareTag("Food"))
            return false;

        Transform target = snapPoint != null ? snapPoint : transform;

        // Keep the food independent of the snap point's scale.
        food.transform.SetParent(null, true);
        food.transform.position = target.position;
        food.transform.rotation = target.rotation;

        Rigidbody rb = food.GetComponent<Rigidbody>();

        if (rb != null)
        {
            rb.linearVelocity = Vector3.zero;
            rb.angularVelocity = Vector3.zero;
            rb.isKinematic = false;
            rb.useGravity = true;
        }

        currentFood = food;

        Debug.Log("FOOD SNAPPED: " + food.name);
        return true;
    }

    // Called when the waiter collects the food.
    public void ReleaseFood(GameObject food)
    {
        if (currentFood != food)
            return;

        currentFood = null;
        Debug.Log("FOOD TAKEN FROM HANDOFF: " + food.name);
    }
}
