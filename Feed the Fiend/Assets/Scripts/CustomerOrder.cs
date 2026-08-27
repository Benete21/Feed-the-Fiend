using System.Collections;
using UnityEngine;
using UnityEngine.UI;

public class CustomerOrder : MonoBehaviour, IInteractable
{

    public Canvas loadingCanvas;
    public Slider loadingBar;

    bool hasOrdered = false;

    public Food_Types[] currentOrder;

    public float maxWait = 30f;
    public float waitTime;
    public bool waiting;

    public MonsterAI monsterAI;
    public MonsterSpawner satisfied;
    public bool isBerserk = false;


    void Update()
    {
        if (!waiting)
            return;

        waitTime -= Time.deltaTime;

        if (waitTime <= 0)
        {
            waiting = false;
            Berserk();
            Debug.Log("Start Berskering");
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
        {
            return;
        }

        FoodItems food = held.GetComponent<FoodItems>();

        if (food == null)
        {
            return;
        }

        if (food.foodType == currentOrder[0])
        {
            Debug.Log("Correct food!");

            waiting = false;

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

        loadingCanvas.gameObject.SetActive(true);

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

        loadingCanvas.gameObject.SetActive(false);

        GenerateRandomOrder();

        waiter.GiveOrderSlip(currentOrder);

        waitTime = maxWait;
        waiting = true;
    }

    void GenerateRandomOrder()
    {
        int amount = Random.Range(1, 4);

        currentOrder = new Food_Types[amount];

        for (int i = 0; i < amount; i++)
        {
            currentOrder[i] = (Food_Types)Random.Range(0, System.Enum.GetValues(typeof(Food_Types)).Length);
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
}


