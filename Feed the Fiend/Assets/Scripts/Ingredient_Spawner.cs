using System.Collections;
using UnityEngine;
using UnityEngine.UI;

public class Ingredient_Spawner : MonoBehaviour
{
    [Header("Ingredient")]
    [SerializeField] private GameObject ingredientPrefab;
    [SerializeField] private Transform spawnPoint;

    [Header("Timer")]
    [SerializeField] private float respawnTime = 5f;

    [Header("UI")]
    [SerializeField] private GameObject progressBarObject;
    [SerializeField] private Slider progressBar;

    private GameObject currentIngredient;
    private bool isRespawning;

    private void OnTriggerEnter(Collider other)
    {
        if (!other.CompareTag("Chef"))
            return;

        if (currentIngredient == null && !isRespawning)
        {
            SpawnIngredient();
        }
    }

    private void SpawnIngredient()
    {
        if (ingredientPrefab == null || spawnPoint == null)
            return;

        currentIngredient = Instantiate( ingredientPrefab, spawnPoint.position,spawnPoint.rotation);
    }

    public void IngredientTaken()
    {
        if (currentIngredient == null)
            return;

        currentIngredient = null;
        if (!isRespawning)
        {
            StartCoroutine(RespawnIngredient());
        }
    }

    private IEnumerator RespawnIngredient()
    {
        isRespawning = true;

        if (progressBarObject != null)
            progressBarObject.SetActive(true);

        if (progressBar != null)
            progressBar.value = 0f;

        float timer = 0f;

        while (timer < respawnTime)
        {
            timer += Time.deltaTime;

            if (progressBar != null)
            {
                progressBar.value =
                    Mathf.Clamp01(timer / respawnTime);
            }

            yield return null;
        }

        if (progressBarObject != null)
        {
            progressBarObject.SetActive(false);
        }

        SpawnIngredient();

        isRespawning = false;
    }
}

