
using System.Collections;
using TMPro;
using UnityEngine;
using UnityEngine.InputSystem;

public class InteractionUI : MonoBehaviour
{
    [Header("Player Input")]
    [SerializeField] private PlayerInput playerInput;

    [Header("UI References")]
    [SerializeField] private CanvasGroup canvasGroup;
    [SerializeField] private RectTransform animatedContent;
    [SerializeField] private TMP_Text keyText;
    [SerializeField] private TMP_Text actionText;

    [Header("Animation")]
    [SerializeField] private float fadeDuration = 0.2f;
    [SerializeField] private float hiddenScale = 0.9f;

    private Coroutine animationCoroutine;
    private bool isVisible;

    private string currentAction = "";

    private void Awake()
    {
        if (canvasGroup != null)
        {
            canvasGroup.alpha = 0f;
            canvasGroup.interactable = false;
            canvasGroup.blocksRaycasts = false;
        }

        if (animatedContent != null)
            animatedContent.localScale = Vector3.one * hiddenScale;

        gameObject.SetActive(false);
    }

    public void Show(string message)
    {
        // Expected format: "E  COOK" or "A  PICK UP"
        string[] parts = message.Split(
            new char[] { ' ' },
            2,
            System.StringSplitOptions.RemoveEmptyEntries
        );

        if (parts.Length == 0)
            return;

        // Store the action text.
        // The button will be determined by this player's controller.
        currentAction = parts.Length > 1 ? parts[1] : "";

        UpdatePrompt();

        if (!gameObject.activeSelf)
            gameObject.SetActive(true);

        if (isVisible)
            return;

        isVisible = true;
        StartAnimation(1f, 1f);
    }

    private void UpdatePrompt()
    {
        if (playerInput == null)
            return;

        string button = GetInteractionButton();

        if (keyText != null)
            keyText.text = button;

        if (actionText != null)
            actionText.text = currentAction;
    }
    private string GetInteractionButton()
    {
        if (playerInput == null)
            return "?";

        string scheme = playerInput.currentControlScheme;

        // Keyboard controls
        if (scheme == "Keyboard&Mouse" || scheme == "Keyboard")
            return "E";

        // Gamepad controls
        if (scheme == "Gamepad")
        {
            foreach (InputDevice device in playerInput.devices)
            {
                if (device is Gamepad)
                {
                    // Detect PlayStation controllers by their device name.
                    string deviceName = device.displayName.ToLower();

                    if (deviceName.Contains("dualshock") ||
                        deviceName.Contains("dualsense") ||
                        deviceName.Contains("playstation"))
                    {
                        return "Cross";
                    }

                    // Default to Xbox-style controls.
                    return "A";
                }
            }
        }

        return "?";
    }


    public void Hide()
    {
        if (!isVisible)
            return;

        isVisible = false;
        StartAnimation(0f, hiddenScale);
    }

    private void StartAnimation(float targetAlpha, float targetScale)
    {
        if (animationCoroutine != null)
            StopCoroutine(animationCoroutine);

        animationCoroutine = StartCoroutine(
            AnimatePrompt(targetAlpha, targetScale)
        );
    }

    private IEnumerator AnimatePrompt(float targetAlpha, float targetScale)
    {
        float startAlpha = canvasGroup != null
            ? canvasGroup.alpha
            : 0f;

        Vector3 startScale = animatedContent != null
            ? animatedContent.localScale
            : Vector3.one;

        Vector3 endScale = Vector3.one * targetScale;

        float timer = 0f;

        while (timer < fadeDuration)
        {
            timer += Time.deltaTime;

            float t = fadeDuration > 0f
                ? Mathf.Clamp01(timer / fadeDuration)
                : 1f;

            t = t * t * (3f - 2f * t);

            if (canvasGroup != null)
                canvasGroup.alpha = Mathf.Lerp(startAlpha, targetAlpha, t);

            if (animatedContent != null)
            {
                animatedContent.localScale =
                    Vector3.Lerp(startScale, endScale, t);
            }

            yield return null;
        }

        if (canvasGroup != null)
            canvasGroup.alpha = targetAlpha;

        if (animatedContent != null)
            animatedContent.localScale = endScale;

        if (targetAlpha == 0f)
            gameObject.SetActive(false);

        animationCoroutine = null;
    }
}
