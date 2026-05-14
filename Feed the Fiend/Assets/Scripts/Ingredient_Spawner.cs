using UnityEngine;

public class Ingredient_Spawner : MonoBehaviour
{
    public GameObject [] typeFood = new GameObject[6];
    public GameObject objectToSpawn;
    public Transform player;
    public float spawnDistance = 5.0f;
    private bool hasSpawned = false;

    void Update()
    {
        if (!hasSpawned && Vector3.Distance(transform.position, player.position) < spawnDistance)
        {
            Instantiate(objectToSpawn, transform.position, Quaternion.identity);
            hasSpawned = true;
        }
    }
}

