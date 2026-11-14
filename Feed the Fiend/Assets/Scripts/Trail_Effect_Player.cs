using UnityEngine;

public class Trail_Effect_Player : MonoBehaviour
{
    [Header("Dust Effect")]
    [SerializeField] private ParticleSystem dustPrefab;
    [SerializeField] private Transform dustPoint;

    [Header("Movement Settings")]
    [SerializeField] private float movementThreshold = 0.01f;
    [SerializeField] private float dustInterval = 0.12f;
    [SerializeField] private int particlesPerPuff = 4;

    private ParticleSystem dustParticles;
    private Vector3 lastPosition;
    private float dustTimer;

    private void Start()
    {
        lastPosition = transform.position;

        if (dustPrefab != null)
        {
            dustParticles = Instantiate(dustPrefab);

            var main = dustParticles.main;
            main.simulationSpace = ParticleSystemSimulationSpace.World;

            dustParticles.Stop(true, ParticleSystemStopBehavior.StopEmittingAndClear);
        }
        else
        {
            Debug.LogWarning("Dust prefab has not been assigned!");
        }
    }

    private void LateUpdate()
    {
        if (dustParticles == null)
            return;

        float distanceMoved =
            Vector3.Distance(transform.position, lastPosition);

        bool isMoving = distanceMoved > movementThreshold;

        if (isMoving)
        {
            dustTimer += Time.deltaTime;

            if (dustTimer >= dustInterval)
            {
                CreateDust();
                dustTimer = 0f;
            }
        }
        else
        {
            dustTimer = 0f;
        }

        lastPosition = transform.position;
    }

    private void CreateDust()
    {
        Vector3 spawnPosition = dustPoint != null
            ? dustPoint.position
            : transform.position + Vector3.down * 0.8f;

        dustParticles.transform.position = spawnPosition;

        dustParticles.Emit(particlesPerPuff);
    }

    private void OnDestroy()
    {
        if (dustParticles != null)
            Destroy(dustParticles.gameObject);
    }
}
