using System.Collections;
using UnityEngine;
using UnityEngine.UI;

public class CustomerOrder : MonoBehaviour, IInteractable
{
    public Canvas loadingCanvas;
    public Slider loadingBar;
    public Slider berserkBar;

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
        if (berserkBar != null)
        {
            berserkBar.minValue = 0f;
            berserkBar.maxValue = 1f;
            berserkBar.value = 1f;

            berserkBar.gameObject.SetActive(true);
        }
    }

    void Update()
    {
        if (!waiting)
            return;

        waitTime -= Time.deltaTime;

        if (berserkBar != null)
        {
            berserkBar.value = Mathf.Clamp01(waitTime / maxWait);
        }

        if (waitTime <= 0)
        {
            waitTime = 0f;
            waiting = false;

            if (berserkBar != null)
            {
                berserkBar.value = 0f;
            }

            Berserk();

            Debug.Log("Start Berserking");
        }
    }


    public void Interact(Waiter_Controls waiter)
    {
        if (!hasOrdered)
        {
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

            if (berserkBar != null)
            {
                berserkBar.gameObject.SetActive(false);
            }

            Destroy(held);

            waiter.RemoveHeldObject();
            waiter.RemoveOrderSlip();

            Satisfied();
        }

    }

    IEnumerator OrderRoutine(Waiter_Controls waiter)
    {
        hasOrdered = true;

        // Show loading bar
        loadingBar.gameObject.SetActive(true);

        float timer = 0f;
        float duration = 3f;

        while (timer < duration)
        {
            timer += Time.deltaTime;

            if (loadingBar != null)
            {
                loadingBar.value = timer / duration;
            }

            yield return null;
        }

        // Hide loading bar
        loadingBar.gameObject.SetActive(false);

        GenerateRandomOrder();

        waiter.GiveOrderSlip(currentOrder);

        // Start waiting for the food
        waitTime = maxWait;
        waiting = true;

        // Show berserk bar
        if (berserkBar != null)
        {
            berserkBar.gameObject.SetActive(true);
            berserkBar.value = 1f;
        }
    }


    void GenerateRandomOrder()
    {
        int amount = Random.Range(1, 4);

        currentOrder = new Food_Types[amount];

        for (int i = 0; i < amount; i++)
        {
            currentOrder[i] = (Food_Types)Random.Range(
                0,
                System.Enum.GetValues(typeof(Food_Types)).Length
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

        if (berserkBar != null)
        {
            berserkBar.value = 0f;
        }

        monsterAI.StartBerserk();
    }
}
