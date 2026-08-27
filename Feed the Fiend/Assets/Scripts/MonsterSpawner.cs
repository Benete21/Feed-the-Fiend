using System.Collections;
using UnityEditorInternal;
using UnityEngine;
using UnityEngine.AI;
using UnityEngine.UI;
using TMPro;

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

    public GameObject Day1Finish;

    [Header("Time")]
    public float minSpawn = 10f;
    public float maxSpawn = 30f;

    [Header("Restaurant Tables")]
    public RestrauntTable[] tables;

    [Header("NavMesh")]
    public float navMeshSearchRadius = 3f;
    private Coroutine spawnCoroutine;

    [Header("UI")]
    public TMP_Text dayText;
    public TMP_Text monsterText;
    public Image monsterProgressBar;
    private int totalMonsters;
    private int monstersLeft;

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

        totalMonsters = startingMonsterCount + ((currentDay - 1) * monstersAddedPerDay);
        monstersLeft = totalMonsters;

        UpdateUI();

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
        spawnCoroutine = null;
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

        GameObject randomPrefab =monsterPrefabs[Random.Range(0, monsterPrefabs.Length)];
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

        monsterAI.tables = tables;
    }
    public void Served()
    {
        monstersLeft--;
        monstersLeft = Mathf.Max(monstersLeft, 0);

        UpdateUI();

        if(monstersLeft == 0)
        {
            Day1Finish.SetActive(true);
        }
    }

    public void UpdateUI()
    {
        if(dayText != null)
        {
            dayText.text = "Day" + currentDay;
        }
        if (monsterText != null)
        {
            monsterText.text =
                monstersLeft + " / " + totalMonsters;
        }

        if (monsterProgressBar != null)
        {
            if (totalMonsters > 0)
            {
                monsterProgressBar.fillAmount = (float) monstersLeft / totalMonsters;
            }
            else
            {
                monsterProgressBar.fillAmount = 0;
            }
        }
    }



public void NextDay()
    {
        currentDay++;

        StartNewDay(currentDay);
    }
}
