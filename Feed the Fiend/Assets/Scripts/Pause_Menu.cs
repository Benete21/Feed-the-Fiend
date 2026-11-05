using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class Pause_Menu : MonoBehaviour
{

    public static Pause_Menu Instance;

    [Header("Pause Menu")]
    [SerializeField] private GameObject pausePanel;
    [SerializeField] private Button[] menuButtons;

    [Header("Players")]
    [SerializeField] private PlayerInput[] players;

    private PlayerInput pausingPlayer;
    private string previousPlayerMap;

    private Dictionary<PlayerInput, string> previousMaps =
        new Dictionary<PlayerInput, string>();

    private InputAction navigateAction;
    private InputAction submitAction;
    private InputAction cancelAction;

    private int selectedIndex;
    private float nextNavigationTime;

    [SerializeField] private float navigationDelay = 0.2f;

    public bool IsPaused { get; private set; }

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
        if (requester == null)
            return;

        // Only the player who opened the menu can close it.
        if (IsPaused)
        {
            if (requester == pausingPlayer)
                ResumeGame();

            return;
        }

        PauseGame(requester);
    }

    private void PauseGame(PlayerInput requester)
    {
        if (requester.actions.FindActionMap("UI", false) == null)
        {
            Debug.LogError("UI Action Map is missing!");
            return;
        }

        navigateAction = requester.actions.FindAction("UI/Navigate", false);
        submitAction = requester.actions.FindAction("UI/Submit", false);
        cancelAction = requester.actions.FindAction("UI/Cancel", false);

        if (navigateAction == null ||
            submitAction == null ||
            cancelAction == null)
        {
            Debug.LogError("Check Navigate, Submit and Cancel actions!");
            return;
        }

        pausingPlayer = requester;
        previousMaps.Clear();

        foreach (PlayerInput player in players)
        {
            if (player == null)
                continue;

            string currentMap = player.currentActionMap != null
                ? player.currentActionMap.name
                : "";

            previousMaps[player] = currentMap;

            if (player == requester)
            {
                previousPlayerMap = currentMap;
                player.SwitchCurrentActionMap("UI");
            }
            else
            {
                // Disable every other player's input.
                player.DeactivateInput();
            }
        }

        Time.timeScale = 0f;
        IsPaused = true;

        if (pausePanel != null)
            pausePanel.SetActive(true);

        // Listen only to the requesting player's UI actions.
        navigateAction.performed += OnNavigate;
        submitAction.performed += OnSubmit;
        cancelAction.performed += OnCancel;

        selectedIndex = 0;
        SelectButton(selectedIndex);
    }

    private void OnNavigate(InputAction.CallbackContext context)
    {
        if (Time.unscaledTime < nextNavigationTime)
            return;

        Vector2 direction = context.ReadValue<Vector2>();

        if (direction.y > 0.5f)
        {
            selectedIndex--;
        }
        else if (direction.y < -0.5f)
        {
            selectedIndex++;
        }
        else
        {
            return;
        }

        if (menuButtons == null || menuButtons.Length == 0)
            return;

        selectedIndex = (selectedIndex + menuButtons.Length)
            % menuButtons.Length;

        nextNavigationTime = Time.unscaledTime + navigationDelay;

        SelectButton(selectedIndex);
    }

    private void SelectButton(int index)
    {
        if (menuButtons == null || menuButtons.Length == 0)
            return;

        for (int i = 0; i < menuButtons.Length; i++)
        {
            int buttonIndex = (index + i) % menuButtons.Length;

            if (menuButtons[buttonIndex] != null &&
                menuButtons[buttonIndex].interactable)
            {
                selectedIndex = buttonIndex;
                menuButtons[buttonIndex].Select();
                return;
            }
        }
    }

    private void OnSubmit(InputAction.CallbackContext context)
    {
        if (menuButtons == null ||
            menuButtons.Length == 0)
            return;

        if (selectedIndex < 0 ||
            selectedIndex >= menuButtons.Length)
            return;

        Button selectedButton = menuButtons[selectedIndex];

        if (selectedButton != null &&
            selectedButton.interactable)
        {
            selectedButton.onClick.Invoke();
        }
    }

    private void OnCancel(InputAction.CallbackContext context)
    {
        ResumeGame();
    }

    public void ResumeGame()
    {
        if (!IsPaused)
            return;

        // Stop listening to the UI actions.
        if (navigateAction != null)
            navigateAction.performed -= OnNavigate;

        if (submitAction != null)
            submitAction.performed -= OnSubmit;

        if (cancelAction != null)
            cancelAction.performed -= OnCancel;

        if (pausePanel != null)
            pausePanel.SetActive(false);

        Time.timeScale = 1f;

        // Restore the original action maps.
        foreach (var entry in previousMaps)
        {
            PlayerInput player = entry.Key;

            if (player == null)
                continue;

            if (player == pausingPlayer)
            {
                player.SwitchCurrentActionMap(previousPlayerMap);
            }
            else
            {
                player.ActivateInput();

                if (!string.IsNullOrEmpty(entry.Value))
                    player.SwitchCurrentActionMap(entry.Value);
            }
        }

        previousMaps.Clear();

        pausingPlayer = null;
        navigateAction = null;
        submitAction = null;
        cancelAction = null;

        IsPaused = false;
    }

    private void OnDestroy()
    {
        if (Instance == this)
        {
            if (IsPaused)
            {
                Time.timeScale = 1f;
            }

            Instance = null;
        }
    }
}

