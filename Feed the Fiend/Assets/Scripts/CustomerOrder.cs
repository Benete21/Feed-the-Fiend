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
            berserkBar.value = Mathf.Clamp01(waitTime / maxWait);
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

        // Don't start another timer
        if (waiting)
            return;

        Debug.Log("Monster is ready to order!");

        // Show the exclamation
        if (waitingExclamation != null)
            waitingExclamation.SetActive(true);

        // Reset the timer
        waitTime = maxWait;

        // Start waiting
        waiting = true;

        // Reset and show berserk bar
        if (berserkBar != null)
        {
            berserkBar.value = 1f;
            berserkBar.gameObject.SetActive(true);
        }
    }

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

        FoodItems food = held.GetComponent<FoodItems>();

        if (food == null)
            return;

        if (food.foodType == currentOrder[0])
        {
            Debug.Log("Correct food!");

            waiting = false;
            waitTime = 0f;

            // Hide waiting UI
            if (waitingExclamation != null)
                waitingExclamation.SetActive(false);

            // Hide berserk bar
            if (berserkBar != null)
                berserkBar.gameObject.SetActive(false);

            Destroy(held);

            waiter.RemoveHeldObject();
            waiter.RemoveOrderSlip();

            Satisfied();
        }
        else
        {
            Debug.Log("Wrong food!");
        }
    }

    void TakeOrder(Waiter_Controls waiter)
    {
        Debug.Log("TAKING CUSTOMER ORDER");

        // Stop the "waiting for order" timer
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

        // Generate the customer's order
        GenerateRandomOrder();

        // Give the order to the waiter
        if (waiter != null)
        {
            waiter.GiveOrderSlip(currentOrder);
        }

        // THIS IS THE IMPORTANT PART
        hasOrdered = true;

        Debug.Log("ORDER TAKEN SUCCESSFULLY!");

        // Tell TutorialManager
        OnOrderTaken?.Invoke();

        // Start the delivery timer
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
        int amount = UnityEngine.Random.Range(1, 4);

        currentOrder = new Food_Types[amount];

        Array foodTypes = Enum.GetValues(typeof(Food_Types));

        for (int i = 0; i < amount; i++)
        {
            currentOrder[i] =
                (Food_Types)foodTypes.GetValue(
                    UnityEngine.Random.Range(0, foodTypes.Length)
                );
        }
    }


    void Satisfied()
    {
        Debug.Log("OrderCorrect");

        satisfied.Served();

        Destroy(gameObject, 2f);
    }

    void Berserk()
    {
        isBerserk = true;

        Debug.Log("MONSTER HAS GONE BERSERK!");

        monsterAI.StartBerserk();
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
