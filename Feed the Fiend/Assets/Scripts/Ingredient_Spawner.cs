using UnityEngine;

public class Ingredient_Spawner : MonoBehaviour
{
    [Header("Ingredient")]
    [SerializeField] private GameObject ingredientPrefab;
    [SerializeField] private Transform spawnPoint;

    private Chef_Controls chefInRange;

    public bool ChefInRange => chefInRange != null;

    private void OnTriggerEnter(Collider other)
    {
        Chef_Controls chef = other.GetComponent<Chef_Controls>();

        if (chef != null)
        {
            chefInRange = chef;
        }
    }

    private void OnTriggerExit(Collider other)
    {
        Chef_Controls chef = other.GetComponent<Chef_Controls>();

        if (chef != null && chefInRange == chef)
        {
            chefInRange = null;
        }
    }

    public GameObject TakeIngredient()
    {
        if (!ChefInRange)
            return null;

        if (ingredientPrefab == null || spawnPoint == null)
            return null;

        return Instantiate(
            ingredientPrefab,
            spawnPoint.position,
            spawnPoint.rotation
        );
    }
}
