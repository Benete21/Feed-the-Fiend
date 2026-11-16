using System.Collections;
using UnityEngine;
using UnityEngine.UI;

public class HitFeedback : MonoBehaviour
{

    [Header("Camera Shake")]
    [SerializeField] private float shakeDuration = 0.25f;
    [SerializeField] private float shakeMagnitude = 0.15f;

    [Header("Red Screen Effect")]
    [SerializeField] private Image redOverlay;
    [SerializeField] private float redEffectDuration = 2f;
    [SerializeField] private float redAlpha = 0.35f;

    private Vector3 originalCameraPosition;

    private Coroutine hitEffectCoroutine;

    private void Awake()
    {
        originalCameraPosition = transform.localPosition;

        if (redOverlay != null)
        {
            Color color = redOverlay.color;
            color.a = 0f;
            redOverlay.color = color;
        }
    }
    public void PlayHitEffect()
    {
        transform.localPosition = originalCameraPosition;

        if (hitEffectCoroutine != null)
        {
            StopCoroutine(hitEffectCoroutine);
        }

        hitEffectCoroutine = StartCoroutine(HitEffectRoutine());
    }

    private IEnumerator HitEffectRoutine()
    {
        originalCameraPosition = transform.localPosition;

        float elapsed = 0f;

        // Show the red screen effect immediately.
        if (redOverlay != null)
        {
            Color color = redOverlay.color;
            color.a = redAlpha;
            redOverlay.color = color;
        }

        // Camera shake.
        while (elapsed < shakeDuration)
        {
            float x = Random.Range(-1f, 1f) * shakeMagnitude;
            float y = Random.Range(-1f, 1f) * shakeMagnitude;

            transform.localPosition =
                originalCameraPosition + new Vector3(x, y, 0f);

            elapsed += Time.deltaTime;

            yield return null;
        }

        transform.localPosition = originalCameraPosition;

        // Keep the red tint visible for the remainder of the effect,
        // then fade it out.
        float fadeElapsed = 0f;
        float fadeDuration = Mathf.Max(0f, redEffectDuration - shakeDuration);

        while (fadeElapsed < fadeDuration)
        {
            float alpha = Mathf.Lerp(
                redAlpha,
                0f,
                fadeElapsed / fadeDuration
            );

            if (redOverlay != null)
            {
                Color color = redOverlay.color;
                color.a = alpha;
                redOverlay.color = color;
            }

            fadeElapsed += Time.deltaTime;

            yield return null;
        }

        if (redOverlay != null)
        {
            Color color = redOverlay.color;
            color.a = 0f;
            redOverlay.color = color;
        }

        transform.localPosition = originalCameraPosition;

        hitEffectCoroutine = null;
    }

    private void OnDisable()
    {
        if (hitEffectCoroutine != null)
        {
            StopCoroutine(hitEffectCoroutine);
            hitEffectCoroutine = null;
        }

        transform.localPosition = originalCameraPosition;

        if (redOverlay != null)
        {
            Color color = redOverlay.color;
            color.a = 0f;
            redOverlay.color = color;
        }
    }
}
