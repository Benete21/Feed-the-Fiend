
using System.Collections;
using TMPro;
using UnityEngine;

public class InteractionUI : MonoBehaviour
{
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

    private void Awake()
    {
        canvasGroup.alpha = 0f;
        canvasGroup.interactable = false;
        canvasGroup.blocksRaycasts = false;

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

        keyText.text = parts[0];
        actionText.text = parts.Length > 1 ? parts[1] : "";

        if (!gameObject.activeSelf)
            gameObject.SetActive(true);

        if (isVisible)
            return;

        isVisible = true;
        StartAnimation(1f, 1f);
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
        float startAlpha = canvasGroup.alpha;

        Vector3 startScale = animatedContent != null
            ? animatedContent.localScale
            : Vector3.one;

        Vector3 endScale = Vector3.one * targetScale;

        float timer = 0f;

        while (timer < fadeDuration)
        {
            timer += Time.deltaTime;

            float t = Mathf.Clamp01(timer / fadeDuration);
            t = t * t * (3f - 2f * t);

            canvasGroup.alpha = Mathf.Lerp(startAlpha, targetAlpha, t);

            if (animatedContent != null)
            {
                animatedContent.localScale =
                    Vector3.Lerp(startScale, endScale, t);
            }

            yield return null;
        }

        canvasGroup.alpha = targetAlpha;

        if (animatedContent != null)
            animatedContent.localScale = endScale;

        if (targetAlpha == 0f)
            gameObject.SetActive(false);

        animationCoroutine = null;
    }
}
