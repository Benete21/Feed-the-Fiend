using System.Collections;
using UnityEngine;
using UnityEngine.AI;

public class MonsterSpawner : MonoBehaviour
{
    [Header("Monster Prefabs")]
    public GameObject[] monsterPrefabs;

    [Header("Spawn")]
    public Transform entranceSpawnPoint;

    [Header("Day Progression")]
    public int currentDay = 1;
    public int startingMonsterCount = 4;
    public int monstersAddedPerDay = 1;

    [Header("Time")]
    public float minSpawn = 10f;
    public float maxSpawn = 30f;

    [Header("Restaurant Tables")]
    public RestrauntTable[] tables;

    [Header("NavMesh")]
    public float navMeshSearchRadius = 3f;
    private Coroutine spawnCoroutine;


    void Start()
    {
        StartNewDay(currentDay);
    }

    public void StartNewDay(int day)
    {
        currentDay = day;

        if (spawnCoroutine != null)
        {
            StopCoroutine(spawnCoroutine);
        }

        spawnCoroutine = StartCoroutine(SpawnMonstersOverTime());
    }

    IEnumerator SpawnMonstersOverTime()
    {
        int monsterCount = startingMonsterCount +((currentDay - 1) * monstersAddedPerDay);

        for (int i = 0; i < monsterCount; i++)
        {
            SpawnMonster();

            if (i < monsterCount - 1)
            {
                float randomDelay = Random.Range( minSpawn,maxSpawn);
                yield return new WaitForSeconds(randomDelay);
            }
        }
    }
    void SpawnMonster()
    {
        if (monsterPrefabs == null || monsterPrefabs.Length == 0)
        {
            Debug.LogError("No monster prefabs assigned!");
            return;
        }

        if (entranceSpawnPoint == null)
        {
            Debug.LogError("Entrance spawn point is not assigned!");
            return;
        }


        // Pick a random monster type
        GameObject randomPrefab =
            monsterPrefabs[
                Random.Range(0, monsterPrefabs.Length)
            ];


        // Find the closest valid NavMesh position
        NavMeshHit hit;

        if (!NavMesh.SamplePosition(entranceSpawnPoint.position,out hit,navMeshSearchRadius,NavMesh.AllAreas))
        {
            return;
        }

        GameObject monster = Instantiate(randomPrefab, hit.position, entranceSpawnPoint.rotation);

        MonsterAI monsterAI = monster.GetComponent<MonsterAI>();

        if (monsterAI == null)
        {
            Destroy(monster);
            return;
        }


        // Give monster access to the tables
        monsterAI.tables = tables;
    }


    public void NextDay()
    {
        currentDay++;

        StartNewDay(currentDay);
    }
}
