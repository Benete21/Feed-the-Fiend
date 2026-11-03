
using System.Collections;
using TMPro;
using UnityEngine;

public class Day_Dialogou : MonoBehaviour
{
    [Header("Dialogue UI")]
    [SerializeField] private GameObject dialoguePanel;
    [SerializeField] private TMP_Text dialogueText;
    [SerializeField] private TMP_Text continueText;

    [Header("Settings")]
    [SerializeField] private float textSpeed = 0.03f;

    private void Start()
    {
        // Hide the dialogue panel before starting.
        if (dialoguePanel != null)
            dialoguePanel.SetActive(false);

        // Play the introductory dialogue when the scene starts.
        StartCoroutine(PlayStartDialogue());
    }

    public IEnumerator PlayStartDialogue()
    {
        string[] messages =
        {
            "Welcome to your new restaurant, chef!",
            "Your job is to prepare food and serve the monsters.",
            "The restaurant will get busier as each day passes.",
            "Get ready to cook, serve, and survive!"
        };

        if (dialoguePanel != null)
            dialoguePanel.SetActive(true);

        foreach (string message in messages)
        {
            yield return ShowMessage(message);
        }

        if (dialoguePanel != null)
            dialoguePanel.SetActive(false);
    }

    public IEnumerator PlayDayEndDialogue(int completedDay)
    {
        string[] messages;

        switch (completedDay)
        {
            case 1:
                messages = new string[]
                {
                    "Great work! Day 1 is complete!",
                    "The restaurant is getting busier.",
                    "Get ready for Day 2. More monsters are coming!"
                };
                break;

            case 2:
                messages = new string[]
                {
                    "Amazing work! You survived Day 2!",
                    "Even tougher monsters are approaching.",
                    "Prepare yourself for Day 3!"
                };
                break;

            case 3:
                messages = new string[]
                {
                    "Fantastic! You completed Day 3!",
                    "You have served every monster.",
                    "Well done, chef!"
                };
                break;

            default:
                messages = new string[]
                {
                    "Day " + completedDay + " is complete!",
                    "Get ready for the next day!"
                };
                break;
        }

        if (dialoguePanel != null)
            dialoguePanel.SetActive(true);

        foreach (string message in messages)
        {
            yield return ShowMessage(message);
        }

        if (dialoguePanel != null)
            dialoguePanel.SetActive(false);
    }

    private IEnumerator ShowMessage(string message)
    {
        if (continueText != null)
            continueText.gameObject.SetActive(false);

        if (dialogueText != null)
            dialogueText.text = "";

        // Type out the dialogue one character at a time.
        foreach (char letter in message)
        {
            if (dialogueText != null)
                dialogueText.text += letter;

            yield return new WaitForSeconds(textSpeed);
        }

        // Keep the completed message on screen briefly.
        yield return new WaitForSeconds(2f);
    }
}
