
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using UnityEngine.EventSystems;

public class Pause_Menu : MonoBehaviour
{
    public static Pause_Menu Instance;

    [Header("UI")]
    [SerializeField] private GameObject pausePanel;
    [SerializeField] private Button[] menuButtons;

    [Header("Scene Settings")]
    [SerializeField] private string mainMenuSceneName = "Langa Main Menu";

    [Header("Players")]
    [SerializeField] private PlayerInput[] players;

    [Header("Navigation")]
    [SerializeField] private float navigationDelay = 0.2f;

    private PlayerInput requestingPlayer;
    private string[] previousMaps;
    private bool[] werePlayersActive;

    private int selectedButton = -1;
    private float nextNavigationTime;

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

        ConfigureButtons();

        if (pausePanel != null)
            pausePanel.SetActive(false);
    }

    private void ConfigureButtons()
    {
        if (menuButtons == null)
            return;

        foreach (Button button in menuButtons)
        {
            if (button == null)
                continue;

            Navigation navigation = button.navigation;
            navigation.mode = Navigation.Mode.None;
            button.navigation = navigation;
        }
    }

    public void TogglePause(PlayerInput requester)
    {
        if (isPaused)
        {
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

        InputActionMap gameplayMap = requester.currentActionMap;

        if (gameplayMap == null)
        {
            Debug.LogError("Player has no active Action Map!");
            requestingPlayer = null;
            return;
        }

        navigateAction = gameplayMap.FindAction("Navigate", false);
        submitAction = gameplayMap.FindAction("Submit", false);
        cancelAction = gameplayMap.FindAction("Cancel", false);

        if (navigateAction == null || submitAction == null || cancelAction == null)
        {
            Debug.LogError("Navigate, Submit or Cancel action is missing!");
            requestingPlayer = null;
            return;
        }

        if (players == null)
            players = new PlayerInput[0];

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

            if (players[i] != requester)
                players[i].DeactivateInput();
        }

        isPaused = true;
        Time.timeScale = 0f;

        if (pausePanel != null)
            pausePanel.SetActive(true);

        ConfigureButtons();

        // Start with the first available button.
        selectedButton = -1;
        MoveSelection(1);

        nextNavigationTime = Time.unscaledTime + navigationDelay;

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
            MoveSelection(-1);
            nextNavigationTime = Time.unscaledTime + navigationDelay;
        }
        else if (input.y < -0.5f)
        {
            MoveSelection(1);
            nextNavigationTime = Time.unscaledTime + navigationDelay;
        }
    }

    private void MoveSelection(int direction)
    {
        if (menuButtons == null || menuButtons.Length == 0)
            return;

        for (int i = 0; i < menuButtons.Length; i++)
        {
            selectedButton += direction;

            if (selectedButton >= menuButtons.Length)
                selectedButton = 0;

            if (selectedButton < 0)
                selectedButton = menuButtons.Length - 1;

            Button button = menuButtons[selectedButton];

            if (button != null && button.interactable && button.gameObject.activeInHierarchy)
            {
                SetSelectedButton(button);
                return;
            }
        }

        selectedButton = -1;
    }

    private void SetSelectedButton(Button button)
    {
        if (button == null)
            return;

        EventSystem eventSystem = EventSystem.current;

        if (eventSystem == null)
        {
            Debug.LogError("No EventSystem found in the scene!");
            return;
        }

        eventSystem.SetSelectedGameObject(null);
        eventSystem.SetSelectedGameObject(button.gameObject);
    }

    private void OnSubmit(InputAction.CallbackContext context)
    {
        if (!isPaused || menuButtons == null)
            return;

        if (selectedButton < 0 || selectedButton >= menuButtons.Length)
            return;

        Button button = menuButtons[selectedButton];

        if (button != null && button.interactable && button.gameObject.activeInHierarchy)
            button.onClick.Invoke();
    }

    private void OnCancel(InputAction.CallbackContext context)
    {
        ResumeGame();
    }

    public void ResumeGame()
    {
        if (!isPaused)
            return;

        RemoveInputListeners();

        if (pausePanel != null)
            pausePanel.SetActive(false);

        Time.timeScale = 1f;

        RestorePlayers();

        isPaused = false;
        requestingPlayer = null;
        selectedButton = -1;
    }

    public void ReturnToMainMenu()
    {
        RemoveInputListeners();

        Time.timeScale = 1f;

        isPaused = false;
        requestingPlayer = null;

        SceneManager.LoadScene(mainMenuSceneName);
    }

    public void QuitGame()
    {
        RemoveInputListeners();

        Time.timeScale = 1f;

        Debug.Log("Quitting game...");

        Application.Quit();

#if UNITY_EDITOR
        UnityEditor.EditorApplication.isPlaying = false;
#endif
    }

    private void RemoveInputListeners()
    {
        if (navigateAction != null)
            navigateAction.performed -= OnNavigate;

        if (submitAction != null)
            submitAction.performed -= OnSubmit;

        if (cancelAction != null)
            cancelAction.performed -= OnCancel;
    }

    private void RestorePlayers()
    {
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
    }

    private void OnDestroy()
    {
        RemoveInputListeners();

        if (Instance == this)
        {
            Time.timeScale = 1f;
            Instance = null;
        }
    }
}
