using UnityEngine;

public class Food_Spawner : MonoBehaviour
{
    [SerializeField] private Transform spawnPoint;

    public void SpawnFood(GameObject foodPrefab)
    {
        if (foodPrefab == null)
        {
            Debug.LogWarning("Food prefab is missing.");
            return;
        }

        if (spawnPoint == null)
        {
            Debug.LogWarning("Food spawn point is missing.");
            return;
        }

        Instantiate(foodPrefab,spawnPoint.position,spawnPoint.rotation);
    }
}
