
using System.Collections;
using TMPro;
using UnityEngine;

public class Day_Dialogou : MonoBehaviour
{
    [Header("Dialogue UI")]
    [SerializeField] private GameObject dialoguePanel;
    [SerializeField] private TMP_Text dialogueText;
    [SerializeField] private TMP_Text continueText;

    [Header("Cooking Station Unlock")]
    [SerializeField] private GameObject cookingUI;

    [Header("Player Controls")]
    [SerializeField] private MonoBehaviour[] playerControlScripts;

    [Header("Monster Controls")]
    [SerializeField] private MonoBehaviour monsterSpawner;
    [SerializeField] private MonoBehaviour[] monsterControlScripts;

    [Header("Settings")]
    [SerializeField] private float textSpeed = 0.03f;

    private void Awake()
    {
        // Stop spawning before the scene's Start methods run.
        if (monsterSpawner != null)
            monsterSpawner.enabled = false;
    }

    private void Start()
    {
        if (dialoguePanel != null)
            dialoguePanel.SetActive(false);

        if (cookingUI != null)
            cookingUI.SetActive(false);

        StartCoroutine(PlayStartDialogue());
    }

    private void SetPlayersFrozen(bool frozen)
    {
        if (playerControlScripts == null)
            return;

        foreach (MonoBehaviour control in playerControlScripts)
        {
            if (control != null)
                control.enabled = !frozen;
        }
    }

    private void SetMonstersFrozen(bool frozen)
    {
        if (monsterControlScripts == null)
            return;

        foreach (MonoBehaviour control in monsterControlScripts)
        {
            if (control != null)
                control.enabled = !frozen;
        }
    }

    private void SetMonsterSpawnerPaused(bool paused)
    {
        if (monsterSpawner != null)
            monsterSpawner.enabled = !paused;
    }

    public IEnumerator PlayStartDialogue()
    {
        SetPlayersFrozen(true);
        SetMonstersFrozen(true);
        SetMonsterSpawnerPaused(true);

        string[] messages =
        {
            "Welcome to your new restaurant, chef!",
            "Your job is to prepare food and serve the monsters",
            "The restaurant will get busier as each day passes",
            "Get ready to cook serve and survive!"
        };

        if (dialoguePanel != null)
            dialoguePanel.SetActive(true);

        foreach (string message in messages)
            yield return ShowMessage(message);

        if (dialoguePanel != null)
            dialoguePanel.SetActive(false);

        SetPlayersFrozen(false);
        SetMonstersFrozen(false);

        // Monsters can now start spawning.
        SetMonsterSpawnerPaused(false);
    }

    public IEnumerator PlayDayEndDialogue(int completedDay)
    {
        SetPlayersFrozen(true);
        SetMonstersFrozen(true);
        SetMonsterSpawnerPaused(true);

        string[] messages;

        switch (completedDay)
        {
            case 1:
                messages = new string[]
                {
                    "Great work! Day 1 is complete!",
                    "The restaurant is getting busier.",
                    "Good news, chef! We've unlocked a new cooking station!",
                    "You can now use the new station to prepare even more food!",
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

            if (completedDay == 1 &&
                message == "Good news, chef! We've unlocked a new cooking station!")
            {
                if (cookingUI != null)
                    cookingUI.SetActive(true);
            }
        }

        if (dialoguePanel != null)
            dialoguePanel.SetActive(false);

        SetPlayersFrozen(false);
        SetMonstersFrozen(false);
        SetMonsterSpawnerPaused(false);
    }

    private IEnumerator ShowMessage(string message)
    {
        if (continueText != null)
            continueText.gameObject.SetActive(false);

        if (dialogueText != null)
            dialogueText.text = "";

        foreach (char letter in message)
        {
            if (dialogueText != null)
                dialogueText.text += letter;

            yield return new WaitForSeconds(textSpeed);
        }

        yield return new WaitForSeconds(2f);
    }
}
