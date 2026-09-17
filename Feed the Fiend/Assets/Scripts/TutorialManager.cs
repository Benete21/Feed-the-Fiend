using System.Collections;
using TMPro;
using UnityEngine;
using UnityEngine.InputSystem;

public class TutorialManager : MonoBehaviour
{
    [Header("Characters")]
    [SerializeField] private Chef_Controls chef;
    [SerializeField] private Waiter_Controls waiter;

    [Header("Stations")]
    [SerializeField] private Ingredient_Spawner ingredientSpawner;
    [SerializeField] private PrepFoodStation prepStation;

    [Header("Customer")]
    [SerializeField] private CustomerOrder customer;

    [Header("Dialogue UI")]
    [SerializeField] private GameObject dialoguePanel;
    [SerializeField] private TMP_Text dialogueText;
    [SerializeField] private TMP_Text continueText;

    [Header("Dialogue")]
    [SerializeField] private float textSpeed = 0.03f;

    private void Start()
    {
        if (dialoguePanel != null)
            dialoguePanel.SetActive(true);

        StartCoroutine(RunTutorial());
    }

    private IEnumerator RunTutorial()
    {
        // =========================
        // WELCOME
        // =========================

        yield return Dialogue(
            "Welcome to the restaurant!"
        );

        yield return Dialogue(
            "Let's learn how to prepare and serve food."
        );


        // =========================
        // INGREDIENT
        // =========================

        yield return Dialogue(
            "First, walk over to an ingredient station."
        );

        // Hide dialogue while player moves
        yield return WaitForAction(() =>
            ingredientSpawner != null &&
            ingredientSpawner.ChefInRange
        );

        yield return Dialogue(
            "Good! You are at an ingredient station."
        );

        yield return Dialogue(
            "Press your PICKUP button to get the ingredient."
        );

        // Hide dialogue while player performs pickup
        yield return WaitForAction(() =>
            chef != null &&
            chef.GetHeldObject() != null
        );

        yield return Dialogue(
            "Perfect! The ingredient is now in your hands."
        );


        // =========================
        // PREP
        // =========================

        yield return Dialogue(
            "Now take the ingredient to the preparation station."
        );

        // Hide dialogue while player moves and places ingredient
        yield return WaitForAction(() =>
            prepStation != null &&
            prepStation.HasIngredients()
        );

        yield return Dialogue(
            "Good! The ingredient is on the preparation station."
        );

        yield return Dialogue(
            "When you have enough ingredients, press PREPARE."
        );

        // Hide dialogue while player presses prep
        yield return WaitForAction(() =>
            prepStation != null &&
            prepStation.IsPreparing()
        );

        yield return Dialogue(
            "The food is now being prepared."
        );

        yield return Dialogue(
            "Wait for the preparation bar to finish."
        );

        // Hide dialogue while food is cooking
        yield return WaitForAction(() =>
            prepStation != null &&
            !prepStation.IsPreparing()
        );


        // =========================
        // CUSTOMER
        // =========================

        yield return Dialogue(
            "Now let's take a customer's order."
        );

        yield return Dialogue(
            "Walk over to the customer and interact with them."
        );

        // Hide dialogue while player moves to customer
        yield return WaitForAction(() =>
            customer != null &&
            customer.HasOrdered()
        );

        yield return Dialogue(
            "Great! The customer has given you their order."
        );

        yield return Dialogue(
            "The order will appear on your order slip."
        );


        // =========================
        // BERSERK
        // =========================

        yield return Dialogue(
            "Be careful though. Customers won't wait forever."
        );

        yield return Dialogue(
            "The BERSERK BAR shows how much time the customer has left."
        );

        yield return Dialogue(
            "You need to prepare and deliver their food before the bar reaches zero."
        );

        yield return Dialogue(
            "If the bar empties completely, the customer will become BERSERK!"
        );

        yield return Dialogue(
            "A berserk customer will leave their table and chase you."
        );

        yield return Dialogue(
            "If they get close enough, they will attack."
        );


        // =========================
        // FINISH
        // =========================

        yield return Dialogue(
            "Now you know the basics!"
        );

        yield return Dialogue(
            "Take orders, prepare food, deliver it quickly, and don't let your customers go berserk!"
        );

        if (dialoguePanel != null)
            dialoguePanel.SetActive(false);
    }


    // =========================================================
    // NORMAL DIALOGUE
    // =========================================================

    private IEnumerator Dialogue(string message)
    {
        // Make sure dialogue is visible
        if (dialoguePanel != null)
            dialoguePanel.SetActive(true);

        if (continueText != null)
            continueText.gameObject.SetActive(false);

        dialogueText.text = "";

        // Type text
        foreach (char letter in message)
        {
            dialogueText.text += letter;
            yield return new WaitForSeconds(textSpeed);
        }

        // Show continue instruction
        if (continueText != null)
        {
            continueText.text = "Press E to continue";
            continueText.gameObject.SetActive(true);
        }

        // Wait for E
        bool continuePressed = false;

        while (!continuePressed)
        {
            if (Keyboard.current != null &&
                Keyboard.current.eKey.wasPressedThisFrame)
            {
                continuePressed = true;
            }

            yield return null;
        }

        // Hide continue text
        if (continueText != null)
            continueText.gameObject.SetActive(false);
    }


    // =========================================================
    // WAIT FOR GAMEPLAY ACTION
    // =========================================================

    private IEnumerator WaitForAction(System.Func<bool> condition)
    {
        // Hide dialogue while player performs the action
        if (dialoguePanel != null)
            dialoguePanel.SetActive(false);

        // Wait until the actual gameplay action happens
        yield return new WaitUntil(condition);

        // Show dialogue again
        if (dialoguePanel != null)
            dialoguePanel.SetActive(true);
    }
}