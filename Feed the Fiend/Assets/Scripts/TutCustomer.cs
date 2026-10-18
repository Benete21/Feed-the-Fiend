
using System;
using System.Collections;
using UnityEngine;
using UnityEngine.UI;

public class TutCustomer : MonoBehaviour, IInteractable
{
    [Header("Order")]
    [SerializeField] private float orderTime = 3f;
    [SerializeField]
    private Food_Types tutorialFood =
        Food_Types.Tenticle_Meat_Dish;

    [Header("Table Assignment")]
    [SerializeField] private int tableNumber = 1;
    public int TableNumber => tableNumber;

    [Header("UI")]
    [SerializeField] private Slider loadingBar;
    [SerializeField] private GameObject waitingExclamation;

    [Header("Food Delivery")]
    [SerializeField] private Collider foodDeliveryTrigger;

    private bool canTakeOrder = true;
    private bool orderTaken = false;
    private bool waitingForFood = false;
    private bool foodDelivered = false;

    public Food_Types[] currentOrder;

    public event Action OnOrderTaken;
    public event Action OnFoodDelivered;
    public event Action<TutCustomer> OnOrderCompleted;

    public bool HasOrdered()
    {
        return orderTaken;
    }

    public bool IsWaitingForFood()
    {
        return waitingForFood;
    }

    private void Start()
    {
        if (loadingBar != null)
        {
            loadingBar.gameObject.SetActive(false);
            loadingBar.value = 0f;
        }

        if (waitingExclamation != null)
            waitingExclamation.SetActive(true);

        // Create the tutorial order.
        currentOrder = new Food_Types[1];
        currentOrder[0] = tutorialFood;

        Debug.Log(
            "Tutorial customer at table " + tableNumber +
            " wants " + currentOrder[0]
        );
    }

    // =====================================================
    // CUSTOMER INTERACTION
    // =====================================================

    public void Interact(Waiter_Controls waiter)
    {
        if (waiter == null)
            return;

        // Only allow the order to be taken once.
        if (!orderTaken)
        {
            if (!canTakeOrder)
                return;

            TakeOrder(waiter);
        }

        // Food delivery is handled by the delivery trigger.
    }

    // =====================================================
    // TAKE ORDER
    // =====================================================

    private void TakeOrder(Waiter_Controls waiter)
    {
        if (!canTakeOrder || orderTaken)
            return;

        canTakeOrder = false;

        Debug.Log(
            "Taking tutorial order for table " + tableNumber
        );

        StartCoroutine(OrderRoutine(waiter));
    }

    private IEnumerator OrderRoutine(Waiter_Controls waiter)
    {
        if (waitingExclamation != null)
            waitingExclamation.SetActive(false);

        if (loadingBar != null)
        {
            loadingBar.gameObject.SetActive(true);
            loadingBar.value = 0f;
        }

        float timer = 0f;

        while (timer < orderTime)
        {
            timer += Time.deltaTime;

            if (loadingBar != null)
            {
                loadingBar.value = orderTime > 0f
                    ? Mathf.Clamp01(timer / orderTime)
                    : 1f;
            }

            yield return null;
        }

        if (loadingBar != null)
            loadingBar.gameObject.SetActive(false);

        // Order is now ready.
        orderTaken = true;
        waitingForFood = true;

        Debug.Log(
            "Tutorial order taken. Table: " + tableNumber +
            ", Food: " + currentOrder[0]
        );

        // Give the order slip directly to the waiter.
        // The slip receives this customer's table number
        // and reference for later table assignment.
        if (waiter != null)
        {
            waiter.GiveOrderDirectlyToHand(
                currentOrder,
                tableNumber
            );
        }

        OnOrderTaken?.Invoke();

        Debug.Log(
            "Tutorial customer at table " + tableNumber +
            " is waiting for food."
        );
    }

    // =====================================================
    // FOOD DELIVERY TRIGGER
    // =====================================================

    private void OnTriggerEnter(Collider other)
    {
        if (!orderTaken || !waitingForFood || foodDelivered)
            return;

        // Find food on the collider, its parent or its children.
        FoodItems food = other.GetComponent<FoodItems>();

        if (food == null)
            food = other.GetComponentInParent<FoodItems>();

        if (food == null)
            food = other.GetComponentInChildren<FoodItems>();

        if (food == null)
            return;

        // Find the waiter carrying the food.
        Waiter_Controls waiter =
            food.GetComponentInParent<Waiter_Controls>();

        TryDeliverFood(food, waiter);
    }

    // =====================================================
    // FOOD DELIVERY
    // =====================================================

    private void TryDeliverFood(
        FoodItems food,
        Waiter_Controls waiter)
    {
        if (food == null ||
            !orderTaken ||
            !waitingForFood ||
            foodDelivered)
        {
            return;
        }

        if (currentOrder == null || currentOrder.Length == 0)
        {
            Debug.LogWarning(
                "Tutorial customer has no current order."
            );
            return;
        }

        Debug.Log(
            "Table " + tableNumber +
            " wants " + currentOrder[0] +
            ". Delivered: " + food.foodType
        );

        // Reject incorrect food.
        if (food.foodType != currentOrder[0])
        {
            Debug.Log("Tutorial customer: WRONG FOOD!");
            return;
        }

        // Correct food delivered.
        foodDelivered = true;
        waitingForFood = false;

        Debug.Log(
            "Correct food delivered to tutorial table " +
            tableNumber
        );

        if (waiter != null)
        {
            waiter.RemoveHeldObject();
            waiter.RemoveOrderSlip();
        }
        else
        {
            Destroy(food.gameObject);
        }

        // Notify the tutorial manager.
        OnFoodDelivered?.Invoke();
        OnOrderCompleted?.Invoke(this);

        Debug.Log(
            "Tutorial order completed at table " + tableNumber
        );
    }

    // =====================================================
    // RESET / QUERY HELPERS
    // =====================================================

    public bool IsFoodDelivered()
    {
        return foodDelivered;
    }

    public Food_Types GetOrderedFood()
    {
        return tutorialFood;
    }
}
