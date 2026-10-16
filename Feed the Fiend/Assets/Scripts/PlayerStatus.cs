using System.Collections;
using UnityEngine;

public class PlayerStatus : MonoBehaviour
{
    [Header("Slow Effect")]
    [Range(0.1f, 1f)]
    public float slowMultiplier = 0.5f;

    public float SlowMultiplier { get; private set; } = 1f;

    private Coroutine slowCoroutine;

    public void ApplySlow(float duration, float multiplier)
    {
        if (slowCoroutine != null)
            StopCoroutine(slowCoroutine);

        slowCoroutine = StartCoroutine(
            SlowRoutine(duration, multiplier)
        );
    }

    private IEnumerator SlowRoutine(float duration, float multiplier)
    {
        SlowMultiplier = Mathf.Clamp(multiplier, 0.1f, 1f);

        yield return new WaitForSeconds(duration);

        SlowMultiplier = 1f;
        slowCoroutine = null;
    }
}

