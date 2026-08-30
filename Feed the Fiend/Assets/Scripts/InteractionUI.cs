using TMPro;
using UnityEngine;

public class InteractionUI : MonoBehaviour
{
    [SerializeField] private GameObject promptObject;
    [SerializeField] private TMP_Text promptText;

    private void Awake()
    {
        Hide();
    }

    public void Show(string message)
    {
        promptText.text = message;
        promptObject.SetActive(true);
    }

    public void Hide()
    {
        promptObject.SetActive(false);
    }
}


