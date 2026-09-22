using System;
using System.Collections;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.UI;
using static System.Net.Mime.MediaTypeNames;

public class TutCustomer : MonoBehaviour, IInteractable
{
    [Header("Order")]
    [SerializeField] private float orderTime = 3f;
    [SerializeField] private Food_Types tutorialFood = Food_Types.Food_Green;

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

        // Create tutorial order
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

            StartCoroutine(TakeOrderRoutine(waiter));
        }
    }


    // =========================================================
    // TAKE ORDER
    // =========================================================

    private IEnumerator TakeOrderRoutine(Waiter_Controls waiter)
    {
        canTakeOrder = false;

        Debug.Log("Tutorial customer: Taking order...");

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
                loadingBar.value =
                    Mathf.Clamp01(timer / orderTime);
            }

            yield return null;
        }

        if (loadingBar != null)
            loadingBar.gameObject.SetActive(false);

        // Give order to waiter
        waiter.GiveOrderSlip(currentOrder);

        orderTaken = true;
        waitingForFood = true;

        Debug.Log(
            "Tutorial customer: ORDER TAKEN. " +
            "Waiting for: " +
            currentOrder[0]
        );

        OnOrderTaken?.Invoke();
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

        // Customer must already have an order
        if (!orderTaken)
        {
            Debug.Log(
                "Tutorial customer: No order has been taken yet."
            );

            return;
        }

        // Customer must be waiting for food
        if (!waitingForFood)
        {
            Debug.Log(
                "Tutorial customer: Not waiting for food."
            );

            return;
        }

        // Look for FoodItems on the collider
        FoodItems food =
            other.GetComponent<FoodItems>();

        // FoodItems might be on a parent
        if (food == null)
        {
            food =
                other.GetComponentInParent<FoodItems>();
        }

        // FoodItems might be on a child
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

        // Find the waiter holding the food
        Waiter_Controls waiter =
            food.GetComponentInParent<Waiter_Controls>();

        TryDeliverFood(food, waiter);
    }


    // =========================================================
    // DELIVER FOOD
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
            " | Food received: " +
            food.foodType
        );


        // =====================================================
        // WRONG FOOD
        // =====================================================

        if (food.foodType != currentOrder[0])
        {
            Debug.Log(
                "Tutorial customer: WRONG FOOD!"
            );

            return;
        }


        // =====================================================
        // CORRECT FOOD
        // =====================================================

        Debug.Log(
            "Tutorial customer: CORRECT FOOD DELIVERED!"
        );

        waitingForFood = false;


        // Remove food from waiter
        if (waiter != null)
        {
            waiter.RemoveHeldObject();
            waiter.RemoveOrderSlip();
        }
        else
        {
            // If we couldn't find the waiter,
            // destroy the delivered food directly.
            Destroy(food.gameObject);
        }


        // Tell TutorialManager
        OnFoodDelivered?.Invoke();

        Debug.Log(
            "Tutorial customer: FOOD DELIVERY COMPLETE!"
        );
    }
}
