
using System.Collections;
using System;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class CustomerOrder : MonoBehaviour, IInteractable
{
    [Header("Waiting UI")]
    public Canvas loadingCanvas;
    public Slider loadingBar;
    public Slider berserkBar;
    public GameObject waitingExclamation;

    [Header("Order")]
    private bool hasOrdered = false;
    public Food_Types[] currentOrder;

    public float maxWait = 30f;
    public float waitTime;
    public bool waiting;

    [Header("Monster")]
    public MonsterAI monsterAI;
    public MonsterSpawner satisfied;
    public bool isBerserk = false;

    [Header("Table Food Delivery")]
    [SerializeField] private Transform foodDeliveryPoint;
    [SerializeField] private float foodTravelDuration = 0.8f;

    [Header("Eating Effect")]
    [SerializeField] private Transform eatingEffectPoint;
    [SerializeField] private ParticleSystem eatingEffectPrefab;
    [SerializeField] private float eatingDuration = 2.5f;

    [Header("Inventory")]
    [SerializeField] private Chef_Order_Inventory chefInventory;

    private bool isDelivering = false;

    public event Action OnOrderTaken;
    public event Action OnOrderStarted;

    [SerializeField] private int tableNumber = 1;
    public int TableNumber => tableNumber;

    public event Action<CustomerOrder> OnOrderCompleted;

    private void Start()
    {
        if (loadingBar != null)
            loadingBar.gameObject.SetActive(false);

        if (berserkBar != null)
            berserkBar.gameObject.SetActive(false);

        if (waitingExclamation != null)
            waitingExclamation.SetActive(false);

        waiting = false;
        waitTime = 0f;
    }

    private void Update()
    {
        if (!waiting || isDelivering)
            return;

        waitTime -= Time.deltaTime;

        if (berserkBar != null)
            berserkBar.value = Mathf.Clamp01(waitTime / maxWait);

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

    public void StartWaitingForOrder()
    {
        if (isBerserk || waiting)
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

    public void Interact(Waiter_Controls waiter)
    {
        if (isDelivering)
            return;

        if (!waiting && !hasOrdered)
            return;

        if (!hasOrdered)
        {
            TakeOrder(waiter);
            return;
        }

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

    private void TakeOrder(Waiter_Controls waiter)
    {
        Debug.Log("TAKING CUSTOMER ORDER");

        OnOrderStarted?.Invoke();

        waiting = false;
        waitTime = 0f;

        HideWaitingExclamation();

        StartCoroutine(OrderRoutine(waiter));
    }

    private IEnumerator OrderRoutine(Waiter_Controls waiter)
    {
        waiting = false;

        if (berserkBar != null)
            berserkBar.gameObject.SetActive(false);

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

        if (loadingBar != null)
            loadingBar.gameObject.SetActive(false);

        GenerateRandomOrder();

        if (waiter != null)
        {
            waiter.GiveOrderDirectlyToHand(
                currentOrder,
                TableNumber
            );
        }

        hasOrdered = true;

        Debug.Log("ORDER TAKEN SUCCESSFULLY!");

        OnOrderTaken?.Invoke();

        waitTime = maxWait;
        waiting = true;

        if (berserkBar != null)
        {
            berserkBar.value = 1f;
            berserkBar.gameObject.SetActive(true);
        }
    }

    private void GenerateRandomOrder()
    {
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
        if (food == null || isDelivering)
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

        if (currentOrder == null || currentOrder.Length == 0)
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

        // Check the food before removing the inventory order.
        if (food.foodType != currentOrder[0])
        {
            Debug.Log("WRONG FOOD!");
            return;
        }

        Debug.Log("CORRECT FOOD DELIVERED!");

        isDelivering = true;
        waiting = false;
        waitTime = 0f;

        if (waitingExclamation != null)
            waitingExclamation.SetActive(false);

        if (berserkBar != null)
            berserkBar.gameObject.SetActive(false);

        StartCoroutine(FoodDeliveryRoutine(food));
    }

    private IEnumerator FoodDeliveryRoutine(FoodItems food)
    {
        // Get the actual held object from the waiter.
        Waiter_Controls waiter =
            FindObjectOfType<Waiter_Controls>();

        GameObject deliveredObject = food.gameObject;

        if (waiter != null && waiter.GetHeldObject() != null)
        {
            deliveredObject = waiter.GetHeldObject();
        }

        if (deliveredObject == null)
            yield break;

        // Detach the food from the waiter.
        deliveredObject.transform.SetParent(null);

        Rigidbody rb =
            deliveredObject.GetComponent<Rigidbody>();

        if (rb != null)
        {
            rb.linearVelocity = Vector3.zero;
            rb.angularVelocity = Vector3.zero;
            rb.isKinematic = true;
            rb.useGravity = false;
        }

        // Disable colliders while the food travels.
        Collider[] foodColliders =
            deliveredObject.GetComponentsInChildren<Collider>();

        foreach (Collider foodCollider in foodColliders)
        {
            foodCollider.enabled = false;
        }

        // Choose where the food should arrive.
        Vector3 targetPosition;

        if (foodDeliveryPoint != null)
        {
            targetPosition = foodDeliveryPoint.position;
        }
        else if (eatingEffectPoint != null)
        {
            targetPosition = eatingEffectPoint.position;
        }
        else if (monsterAI != null)
        {
            targetPosition = monsterAI.transform.position +
                             Vector3.up;
        }
        else
        {
            targetPosition = transform.position +
                             Vector3.up;
        }

        Vector3 startPosition =
            deliveredObject.transform.position;

        Quaternion startRotation =
            deliveredObject.transform.rotation;

        float timer = 0f;

        // Move food towards the table.
        while (timer < foodTravelDuration &&
               deliveredObject != null)
        {
            timer += Time.deltaTime;

            float progress = foodTravelDuration > 0f
                ? Mathf.Clamp01(timer / foodTravelDuration)
                : 1f;

            float smoothProgress =
                Mathf.SmoothStep(0f, 1f, progress);

            deliveredObject.transform.position =
                Vector3.Lerp(
                    startPosition,
                    targetPosition,
                    smoothProgress
                );

            deliveredObject.transform.rotation =
                Quaternion.Slerp(
                    startRotation,
                    Quaternion.identity,
                    smoothProgress
                );

            yield return null;
        }

        if (deliveredObject == null)
            yield break;

        deliveredObject.transform.position = targetPosition;

        // Play eating particle effect.
        if (eatingEffectPrefab != null)
        {
            Vector3 effectPosition =
                eatingEffectPoint != null
                ? eatingEffectPoint.position
                : targetPosition;

            ParticleSystem effect = Instantiate(
                eatingEffectPrefab,
                effectPosition,
                eatingEffectPrefab.transform.rotation
            );

            effect.Play();

            Destroy(
                effect.gameObject,
                effect.main.duration +
                effect.main.startLifetime.constantMax
            );
        }

        Debug.Log("Monster is eating!");

        // Keep the monster at the table while eating.
        yield return new WaitForSeconds(eatingDuration);

        // Remove the delivered food.
        if (waiter != null)
        {
            waiter.RemoveHeldObject();
            waiter.RemoveOrderSlip();
        }

        if (deliveredObject != null)
            Destroy(deliveredObject);

        // Complete the inventory order only after delivery.
        if (chefInventory != null)
        {
            chefInventory.CompleteOrderForTable(TableNumber);
        }

        OnOrderCompleted?.Invoke(this);

        isDelivering = false;

        // Start the existing monster leaving sequence.
        Satisfied();
    }

    // =========================================================
    // CUSTOMER SATISFIED
    // =========================================================

    private void Satisfied()
    {
        Debug.Log("OrderCorrect");

        if (monsterAI != null)
        {
            monsterAI.BecomeSatisfied();
        }
    }

    // =========================================================
    // BERSERK
    // =========================================================

    private void Berserk()
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
            waitingExclamation.SetActive(true);
    }

    public void HideWaitingExclamation()
    {
        if (waitingExclamation != null)
            waitingExclamation.SetActive(false);
    }

    public bool HasOrdered()
    {
        return hasOrdered;
    }
}
