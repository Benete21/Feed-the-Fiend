using UnityEngine;
using UnityEngine.UI;
using System.Collections;

public class Camera_Trail : MonoBehaviour
{
    [Header("Trail Screen Effect")]
    [SerializeField] private Image trailOverlay;

    [Header("Effect Settings")]
    [SerializeField] private float effectDuration = 1.5f;
    [SerializeField] private float fadeInDuration = 0.2f;
    [SerializeField] private float maximumAlpha = 0.2f;

    private Coroutine effectCoroutine;

    private void Awake()
    {
        if (trailOverlay != null)
        {
            Color color = trailOverlay.color;
            color.a = 0f;
            trailOverlay.color = color;
        }
    }

    public void PlayTrailEffect(Color trailColor)
    {
        if (trailOverlay == null)
            return;

        if (effectCoroutine != null)
            StopCoroutine(effectCoroutine);

        effectCoroutine = StartCoroutine(
            TrailEffectRoutine(trailColor)
        );
    }

    private IEnumerator TrailEffectRoutine(Color trailColor)
    {
        float elapsed = 0f;

        // Fade in.
        while (elapsed < fadeInDuration)
        {
            elapsed += Time.deltaTime;

            float alpha = Mathf.Lerp(
                0f,
                maximumAlpha,
                elapsed / fadeInDuration
            );

            SetOverlayColor(trailColor, alpha);

            yield return null;
        }

        SetOverlayColor(trailColor, maximumAlpha);

        // Keep the colour visible briefly.
        float holdDuration = Mathf.Max(
            0f,
            effectDuration - fadeInDuration
        );

        yield return new WaitForSeconds(holdDuration);

        // Fade out.
        elapsed = 0f;

        while (elapsed < fadeInDuration)
        {
            elapsed += Time.deltaTime;

            float alpha = Mathf.Lerp(
                maximumAlpha,
                0f,
                elapsed / fadeInDuration
            );

            SetOverlayColor(trailColor, alpha);

            yield return null;
        }

        SetOverlayColor(trailColor, 0f);

        effectCoroutine = null;
    }

    private void SetOverlayColor(Color color, float alpha)
    {
        color.a = alpha;
        trailOverlay.color = color;
    }

    private void OnDisable()
    {
        if (effectCoroutine != null)
        {
            StopCoroutine(effectCoroutine);
            effectCoroutine = null;
        }

        if (trailOverlay != null)
        {
            Color color = trailOverlay.color;
            color.a = 0f;
            trailOverlay.color = color;
        }
    }
}
