
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
        // Only accept objects with the Food tag.
        if (currentFood != null)
            return;

        if (!other.CompareTag("Food"))
            return;

        SnapFood(other.gameObject);
    }

    private void SnapFood(GameObject food)
    {
        if (currentFood != null || food == null)
            return;

        currentFood = food;

        Transform target = snapPoint != null
            ? snapPoint
            : transform;

        Vector3 originalScale = food.transform.lossyScale;

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

        Vector3 parentScale = target.lossyScale;

        food.transform.localScale = new Vector3(
            parentScale.x != 0 ? originalScale.x / parentScale.x : originalScale.x,
            parentScale.y != 0 ? originalScale.y / parentScale.y : originalScale.y,
            parentScale.z != 0 ? originalScale.z / parentScale.z : originalScale.z
        );

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
