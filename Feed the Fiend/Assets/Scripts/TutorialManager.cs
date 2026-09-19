using System.Collections;
using TMPro;
using UnityEngine;
using UnityEngine.InputSystem;

public class TutorialManager : MonoBehaviour
{
    [Header("Characters")]
    [SerializeField] private Chef_Controls chef;
    [SerializeField] private Waiter_Controls waiter;

    [Header("Character GameObjects")]
    [SerializeField] private GameObject chefObject;
    [SerializeField] private GameObject waiterObject;

    [Header("Stations")]
    [SerializeField] private Ingredient_Spawner ingredientSpawner;
    [SerializeField] private PrepFoodStation prepStation;

    [Header("Customer")]
    [SerializeField] private TutCustomer tutorialCustomer;


    [Header("Dialogue UI")]
    [SerializeField] private GameObject dialoguePanel;
    [SerializeField] private TMP_Text dialogueText;
    [SerializeField] private TMP_Text continueText;

    [Header("Dialogue")]
    [SerializeField] private float textSpeed = 0.03f;


    private void Start()
    {
        // Start the tutorial as the waiter
        SwitchToWaiter();

        if (dialoguePanel != null)
            dialoguePanel.SetActive(true);

        StartCoroutine(RunTutorial());
    }


    private IEnumerator RunTutorial()
    {
        // =====================================================
        // INTRO
        // =====================================================

        yield return Dialogue(
            "Welcome to the restaurant!"
        );

        yield return Dialogue(
            "Let's learn how to take orders, prepare food, and serve customers."
        );


        // =====================================================
        // WAITER - TAKE ORDER
        // =====================================================

        yield return Dialogue(
            "You are now controlling the waiter."
        );

        yield return Dialogue(
            "Your first job is to take the customer's order."
        );

        yield return Dialogue(
            "Walk over to the customer and interact with them."
        );


        while (tutorialCustomer == null)
        {
            yield return null;
        }

        yield return WaitForTutorialOrder();


        yield return Dialogue(
            "Great! You have taken the customer's order."
        );



        yield return Dialogue(
            "The order will appear on your order slip."
        );



        // =====================================================
        // SWITCH TO CHEF
        // =====================================================

        yield return Dialogue(
            "Now we need to prepare the customer's food."
        );

        yield return Dialogue(
            "Let's switch to the chef."
        );

        SwitchToChef();

        yield return Dialogue(
            "You are now controlling the chef."
        );


        // =====================================================
        // CHEF - INGREDIENT
        // =====================================================

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

        // Hide dialogue while player picks up ingredient
        yield return WaitForAction(() =>
            chef != null &&
            chef.GetHeldObject() != null
        );

        yield return Dialogue(
            "Perfect! The ingredient is now in your hands."
        );


        // =====================================================
        // CHEF - PREPARATION
        // =====================================================

        yield return Dialogue(
            "Now take the ingredient to the preparation station."
        );

        // Hide dialogue while player places ingredient
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

        // Hide dialogue while player presses prepare
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

        yield return Dialogue(
            "Excellent! The food is ready."
        );


        // =====================================================
        // SWITCH BACK TO WAITER
        // =====================================================

        yield return Dialogue(
            "Now it's time to serve the customer."
        );

        yield return Dialogue(
            "Let's switch back to the waiter."
        );

        SwitchToWaiter();

        yield return Dialogue(
            "You are now controlling the waiter."
        );

        yield return Dialogue(
            "Take the prepared food and deliver it to the customer."
        );


        // =====================================================
        // BERSERK
        // =====================================================

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


        // =====================================================
        // FINISH
        // =====================================================

        yield return Dialogue(
            "Now you know the basics!"
        );

        yield return Dialogue(
            "Take orders with the waiter, prepare food with the chef, and serve the customer before they go berserk!"
        );

        if (dialoguePanel != null)
            dialoguePanel.SetActive(false);
    }


    // =========================================================
    // SWITCH TO CHEF
    // =========================================================

    private void SwitchToChef()
    {
        if (waiterObject != null)
            waiterObject.SetActive(false);

        if (chefObject != null)
            chefObject.SetActive(true);

        if (waiter != null)
            waiter.enabled = false;

        if (chef != null)
            chef.enabled = true;
    }


    // =========================================================
    // SWITCH TO WAITER
    // =========================================================

    private void SwitchToWaiter()
    {
        if (chefObject != null)
            chefObject.SetActive(false);

        if (waiterObject != null)
            waiterObject.SetActive(true);

        if (chef != null)
            chef.enabled = false;

        if (waiter != null)
            waiter.enabled = true;
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

    private IEnumerator WaitForTutorialOrder()
    {
        // Hide dialogue while the player interacts
        if (dialoguePanel != null)
            dialoguePanel.SetActive(false);

        bool orderTaken = false;

        void OrderFinished()
        {
            Debug.Log("TutorialManager: Order received!");
            orderTaken = true;
        }

        // Subscribe to the tutorial customer's event.
        tutorialCustomer.OnOrderTaken += OrderFinished;

        // Check in case it somehow happened before we subscribed.
        if (tutorialCustomer.HasOrdered())
        {
            orderTaken = true;
        }

        // Wait until the customer finishes taking the order.
        while (!orderTaken)
        {
            yield return null;
        }

        // Stop listening.
        tutorialCustomer.OnOrderTaken -= OrderFinished;

        // Show dialogue again.
        if (dialoguePanel != null)
            dialoguePanel.SetActive(true);
    }



}
