using UnityEngine;
using TMPro;

public class ShowTextAtStart : MonoBehaviour
{
    public TextMeshProUGUI instructionText;
    public float displayTime = 5f;

    private void Start()
    {
        StartCoroutine(ShowText());
    }

    private System.Collections.IEnumerator ShowText()
    {
        instructionText.gameObject.SetActive(true);

        yield return new WaitForSeconds(displayTime);

        instructionText.gameObject.SetActive(false);
    }
}