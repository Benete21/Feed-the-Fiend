
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.SceneManagement;

public class TutorialManager : MonoBehaviour
{
    public enum TutorialArrowLocation
    {
        Customer,
        IngredientA,
        IngredientB,
        PrepStation,
        FoodHandoff,
        OrderDropOff
    }

    [System.Serializable]
    private class ArrowTarget
    {
        public TutorialArrowLocation location;
        public Transform target;
    }

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

    [Header("Order Slip Drop-Off")]
    [SerializeField] private OrderSlipDropOff orderSlipDropOff;

    [Header("Tutorial Arrow")]
    [SerializeField] private TutorialArrow tutorialArrow;
    [SerializeField]
    private List<ArrowTarget> arrowTargets =
        new List<ArrowTarget>();

    [Header("Dialogue UI")]
    [SerializeField] private GameObject dialoguePanel;
    [SerializeField] private TMP_Text dialogueText;
    [SerializeField] private TMP_Text continueText;

    [Header("Dialogue")]
    [SerializeField] private float textSpeed = 0.03f;

    [Header("Scene")]
    [SerializeField] private string playSceneName = "PlayScene";

    // Dialogue and character control states.
    private bool dialogueIsActive;
    private bool chefSelected;
    private bool waiterSelected;


    private void Start()
    {
        SwitchToWaiter();

        if (dialoguePanel != null)
            dialoguePanel.SetActive(true);

        StartCoroutine(RunTutorial());
    }


    // =========================================================
    // PLAYER CONTROL / DIALOGUE FREEZE
    // =========================================================

    private void SetDialogueFreeze(bool freeze)
    {
        dialogueIsActive = freeze;
        ApplyPlayerControlState();
    }

    private void ApplyPlayerControlState()
    {
        bool chefCanMove = chefSelected && !dialogueIsActive;
        bool waiterCanMove = waiterSelected && !dialogueIsActive;

        if (chef != null)
            chef.enabled = chefCanMove;

        if (waiter != null)
            waiter.enabled = waiterCanMove;

        SetPlayerInput(chefObject, chefCanMove);
        SetPlayerInput(waiterObject, waiterCanMove);
    }

    private void SetPlayerInput(GameObject character, bool enableInput)
    {
        if (character == null)
            return;

        PlayerInput input = character.GetComponent<PlayerInput>();

        if (input == null)
            input = character.GetComponentInParent<PlayerInput>();

        if (input == null)
        {
            Debug.LogWarning(
                "PlayerInput component not found on " + character.name
            );
            return;
        }

        if (enableInput)
            input.ActivateInput();
        else
            input.DeactivateInput();
    }


    // =========================================================
    // MAIN TUTORIAL
    // =========================================================

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
            "Go to the customer and press B to interact with them."
        );

        ShowArrow(TutorialArrowLocation.Customer);

        yield return WaitForTutorialOrder();

        HideArrow();

        yield return Dialogue(
            "You have taken the customer's order."
        );

        yield return Dialogue(
            "The order will appear on your order slip."
        );

        yield return Dialogue(
            "Take the order slip to the order drop-off counter."
        );

        ShowArrow(TutorialArrowLocation.OrderDropOff);

        yield return WaitForAction(() =>
            orderSlipDropOff != null &&
            orderSlipDropOff.HasOrders()
        );

        HideArrow();

        yield return Dialogue(
            "The order has been sent to the chef's inventory. You can now switch characters."
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

        GameObject objectBeforeIngredientA =
            chef != null ? chef.GetHeldObject() : null;

        ShowArrow(TutorialArrowLocation.IngredientA);

        yield return WaitForAction(() =>
            chef != null &&
            chef.GetHeldObject() != null &&
            chef.GetHeldObject() != objectBeforeIngredientA
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

        ShowArrow(TutorialArrowLocation.PrepStation);

        yield return WaitForAction(() =>
            chef != null &&
            chef.GetHeldObject() == null &&
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

        GameObject objectBeforeIngredientB =
            chef != null ? chef.GetHeldObject() : null;

        ShowArrow(TutorialArrowLocation.IngredientB);

        yield return WaitForAction(() =>
            chef != null &&
            chef.GetHeldObject() != null &&
            chef.GetHeldObject() != objectBeforeIngredientB
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

        ShowArrow(TutorialArrowLocation.PrepStation);

        yield return WaitForAction(() =>
            chef != null &&
            chef.GetHeldObject() == null &&
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
            "Press A to PREPARE the food."
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

        ShowArrow(TutorialArrowLocation.FoodHandoff);

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

        ShowArrow(TutorialArrowLocation.FoodHandoff);

        yield return WaitForAction(() =>
            waiter != null &&
            waiter.GetHeldObject() != null &&
            foodHandoffPoint != null &&
            !foodHandoffPoint.HasFood()
        );

        HideArrow();


        // =====================================================
        // DELIVER FOOD
        // =====================================================

        yield return Dialogue(
            "Now deliver the food to the customer."
        );

        ShowArrow(TutorialArrowLocation.Customer);

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

        chefSelected = true;
        waiterSelected = false;

        ApplyPlayerControlState();
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

        chefSelected = false;
        waiterSelected = true;

        ApplyPlayerControlState();
    }


    // =========================================================
    // ARROW
    // =========================================================

    private void ShowArrow(TutorialArrowLocation location)
    {
        if (tutorialArrow == null)
            return;

        Transform target = null;

        foreach (ArrowTarget arrowTarget in arrowTargets)
        {
            if (arrowTarget != null &&
                arrowTarget.location == location)
            {
                target = arrowTarget.target;
                break;
            }
        }

        if (target == null)
        {
            Debug.LogWarning(
                $"Tutorial arrow target '{location}' is not assigned in the TutorialManager list.",
                this
            );

            tutorialArrow.HideArrow();
            return;
        }

        tutorialArrow.ShowArrow(target);
    }


    private void HideArrow()
    {
        if (tutorialArrow != null)
            tutorialArrow.HideArrow();
    }


    // =========================================================
    // DIALOGUE
    // =========================================================

    private IEnumerator Dialogue(string message)
    {
        // Freeze both characters before showing dialogue.
        SetDialogueFreeze(true);

        if (dialoguePanel != null)
            dialoguePanel.SetActive(true);

        if (continueText != null)
            continueText.gameObject.SetActive(false);

        dialogueText.text = "";

        // Type the dialogue.
        foreach (char letter in message)
        {
            dialogueText.text += letter;
            yield return new WaitForSeconds(textSpeed);
        }

        // Show controller instruction.
        if (continueText != null)
        {
            continueText.text = "Press X to continue";
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

        // Unfreeze the currently selected character.
        SetDialogueFreeze(false);
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
            orderTaken = true;

        while (!orderTaken)
            yield return null;

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
            yield return null;

        tutorialCustomer.OnFoodDelivered -= FoodDelivered;

        if (dialoguePanel != null)
            dialoguePanel.SetActive(true);
    }
}
