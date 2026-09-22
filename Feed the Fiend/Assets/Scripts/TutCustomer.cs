using UnityEngine;
using System;
using System.Collections;
using UnityEngine.UI;

public class TutCustomer : MonoBehaviour, IInteractable
{

    [Header("Order")]
    [SerializeField] private float orderTime = 3f;
    public event Action OnFoodDelivered;

    [Header("UI")]
    [SerializeField] private Slider loadingBar;
    [SerializeField] private GameObject waitingExclamation;

    [Header("Order Slip")]
    [SerializeField] private Food_Types tutorialFood = Food_Types.Food_Green;

    private bool canTakeOrder = true;
    private bool orderTaken = false;

    public Food_Types[] currentOrder;

    // TutorialManager listens to this.
    public event Action OnOrderTaken;

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
        if (!canTakeOrder)
            return;

        if (orderTaken)
            return;

        if (waiter == null)
            return;

        StartCoroutine(TakeOrderRoutine(waiter));
    }

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
                loadingBar.value = Mathf.Clamp01(timer / orderTime);
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

        Debug.Log("Tutorial customer: ORDER TAKEN!");

        // Tell TutorialManager.
        OnOrderTaken?.Invoke();
    }
}

