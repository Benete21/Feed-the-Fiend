
using System.Collections;
using TMPro;
using UnityEngine;

public class InteractionUI : MonoBehaviour
{
    [Header("UI References")]
    [SerializeField] private CanvasGroup canvasGroup;
    [SerializeField] private TMP_Text promptText;

    [Header("Animation")]
    [SerializeField] private float fadeDuration = 0.2f;
    [SerializeField] private float visibleScale = 1f;
    [SerializeField] private float hiddenScale = 0.9f;

    private Coroutine animationCoroutine;
    private bool isVisible;

    private void Awake()
    {
        canvasGroup.alpha = 0f;
        canvasGroup.interactable = false;
        canvasGroup.blocksRaycasts = false;

        transform.localScale = Vector3.one * hiddenScale;
        gameObject.SetActive(false);
    }

    public void Show(string message)
    {
        promptText.text = message;

        if (!gameObject.activeSelf)
            gameObject.SetActive(true);

        if (isVisible)
            return;

        isVisible = true;
        StartAnimation(1f, visibleScale);
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
        Vector3 startScale = transform.localScale;
        Vector3 endScale = Vector3.one * targetScale;

        float timer = 0f;

        while (timer < fadeDuration)
        {
            timer += Time.deltaTime;

            float t = Mathf.Clamp01(timer / fadeDuration);

            // Smooth easing.
            t = t * t * (3f - 2f * t);

            canvasGroup.alpha = Mathf.Lerp(startAlpha, targetAlpha, t);
            transform.localScale = Vector3.Lerp(startScale, endScale, t);

            yield return null;
        }

        canvasGroup.alpha = targetAlpha;
        transform.localScale = endScale;

        if (targetAlpha == 0f)
            gameObject.SetActive(false);

        animationCoroutine = null;
    }
}
