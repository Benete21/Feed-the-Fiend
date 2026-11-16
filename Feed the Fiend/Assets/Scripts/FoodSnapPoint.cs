
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
                GameObject food = current.gameObject;

                Rigidbody rb = food.GetComponent<Rigidbody>();

                // Only accept food that has been dropped.
                if (rb != null && rb.isKinematic)
                    return;

                Debug.Log("Dropped food detected: " + food.name);

                SnapFood(food);
                return;
            }

            current = current.parent;
        }
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

        food.transform.SetParent(target, true);

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
