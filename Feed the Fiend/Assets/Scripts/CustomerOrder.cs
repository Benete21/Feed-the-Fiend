using System.Collections;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class CustomerOrder : MonoBehaviour, IInteractable
{

    public Canvas loadingCanvas;
    public GameObject orderPanel;
    public Slider loadingBar;

    bool hasOrdered = false;

    public Food_Types[] currentOrder;
    public TMP_Text food1;
    public TMP_Text food2;
    public TMP_Text food3;


    public void Interact()
    {
        if (hasOrdered)
            return;

        StartCoroutine(OrderRoutine());
    }

    IEnumerator OrderRoutine()
    {
        hasOrdered = true;
        loadingCanvas.gameObject.SetActive(true);
        float timer = 0f;
        float duration = 3f;

        while (timer < duration)
        {
            timer += Time.deltaTime;

            if (loadingBar != null)
                loadingBar.value = timer / duration;

            yield return null;
        }

        loadingCanvas.gameObject.SetActive(false);

        GenerateRandomOrder();

        orderPanel.SetActive(true);
    }

    void GenerateRandomOrder()
    {
        int amount = Random.Range(1, 4);

        currentOrder = new Food_Types[amount];

        for (int i = 0; i < amount; i++)
        {
            currentOrder[i] = (Food_Types)Random.Range(0, System.Enum.GetValues(typeof(Food_Types)).Length);
        }

        DisplayOrder();
    }
    void DisplayOrder()
    {
        food1.text = "";
        food2.text = "";
        food3.text = "";

        if (currentOrder.Length > 0)
            food1.text = currentOrder[0].ToString();

        if (currentOrder.Length > 1)
            food2.text = currentOrder[1].ToString();

        if (currentOrder.Length > 2)
            food3.text = currentOrder[2].ToString();
    }
}

