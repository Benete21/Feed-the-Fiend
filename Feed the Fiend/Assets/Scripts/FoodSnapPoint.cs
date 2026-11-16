
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
        if (currentFood != null)
            return;

        // Search the collider object and its parents for the Food tag.
        Transform current = other.transform;

        while (current != null)
        {
            if (current.CompareTag("Food"))
            {
                Debug.Log("Food detected by handoff point: " + current.name);

                SnapFood(current.gameObject);
                return;
            }

            current = current.parent;
        }

        Debug.Log("Object rejected by handoff point: " + other.name);
    }

    private void SnapFood(GameObject food)
    {
        if (currentFood != null || food == null)
            return;

        currentFood = food;

        Transform target = snapPoint != null
            ? snapPoint
            : transform;

        Rigidbody rb = food.GetComponent<Rigidbody>();

        if (rb != null)
        {
            rb.linearVelocity = Vector3.zero;
            rb.angularVelocity = Vector3.zero;
            rb.isKinematic = true;
            rb.useGravity = false;
        }

        // Preserve the food's original world scale.
        food.transform.SetParent(target, true);

        // Snap position and rotation without changing scale.
        food.transform.position = target.position;
        food.transform.rotation = target.rotation;

        Debug.Log("FOOD SNAPPED: " + food.name);
    }

    public GameObject TakeFood()
    {
        if (currentFood == null)
            return null;

        GameObject food = currentFood;
        currentFood = null;

        // Preserve its world scale when removing it from the snap point.
        food.transform.SetParent(null, true);

        Rigidbody rb = food.GetComponent<Rigidbody>();

        if (rb != null)
        {
            rb.isKinematic = false;
            rb.useGravity = true;
        }

        Debug.Log("FOOD TAKEN: " + food.name);

        return food;
    }
}
