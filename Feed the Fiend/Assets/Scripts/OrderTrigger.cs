using UnityEngine;

public class OrderTrigger : MonoBehaviour
{
    private CustomerOrder customerOrder;

    private void Awake()
    {
        customerOrder = GetComponentInParent<CustomerOrder>();

        if (customerOrder == null)
        {
            Debug.LogError("CustomerDeliveryTrigger could not find CustomerOrder in parent!");
        }
    }

    private void OnTriggerEnter(Collider other)
    {
        if (customerOrder == null)
            return;

        FoodItems food = other.GetComponentInParent<FoodItems>();

        if (food == null)
            return;

        Debug.Log("FOOD ENTERED CUSTOMER TRIGGER: " + food.foodType);

        customerOrder.TryDeliverFood(food);
    }
}

