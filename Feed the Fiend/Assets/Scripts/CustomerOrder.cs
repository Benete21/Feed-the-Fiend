using System.Collections;
using UnityEngine;
using UnityEngine.UI;
using System;

public class CustomerOrder : MonoBehaviour, IInteractable
{
    public Canvas loadingCanvas;

    public Slider loadingBar;
    public Slider berserkBar;
    public GameObject waitingExclamation;

    private bool hasOrdered = false;

    public Food_Types[] currentOrder;

    public float maxWait = 30f;
    public float waitTime;
    public bool waiting;

    public MonsterAI monsterAI;
    public MonsterSpawner satisfied;

    public bool isBerserk = false;

    public event Action OnOrderTaken;
    public event Action OnOrderStarted;


    void Start()
    {
        // Hide UI at the beginning
        if (loadingBar != null)
            loadingBar.gameObject.SetActive(false);

        if (berserkBar != null)
            berserkBar.gameObject.SetActive(false);

        if (waitingExclamation != null)
            waitingExclamation.SetActive(false);

        waiting = false;
        waitTime = 0f;
    }


    void Update()
    {
        if (!waiting)
            return;

        waitTime -= Time.deltaTime;

        // Update berserk timer
        if (berserkBar != null)
        {
            berserkBar.value =
                Mathf.Clamp01(waitTime / maxWait);
        }

        // Time ran out
        if (waitTime <= 0f)
        {
            waitTime = 0f;
            waiting = false;

            if (berserkBar != null)
                berserkBar.gameObject.SetActive(false);

            if (waitingExclamation != null)
                waitingExclamation.SetActive(false);

            Berserk();

            Debug.Log("Start Berserking");
        }
    }


    // Called by MonsterAI when the monster reaches its table
    public void StartWaitingForOrder()
    {
        if (isBerserk)
            return;

        if (waiting)
            return;

        Debug.Log("Monster is ready to order!");

        if (waitingExclamation != null)
            waitingExclamation.SetActive(true);

        waitTime = maxWait;
        waiting = true;

        if (berserkBar != null)
        {
            berserkBar.value = 1f;
            berserkBar.gameObject.SetActive(true);
        }
    }


    // Called when the waiter interacts with the monster
    public void Interact(Waiter_Controls waiter)
    {
        // Monster has not reached the table yet
        if (!waiting && !hasOrdered)
            return;

        // Take the order
        if (!hasOrdered)
        {
            TakeOrder(waiter);
            return;
        }

        // Order has already been taken
        if (!waiting)
            return;

        GameObject held = waiter.GetHeldObject();

        if (held == null)
            return;

        FoodItems food =
            held.GetComponentInChildren<FoodItems>();

        if (food == null)
            return;

        TryDeliverFood(food);
    }


    // Starts taking the customer's order
    void TakeOrder(Waiter_Controls waiter)
    {
        Debug.Log("TAKING CUSTOMER ORDER");

        OnOrderStarted?.Invoke();

        waiting = false;
        waitTime = 0f;

        HideWaitingExclamation();

        StartCoroutine(OrderRoutine(waiter));
    }


    IEnumerator OrderRoutine(Waiter_Controls waiter)
    {
        waiting = false;

        if (berserkBar != null)
            berserkBar.gameObject.SetActive(false);

        // Show loading bar
        if (loadingBar != null)
        {
            loadingBar.gameObject.SetActive(true);
            loadingBar.value = 0f;
        }

        float timer = 0f;
        float duration = 3f;

        while (timer < duration)
        {
            timer += Time.deltaTime;

            if (loadingBar != null)
                loadingBar.value = timer / duration;

            yield return null;
        }

        // Hide loading bar
        if (loadingBar != null)
            loadingBar.gameObject.SetActive(false);

        // Generate order
        GenerateRandomOrder();

        // Give order slip to waiter
        if (waiter != null)
        {
            waiter.GiveOrderDirectlyToHand(currentOrder);
        }

        hasOrdered = true;

        Debug.Log("ORDER TAKEN SUCCESSFULLY!");

        OnOrderTaken?.Invoke();

        // Start delivery timer
        waitTime = maxWait;
        waiting = true;

        if (berserkBar != null)
        {
            berserkBar.value = 1f;
            berserkBar.gameObject.SetActive(true);
        }
    }

    void GenerateRandomOrder()
    {
        // Customer orders exactly ONE food item
        currentOrder = new Food_Types[1];

        Array foodTypes =
            Enum.GetValues(typeof(Food_Types));

        currentOrder[0] =
            (Food_Types)foodTypes.GetValue(
                UnityEngine.Random.Range(
                    0,
                    foodTypes.Length
                )
            );

        Debug.Log("Customer ordered: " + currentOrder[0]);
    }


    // =========================================================
    // FOOD DELIVERY
    // =========================================================

    public void TryDeliverFood(FoodItems food)
    {
        if (food == null)
            return;

        if (!hasOrdered)
        {
            Debug.Log("Customer has not ordered yet.");
            return;
        }

        if (!waiting)
        {
            Debug.Log("Customer is not waiting for food.");
            return;
        }

        if (currentOrder == null ||
            currentOrder.Length == 0)
        {
            Debug.LogWarning("Customer has no current order.");
            return;
        }

        Debug.Log(
            "Customer wants: " +
            currentOrder[0] +
            " | Delivered: " +
            food.foodType
        );

        // Wrong food
        if (food.foodType != currentOrder[0])
        {
            Debug.Log("WRONG FOOD!");
            return;
        }

        // Correct food
        Debug.Log("CORRECT FOOD DELIVERED!");

        waiting = false;
        waitTime = 0f;

        if (waitingExclamation != null)
            waitingExclamation.SetActive(false);

        if (berserkBar != null)
            berserkBar.gameObject.SetActive(false);

        // Find waiter
        Waiter_Controls waiter =
            food.GetComponentInParent<Waiter_Controls>();

        if (waiter != null)
        {
            waiter.RemoveHeldObject();
            waiter.RemoveOrderSlip();
        }
        else
        {
            // If the food isn't a child of the waiter,
            // destroy it directly.
            Destroy(food.gameObject);
        }

        Satisfied();
    }


    // =========================================================
    // CUSTOMER SATISFIED
    // =========================================================

void Satisfied()
    {
        Debug.Log("OrderCorrect");

        if (satisfied != null)
        {
            satisfied.Served();
        }

        if (monsterAI != null)
        {
            monsterAI.BecomeSatisfied();
        }
    }



    // =========================================================
    // BERSERK
    // =========================================================

    void Berserk()
    {
        isBerserk = true;

        Debug.Log("MONSTER HAS GONE BERSERK!");

        if (monsterAI != null)
        {
            monsterAI.StartBerserk();
        }
    }


    public void ShowWaitingExclamation()
    {
        if (waitingExclamation != null)
        {
            waitingExclamation.SetActive(true);
        }
    }


    public void HideWaitingExclamation()
    {
        if (waitingExclamation != null)
        {
            waitingExclamation.SetActive(false);
        }
    }


    public bool HasOrdered()
    {
        return hasOrdered;
    }
}
