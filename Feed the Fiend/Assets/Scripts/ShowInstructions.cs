using UnityEngine;
using TMPro;
using System.Collections;

public class ShowInstructions : MonoBehaviour
{
    [Header("Instruction Text")]
    public TextMeshProUGUI instructionText;

    [Header("How long the text stays visible")]
    public float displayTime = 5;

    private Coroutine currentCoroutine;

    private void Start()
    {
        if (instructionText != null)
        {
            instructionText.gameObject.SetActive(false);
        }
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Waiter") || other.CompareTag("Chef"))
        {
            if (currentCoroutine != null)
            {
                StopCoroutine(currentCoroutine);
            }

            currentCoroutine = StartCoroutine(ShowText());
        }
    }

    private IEnumerator ShowText()
    {
        instructionText.gameObject.SetActive(true);

        yield return new WaitForSeconds(displayTime);

        instructionText.gameObject.SetActive(false);
    }
}