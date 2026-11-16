
using System.Collections;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using UnityEngine.EventSystems;

public class Day_Dialogou : MonoBehaviour
{
    [Header("Dialogue UI")]
    [SerializeField] private GameObject dialoguePanel;
    [SerializeField] private TMP_Text dialogueText;
    [SerializeField] private TMP_Text continueText;

    [Header("Cooking Station")]
    [SerializeField] private GameObject cookingUI;

    [Header("Final Day UI")]
    [SerializeField] private GameObject endingPanel;
    [SerializeField] private Button endingFirstButton;

    [Header("Scene Settings")]
    [SerializeField] private string mainMenuSceneName = "Langa Main Menu";

    [Header("Player Controls")]
    [SerializeField] private MonoBehaviour[] playerControlScripts;

    [Header("Monster Controls")]
    [SerializeField] private MonoBehaviour monsterSpawner;
    [SerializeField] private MonoBehaviour[] monsterControlScripts;

    [Header("Settings")]
    [SerializeField] private float textSpeed = 0.03f;

    private bool finalDayFinished;

    private void Awake()
    {
        // Prevent monsters spawning before the opening dialogue.
        if (monsterSpawner != null)
            monsterSpawner.enabled = false;
    }

    private void Start()
    {
        if (dialoguePanel != null)
            dialoguePanel.SetActive(false);

        // Cooking station UI starts hidden.
        if (cookingUI != null)
            cookingUI.SetActive(false);

        if (endingPanel != null)
            endingPanel.SetActive(false);

        StartCoroutine(PlayStartDialogue());
    }

    private void Update()
    {
        // Press F to test the end of Day 1.
        if (Input.GetKeyDown(KeyCode.F))
        {
            StopAllCoroutines();
            StartCoroutine(PlayDayEndDialogue(1));
        }

        // Press G to test the end of Day 2.
        if (Input.GetKeyDown(KeyCode.G))
        {
            StopAllCoroutines();
            StartCoroutine(PlayDayEndDialogue(2));
        }
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

    // =========================================================
    // START OF DAY 1 DIALOGUE
    // =========================================================


    public IEnumerator PlayStartDialogue()
    {
        // Freeze gameplay while the opening sequence plays.
        SetPlayersFrozen(true);
        SetMonstersFrozen(true);
        SetMonsterSpawnerPaused(true);

        // Show the cooking station UI first.
        if (cookingUI != null)
        {
            cookingUI.SetActive(true);
            Debug.Log("Cooking station UI prompt is now visible!");
        }
        else
        {
            Debug.LogWarning("Cooking UI is missing! Assign it in the Inspector.");
        }

        // Give the UI a moment to appear before dialogue begins.
        yield return new WaitForSeconds(1f);

        string[] messages =
        {
        "We've finally opened up the restaurant!",
        "We've also got a new fancy cooking station ready to use! (Fire Icon)",
        "Let's see what sort of whacky customers we'll get today!"
    };

        if (dialoguePanel != null)
            dialoguePanel.SetActive(true);

        foreach (string message in messages)
        {
            yield return ShowMessage(message);
        }

        if (dialoguePanel != null)
            dialoguePanel.SetActive(false);

        // Resume gameplay after the dialogue finishes.
        SetPlayersFrozen(false);
        SetMonstersFrozen(false);
        SetMonsterSpawnerPaused(false);
    }



    // =========================================================
    // END OF DAY DIALOGUE
    // =========================================================

    public IEnumerator PlayDayEndDialogue(int completedDay)
    {
        if (finalDayFinished)
            yield break;

        SetPlayersFrozen(true);
        SetMonstersFrozen(true);
        SetMonsterSpawnerPaused(true);

        string[] messages;

        switch (completedDay)
        {
            case 1:
                messages = new string[]
                {
                    "Finally, we finished the first day!",
                    "Running a restaurant is much harder than I thought",
                    "Lets get ready for Day 2"
                };
                break;

            case 2:
                messages = new string[]
                {
                    "Amazing work! We survived Day 2!",
                    "I think I've had my fair share of running a restaurant",

                };
                break;

            default:
                messages = new string[]
                {
                    "Day " + completedDay + " is complete!"
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

        // DAY 2 IS THE FINAL DAY.
        if (completedDay == 2)
        {
            finalDayFinished = true;

            SetPlayersFrozen(true);
            SetMonstersFrozen(true);
            SetMonsterSpawnerPaused(true);

            if (endingPanel != null)
                endingPanel.SetActive(true);

            // Highlight the first ending button for controller navigation.
            if (endingFirstButton != null)
            {
                EventSystem.current?.SetSelectedGameObject(null);
                EventSystem.current?.SetSelectedGameObject(
                    endingFirstButton.gameObject
                );
            }

            yield break;
        }

        // Unfreeze gameplay after Day 1.
        SetPlayersFrozen(false);
        SetMonstersFrozen(false);
        SetMonsterSpawnerPaused(false);
    }

    // =========================================================
    // TYPEWRITER DIALOGUE
    // =========================================================

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

    // =========================================================
    // RESTART THE RESTAURANT
    // =========================================================

    public void RestartGame()
    {
        Time.timeScale = 1f;

        SceneManager.LoadScene(
            SceneManager.GetActiveScene().buildIndex
        );
    }

    // =========================================================
    // RETURN TO MAIN MENU
    // =========================================================

    public void ReturnToMainMenu()
    {
        Time.timeScale = 1f;

        SceneManager.LoadScene(mainMenuSceneName);
    }

    // =========================================================
    // QUIT THE GAME
    // =========================================================

    public void QuitGame()
    {
        Time.timeScale = 1f;

        Debug.Log("Quitting game...");

        Application.Quit();

#if UNITY_EDITOR
        UnityEditor.EditorApplication.isPlaying = false;
#endif
    }
}
