using NUnit.Framework;
using System;
using System.Collections;
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

    private bool canTakeOrder = true;
    private bool orderTaken = false;
    private bool waitingForFood = false;

    public Food_Types[] currentOrder;

    // TutorialManager listens to these.
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

        // Create the tutorial order immediately.
        currentOrder = new Food_Types[1];
        currentOrder[0] = tutorialFood;
    }


    public void Interact(Waiter_Controls waiter)
    {
        if (waiter == null)
            return;


        // =====================================================
        // TAKE ORDER
        // =====================================================

        if (!orderTaken)
        {
            if (!canTakeOrder)
                return;

            StartCoroutine(TakeOrderRoutine(waiter));
            return;
        }


        // =====================================================
        // DELIVER FOOD
        // =====================================================

        if (orderTaken && waitingForFood)
        {
            GameObject held = waiter.GetHeldObject();

            if (held == null)
            {
                Debug.Log("Tutorial customer: Waiter is not holding food.");
                return;
            }

            FoodItems food = held.GetComponentInChildren<FoodItems>();

            if (food == null)
            {
                Debug.Log("Tutorial customer: Held object is not food.");
                return;
            }

            TryDeliverFood(food, waiter);
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
                loadingBar.value = Mathf.Clamp01(
                    timer / orderTime
                );
            }

            yield return null;
        }

        if (loadingBar != null)
        {
            loadingBar.gameObject.SetActive(false);
        }

        // Give the order slip to the waiter.
        waiter.GiveOrderSlip(currentOrder);

        orderTaken = true;

        // Customer is now waiting for food.
        waitingForFood = true;

        Debug.Log(
            "Tutorial customer: ORDER TAKEN! " +
            "Waiting for " +
            currentOrder[0]
        );

        OnOrderTaken?.Invoke();
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
        // CHECK FOOD TYPE
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


        // Remove food from waiter.
        if (waiter != null)
        {
            waiter.RemoveHeldObject();
            waiter.RemoveOrderSlip();
        }
        else
        {
            Destroy(food.gameObject);
        }


        // Tell TutorialManager that delivery happened.
        OnFoodDelivered?.Invoke();

        Debug.Log(
            "Tutorial customer: FOOD DELIVERY COMPLETE!"
        );
    }
}
