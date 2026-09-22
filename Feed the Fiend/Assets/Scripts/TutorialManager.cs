using System.Collections;
using TMPro;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.SceneManagement;

public class TutorialManager : MonoBehaviour
{
    [Header("Characters")]
    [SerializeField] private Chef_Controls chef;
    [SerializeField] private Waiter_Controls waiter;

    [Header("Character GameObjects")]
    [SerializeField] private GameObject chefObject;
    [SerializeField] private GameObject waiterObject;

    [Header("Stations")]
    [SerializeField] private Ingredient_Spawner ingredientSpawnerA;
    [SerializeField] private Ingredient_Spawner ingredientSpawnerB;
    [SerializeField] private PrepFoodStation prepStation;

    [Header("Food Handoff")]
    [SerializeField] private FoodSnapPoint foodHandoffPoint;

    [Header("Customer")]
    [SerializeField] private TutCustomer tutorialCustomer;

    [Header("Tutorial Arrow")]
    [SerializeField] private TutorialArrow tutorialArrow;

    [Header("Dialogue UI")]
    [SerializeField] private GameObject dialoguePanel;
    [SerializeField] private TMP_Text dialogueText;
    [SerializeField] private TMP_Text continueText;

    [Header("Dialogue")]
    [SerializeField] private float textSpeed = 0.03f;

    [Header("Scene")]
    [SerializeField] private string playSceneName = "PlayScene";


    private void Start()
    {
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
    "Go to the customer and press A to interact with them."
);
        /* yield return Dialogue(
             "Follow the arrow to the customer."
         );*/

        ShowArrowTo(tutorialCustomer.transform);

        yield return WaitForTutorialOrder();

        HideArrow();

        yield return Dialogue(
            "You have taken the customer's order."
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
        // INGREDIENT A
        // =====================================================

        yield return Dialogue(
            "First, collect Ingredient A."
        );

        yield return Dialogue(
    "Go to the Blue ingredient station."
);

        ShowArrowTo(ingredientSpawnerA.transform);

        yield return WaitForAction(() =>
            chef != null &&
            chef.GetHeldObject() != null
        );

        HideArrow();

        yield return Dialogue(
            "You picked up Ingredient A."
        );


        // =====================================================
        // PLACE INGREDIENT A
        // =====================================================

        yield return Dialogue(
            "Take Ingredient A to the preparation station."
        );

        ShowArrowTo(prepStation.transform);

        yield return WaitForAction(() =>
            prepStation != null &&
            prepStation.HasIngredients()
        );

        HideArrow();

        yield return Dialogue(
            "Ingredient A has been placed."
        );


        // =====================================================
        // INGREDIENT B
        // =====================================================

        yield return Dialogue(
            "Now collect Ingredient B."
        );
        yield return Dialogue(
    "Go to the Yellow ingredient station."
);

        ShowArrowTo(ingredientSpawnerB.transform);

        yield return WaitForAction(() =>
            chef != null &&
            chef.GetHeldObject() != null
        );

        HideArrow();

        yield return Dialogue(
            "Excellent! You have Ingredient B."
        );


        // =====================================================
        // PLACE INGREDIENT B
        // =====================================================

        yield return Dialogue(
            "Bring Ingredient B to the preparation station."
        );

        ShowArrowTo(prepStation.transform);

        yield return WaitForAction(() =>
            prepStation != null &&
            prepStation.HasRequiredTutorialIngredients()
        );

        HideArrow();


        // =====================================================
        // PREPARE
        // =====================================================

        yield return Dialogue(
            "Both ingredients are ready."
        );

        yield return Dialogue(
            "Press B to PREPARE the food."
        );

        yield return WaitForAction(() =>
            prepStation != null &&
            prepStation.IsPreparing()
        );

        yield return Dialogue(
            "The food is now being prepared."
        );

        yield return Dialogue(
            "Wait for the preparation to finish."
        );

        yield return WaitForAction(() =>
            prepStation != null &&
            !prepStation.IsPreparing() &&
            prepStation.HasFinishedTutorialFood()
        );


        // =====================================================
        // FOOD HANDOFF
        // =====================================================

        yield return Dialogue(
            "The food is ready!"
        );

        yield return Dialogue(
            "Place the finished food in the handoff area."
        );

        ShowArrowTo(foodHandoffPoint.transform);

        yield return WaitForAction(() =>
            foodHandoffPoint != null &&
            foodHandoffPoint.HasFood()
        );

        HideArrow();

        yield return Dialogue(
            "The waiter can now access the food."
        );


        // =====================================================
        // SWITCH TO WAITER
        // =====================================================

        yield return Dialogue(
            "Let's switch back to the waiter."
        );

        SwitchToWaiter();

        yield return Dialogue(
            "You are now controlling the waiter."
        );

        yield return Dialogue(
            "Collect the prepared food."
        );

        ShowArrowTo(foodHandoffPoint.transform);

        yield return WaitForAction(() =>
            waiter != null &&
            waiter.GetHeldObject() != null
        );

        HideArrow();


        // =====================================================
        // DELIVER FOOD
        // =====================================================

        yield return Dialogue(
            "Now deliver the food to the customer."
        );

        ShowArrowTo(tutorialCustomer.transform);

        yield return WaitForTutorialDelivery();

        HideArrow();


        // =====================================================
        // FINISH
        // =====================================================


        yield return Dialogue(
            "You now know how to take orders, prepare food, and serve customers."
        );

        yield return Dialogue(
            "Good luck running the restaurant!"
        );

        if (dialoguePanel != null)
            dialoguePanel.SetActive(false);

        yield return new WaitForSeconds(1f);

        SceneManager.LoadScene(playSceneName);
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
    // ARROW
    // =========================================================

    private void ShowArrowTo(Transform target)
    {
        if (tutorialArrow != null)
        {
            tutorialArrow.ShowArrow(target);
        }
    }


    private void HideArrow()
    {
        if (tutorialArrow != null)
        {
            tutorialArrow.HideArrow();
        }
    }


    // =========================================================
    // DIALOGUE
    // =========================================================

    private IEnumerator Dialogue(string message)
    {
        if (dialoguePanel != null)
            dialoguePanel.SetActive(true);

        if (continueText != null)
            continueText.gameObject.SetActive(false);

        dialogueText.text = "";

        // Type the dialogue
        foreach (char letter in message)
        {
            dialogueText.text += letter;
            yield return new WaitForSeconds(textSpeed);
        }

        // Show controller instruction
        if (continueText != null)
        {
            continueText.text = "Press Y to continue";
            continueText.gameObject.SetActive(true);
        }

        bool continuePressed = false;

        while (!continuePressed)
        {

            if (Gamepad.current != null &&
                Gamepad.current.buttonWest.wasPressedThisFrame)
            {
                continuePressed = true;
            }

            yield return null;
        }

        if (continueText != null)
            continueText.gameObject.SetActive(false);

}



    // =========================================================
    // WAIT FOR ACTION
    // =========================================================

    private IEnumerator WaitForAction(System.Func<bool> condition)
    {
        if (dialoguePanel != null)
            dialoguePanel.SetActive(false);

        yield return new WaitUntil(condition);

        if (dialoguePanel != null)
            dialoguePanel.SetActive(true);
    }


    // =========================================================
    // WAIT FOR ORDER
    // =========================================================

    private IEnumerator WaitForTutorialOrder()
    {
        if (dialoguePanel != null)
            dialoguePanel.SetActive(false);

        bool orderTaken = false;

        void OrderFinished()
        {
            orderTaken = true;
        }

        tutorialCustomer.OnOrderTaken += OrderFinished;

        if (tutorialCustomer.HasOrdered())
        {
            orderTaken = true;
        }

        while (!orderTaken)
        {
            yield return null;
        }

        tutorialCustomer.OnOrderTaken -= OrderFinished;

        if (dialoguePanel != null)
            dialoguePanel.SetActive(true);
    }


    // =========================================================
    // WAIT FOR DELIVERY
    // =========================================================

    private IEnumerator WaitForTutorialDelivery()
    {
        if (dialoguePanel != null)
            dialoguePanel.SetActive(false);

        bool delivered = false;

        void FoodDelivered()
        {
            delivered = true;
        }

        tutorialCustomer.OnFoodDelivered += FoodDelivered;

        while (!delivered)
        {
            yield return null;
        }

        tutorialCustomer.OnFoodDelivered -= FoodDelivered;

        if (dialoguePanel != null)
            dialoguePanel.SetActive(true);
    }
}