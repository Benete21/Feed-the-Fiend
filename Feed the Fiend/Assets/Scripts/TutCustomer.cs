using System;
using System.Collections;
using UnityEngine;
using UnityEngine.UI;
using static UnityEngine.Rendering.DebugUI.Table;

public class TutCustomer : MonoBehaviour, IInteractable
{
    [Header("Order")]
    [SerializeField] private float orderTime = 3f;
    [SerializeField] private Food_Types tutorialFood = Food_Types.TentacleMeat;

    [Header("UI")]
    [SerializeField] private Slider loadingBar;
    [SerializeField] private GameObject waitingExclamation;

    [Header("Food Delivery")]
    [SerializeField] private Collider foodDeliveryTrigger;

    private bool canTakeOrder = true;
    private bool orderTaken = false;
    private bool waitingForFood = false;

    public Food_Types[] currentOrder;

    public event Action OnOrderTaken;
    public event Action OnFoodDelivered;


    public bool HasOrdered()
    {
        return orderTaken;
    }


    private void Start()
    {
        if (loadingBar != null)
        {
            loadingBar.gameObject.SetActive(false);
            loadingBar.value = 0f;
        }

        if (waitingExclamation != null)
        {
            waitingExclamation.SetActive(true);
        }

        // Create the tutorial order.
        currentOrder = new Food_Types[1];
        currentOrder[0] = tutorialFood;

        Debug.Log(
            "Tutorial customer order: " +
            currentOrder[0]
        );
    }


    // =========================================================
    // CUSTOMER INTERACTION
    // =========================================================

    public void Interact(Waiter_Controls waiter)
    {
        if (waiter == null)
            return;

        // Take order
        if (!orderTaken)
        {
            if (!canTakeOrder)
                return;

            TakeOrder(waiter);
            return;
        }

        // The tutorial uses the food trigger for delivery,
        // so there is no need to deliver food through interaction.
    }


    // =========================================================
    // TAKE ORDER
    // =========================================================

    private void TakeOrder(Waiter_Controls waiter)
    {
        Debug.Log("TUTORIAL CUSTOMER: TAKING ORDER");

        if (!canTakeOrder)
            return;

        canTakeOrder = false;

        StartCoroutine(OrderRoutine(waiter));
    }


    private IEnumerator OrderRoutine(Waiter_Controls waiter)
    {
        if (waitingExclamation != null)
        {
            waitingExclamation.SetActive(false);
        }

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
                loadingBar.value =
                    Mathf.Clamp01(timer / orderTime);
            }

            yield return null;
        }

        if (loadingBar != null)
        {
            loadingBar.gameObject.SetActive(false);
        }


        // =====================================================
        // ORDER FINISHED
        // =====================================================

        orderTaken = true;
        waitingForFood = true;

        Debug.Log(
            "TUTORIAL ORDER TAKEN: " +
            currentOrder[0]
        );


        // Put the order slip DIRECTLY into
        // the waiter's held area.
        if (waiter != null)
        {
            waiter.GiveOrderDirectlyToHand(currentOrder);
        }


        // Tell TutorialManager that the order is finished.
        OnOrderTaken?.Invoke();


        Debug.Log(
            "TUTORIAL CUSTOMER: WAITING FOR FOOD"
        );
    }


    // =========================================================
    // FOOD TRIGGER
    // =========================================================

    private void OnTriggerEnter(Collider other)
    {
        Debug.Log(
            "Tutorial customer trigger hit by: " +
            other.gameObject.name
        );

        if (!orderTaken)
        {
            Debug.Log(
                "Tutorial customer: Order has not been taken."
            );

            return;
        }

        if (!waitingForFood)
        {
            Debug.Log(
                "Tutorial customer: Not waiting for food."
            );

            return;
        }


        // Find FoodItems
        FoodItems food =
            other.GetComponent<FoodItems>();

        if (food == null)
        {
            food =
                other.GetComponentInParent<FoodItems>();
        }

        if (food == null)
        {
            food =
                other.GetComponentInChildren<FoodItems>();
        }


        // Not food
        if (food == null)
        {
            Debug.Log(
                "Tutorial customer: Object is NOT food."
            );

            return;
        }


        Debug.Log(
            "Tutorial customer detected food: " +
            food.name
        );


        // Find waiter carrying the food
        Waiter_Controls waiter =
            food.GetComponentInParent<Waiter_Controls>();


        TryDeliverFood(food, waiter);
    }


    // =========================================================
    // FOOD DELIVERY
    // =========================================================

    private void TryDeliverFood(
        FoodItems food,
        Waiter_Controls waiter
    )
    {
        if (food == null)
            return;

        if (!orderTaken)
            return;

        if (!waitingForFood)
            return;

        if (currentOrder == null ||
            currentOrder.Length == 0)
        {
            Debug.LogWarning(
                "Tutorial customer: No current order."
            );

            return;
        }


        Debug.Log(
            "Tutorial customer wants: " +
            currentOrder[0] +
            " | Delivered: " +
            food.foodType
        );


        // =====================================================
        // WRONG FOOD
        // =====================================================

        if (food.foodType != currentOrder[0])
        {
            Debug.Log(
                "TUTORIAL CUSTOMER: WRONG FOOD!"
            );

            return;
        }


        // =====================================================
        // CORRECT FOOD
        // =====================================================

        Debug.Log(
            "TUTORIAL CUSTOMER: CORRECT FOOD DELIVERED!"
        );

        waitingForFood = false;


        // Remove food from the waiter.
        if (waiter != null)
        {
            waiter.RemoveHeldObject();

            // Remove the order slip UI.
            waiter.RemoveOrderSlip();
        }
        else
        {
            Destroy(food.gameObject);
        }


        // Tell TutorialManager.
        OnFoodDelivered?.Invoke();


        Debug.Log(
            "TUTORIAL CUSTOMER: FOOD DELIVERY COMPLETE!"
        );
    }
}

