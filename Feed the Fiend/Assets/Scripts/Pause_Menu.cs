
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.UI;

public class Pause_Menu : MonoBehaviour
{
    public static Pause_Menu Instance;

    [Header("UI")]
    [SerializeField] private GameObject pausePanel;
    [SerializeField] private Button[] menuButtons;

    [Header("Players")]
    [SerializeField] private PlayerInput[] players;

    private PlayerInput requestingPlayer;
    private InputActionMap gameplayMap;

    private string[] previousMaps;
    private bool[] werePlayersActive;

    private int selectedButton = 0;
    private float nextNavigationTime;
    [SerializeField] private float navigationDelay = 0.2f;

    private bool isPaused;

    private InputAction navigateAction;
    private InputAction submitAction;
    private InputAction cancelAction;

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;

        if (pausePanel != null)
            pausePanel.SetActive(false);
    }

    public void TogglePause(PlayerInput requester)
    {
        if (isPaused)
        {
            // Only the player who opened the menu can close it.
            if (requester == requestingPlayer)
                ResumeGame();

            return;
        }

        PauseGame(requester);
    }

    private void PauseGame(PlayerInput requester)
    {
        if (requester == null)
            return;

        requestingPlayer = requester;

        // Use the existing gameplay Action Map.
        gameplayMap = requester.currentActionMap;

        if (gameplayMap == null)
        {
            Debug.LogError("Player has no active Action Map!");
            return;
        }

        navigateAction = gameplayMap.FindAction("Navigate", false);
        submitAction = gameplayMap.FindAction("Submit", false);
        cancelAction = gameplayMap.FindAction("Cancel", false);

        if (navigateAction == null || submitAction == null || cancelAction == null)
        {
            Debug.LogError(
                "Navigate, Submit, or Cancel action is missing from the existing Action Map!"
            );
            return;
        }

        previousMaps = new string[players.Length];
        werePlayersActive = new bool[players.Length];

        for (int i = 0; i < players.Length; i++)
        {
            if (players[i] == null)
                continue;

            previousMaps[i] = players[i].currentActionMap != null
                ? players[i].currentActionMap.name
                : "";

            werePlayersActive[i] = players[i].inputIsActive;

            // Disable everyone except the player who paused.
            if (players[i] != requester)
                players[i].DeactivateInput();
        }

        isPaused = true;
        Time.timeScale = 0f;

        if (pausePanel != null)
            pausePanel.SetActive(true);

        selectedButton = 0;
        SelectButton();

        navigateAction.performed += OnNavigate;
        submitAction.performed += OnSubmit;
        cancelAction.performed += OnCancel;
    }

    private void OnNavigate(InputAction.CallbackContext context)
    {
        if (Time.unscaledTime < nextNavigationTime)
            return;

        Vector2 input = context.ReadValue<Vector2>();

        if (input.y > 0.5f)
        {
            selectedButton--;
            nextNavigationTime = Time.unscaledTime + navigationDelay;
            SelectButton();
        }
        else if (input.y < -0.5f)
        {
            selectedButton++;
            nextNavigationTime = Time.unscaledTime + navigationDelay;
            SelectButton();
        }
    }

    private void OnSubmit(InputAction.CallbackContext context)
    {
        if (menuButtons == null || menuButtons.Length == 0)
            return;

        if (selectedButton >= 0 &&
            selectedButton < menuButtons.Length &&
            menuButtons[selectedButton] != null &&
            menuButtons[selectedButton].interactable)
        {
            menuButtons[selectedButton].onClick.Invoke();
        }
    }

    private void OnCancel(InputAction.CallbackContext context)
    {
        ResumeGame();
    }

    private void SelectButton()
    {
        if (menuButtons == null || menuButtons.Length == 0)
            return;

        for (int i = 0; i < menuButtons.Length; i++)
        {
            selectedButton = (selectedButton + menuButtons.Length)
                % menuButtons.Length;

            if (menuButtons[selectedButton] != null &&
                menuButtons[selectedButton].interactable)
            {
                menuButtons[selectedButton].Select();
                return;
            }

            selectedButton++;
        }
    }

    public void ResumeGame()
    {
        if (!isPaused)
            return;

        if (navigateAction != null)
            navigateAction.performed -= OnNavigate;

        if (submitAction != null)
            submitAction.performed -= OnSubmit;

        if (cancelAction != null)
            cancelAction.performed -= OnCancel;

        if (pausePanel != null)
            pausePanel.SetActive(false);

        Time.timeScale = 1f;

        for (int i = 0; i < players.Length; i++)
        {
            if (players[i] == null)
                continue;

            if (players[i] != requestingPlayer && werePlayersActive[i])
            {
                players[i].ActivateInput();

                if (!string.IsNullOrEmpty(previousMaps[i]))
                    players[i].SwitchCurrentActionMap(previousMaps[i]);
            }
        }

        isPaused = false;
        requestingPlayer = null;
    }

    private void OnDestroy()
    {
        if (Instance == this)
        {
            Time.timeScale = 1f;
            Instance = null;
        }
    }
}
