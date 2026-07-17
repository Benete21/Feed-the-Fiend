using System.Collections;
using UnityEngine;

public class Ingredient_Spawner : MonoBehaviour
{
    public GameObject ingredientPrefab;
    public Transform spawnPoint;

    private GameObject currentIngredient;

    private void OnTriggerEnter(Collider other)
    {
        if (!other.CompareTag("Chef")) return;

            SpawnIngredient();
    }

    void SpawnIngredient()
    {
        currentIngredient = Instantiate( ingredientPrefab, spawnPoint.position, spawnPoint.rotation);
    }
    public void IngredientTaken()
    {
        currentIngredient = null;
    }
}

