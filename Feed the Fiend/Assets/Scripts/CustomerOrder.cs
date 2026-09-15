using System.Collections;
using UnityEngine;
using UnityEngine.UI;

public class CustomerOrder : MonoBehaviour, IInteractable
{
    public Canvas loadingCanvas;

    public Slider loadingBar;
    public Slider berserkBar;
    public GameObject waitingExclamation;

    bool hasOrdered = false;

    public Food_Types[] currentOrder;

    public float maxWait = 30f;
    public float waitTime;
    public bool waiting;

    public MonsterAI monsterAI;
    public MonsterSpawner satisfied;

    public bool isBerserk = false;

    void Start()
    {
        // Hide UI at the beginning
        if (loadingBar != null)
            loadingBar.gameObject.SetActive(false);

        if (berserkBar != null)
            berserkBar.gameObject.SetActive(false);

        if (waitingExclamation != null)
            waitingExclamation.SetActive(false);
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
        if (waitTime <= 0)
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

    public void Interact(Waiter_Controls waiter)
    {

        if (!hasOrdered)
        {
            HideWaitingExclamation();

            StartCoroutine(OrderRoutine(waiter));
            return;
        }

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

            // Hide waiting UI
            if (waitingExclamation != null)
                waitingExclamation.SetActive(false);

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

    IEnumerator OrderRoutine(Waiter_Controls waiter)
    {
        hasOrdered = true;

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

        GenerateRandomOrder();

        waiter.GiveOrderSlip(currentOrder);

        // Start waiting
        waitTime = maxWait;
        waiting = true;

        // Show berserk bar
        if (berserkBar != null)
        {
            berserkBar.value = 1f;
            berserkBar.gameObject.SetActive(true);
        }
    }

    void GenerateRandomOrder()
    {
        int amount = Random.Range(1, 4);

        currentOrder = new Food_Types[amount];

        for (int i = 0; i < amount; i++)
        {
            currentOrder[i] = (Food_Types)Random.Range(0,System.Enum.GetValues(typeof(Food_Types)).Length);
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


}
