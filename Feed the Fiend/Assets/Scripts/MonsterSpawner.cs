using System.Collections;
using UnityEngine;
using UnityEngine.AI;
using UnityEngine.UI;
using TMPro;

public class MonsterSpawner : MonoBehaviour
{

    [Header("Player Controls")]
    public MonoBehaviour player1Controls;
    public MonoBehaviour player2Controls;

    [System.Serializable]
    public class DaySettings
    {
        public string dayName = "Day 1";

        [Header("Monster Types For This Day")]
        public GameObject[] monsterPrefabs;

        [Header("Monster Count")]
        public int monsterCount = 4;
    }

    [Header("Day Configurations")]
    public DaySettings[] daySettings;

    [Header("Spawn")]
    public Transform entranceSpawnPoint;

    [Header("Day Progression")]
    public int currentDay = 1;
    public GameObject DayStarting;
    public float dayStartingDuration = 3f;


    [Header("End Of Day Screens")]
    public GameObject Day1Finish;
    public GameObject Day2Finish;
    public GameObject Day3Finish;

    [Header("Day Transition Dialogue")]
    public float finishScreenDuration = 3f;
    public Day_Dialogou dayDialogue;

    private bool dayCompletionStarted;


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

    private GameObject[] currentMonsterPrefabs;

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
            spawnCoroutine = null;
        }

        int settingsIndex = currentDay - 1;

        if (daySettings == null ||
            settingsIndex < 0 ||
            settingsIndex >= daySettings.Length)
        {
            Debug.LogError("No settings configured for Day " + currentDay);
            return;
        }

        DaySettings settings = daySettings[settingsIndex];

        currentMonsterPrefabs = settings.monsterPrefabs;
        totalMonsters = settings.monsterCount;
        monstersLeft = totalMonsters;

        dayCompletionStarted = false;

        if (Day1Finish != null)
            Day1Finish.SetActive(false);

        if (Day2Finish != null)
            Day2Finish.SetActive(false);

        if (Day3Finish != null)
            Day3Finish.SetActive(false);

        UpdateUI();

        if (totalMonsters <= 0)
        {
            StartCoroutine(HandleDayComplete());
            return;
        }

        spawnCoroutine = StartCoroutine(SpawnMonstersOverTime());
    }


    IEnumerator SpawnMonstersOverTime()
    {
        for (int i = 0; i < totalMonsters; i++)
        {
            SpawnMonster();

            if (i < totalMonsters - 1)
            {
                float randomDelay = Random.Range(minSpawn, maxSpawn);
                yield return new WaitForSeconds(randomDelay);
            }
        }

        spawnCoroutine = null;
    }

    void SpawnMonster()
    {
        if (currentMonsterPrefabs == null ||
            currentMonsterPrefabs.Length == 0)
        {
            Debug.LogError(
                "No monster prefabs assigned for Day " + currentDay
            );
            return;
        }

        if (entranceSpawnPoint == null)
        {
            Debug.LogError("Entrance spawn point is not assigned!");
            return;
        }

        GameObject randomPrefab = currentMonsterPrefabs[
            Random.Range(0, currentMonsterPrefabs.Length)
        ];

        if (randomPrefab == null)
        {
            Debug.LogError("A monster prefab is missing!");
            return;
        }

        NavMeshHit hit;

        if (!NavMesh.SamplePosition(
            entranceSpawnPoint.position,
            out hit,
            navMeshSearchRadius,
            NavMesh.AllAreas))
        {
            Debug.LogError(
                "Could not find NavMesh position near entrance spawn point!"
            );
            return;
        }

        GameObject monster = Instantiate(
            randomPrefab,
            hit.position,
            entranceSpawnPoint.rotation
        );

        MonsterAI monsterAI = monster.GetComponent<MonsterAI>();

        if (monsterAI == null)
        {
            Debug.LogError(
                "Monster prefab does not have a MonsterAI component!"
            );

            Destroy(monster);
            return;
        }

        monsterAI.spawnPoint = entranceSpawnPoint;
        monsterAI.tables = tables;
        monsterAI.monsterSpawner = this;

        CustomerOrder customerOrder =
            monster.GetComponent<CustomerOrder>();

        if (customerOrder != null)
        {
            customerOrder.satisfied = this;
        }
        else
        {
            Debug.LogWarning(
                "Monster prefab does not have a CustomerOrder component!"
            );
        }
    }


    public void Served()
    {
        // Prevent completion from triggering more than once.
        if (dayCompletionStarted)
            return;

        monstersLeft = Mathf.Max(monstersLeft - 1, 0);

        Debug.Log("Monster served! Remaining: " + monstersLeft);

        UpdateUI();

        if (monstersLeft == 0)
        {
            dayCompletionStarted = true;
            StartCoroutine(HandleDayComplete());
        }
    }

    private IEnumerator HandleDayComplete()
    {
        dayCompletionStarted = true;

        // Freeze both players immediately.
        FreezePlayers(true);

        if (spawnCoroutine != null)
        {
            StopCoroutine(spawnCoroutine);
            spawnCoroutine = null;
        }

        Debug.Log("Day " + currentDay + " completed!");

        // Show the correct end-of-day screen.
        GameObject finishScreen = null;

        if (currentDay == 1)
            finishScreen = Day1Finish;
        else if (currentDay == 2)
            finishScreen = Day2Finish;
        else if (currentDay == 3)
            finishScreen = Day3Finish;

        if (finishScreen != null)
            finishScreen.SetActive(true);

        yield return new WaitForSeconds(finishScreenDuration);

        if (finishScreen != null)
            finishScreen.SetActive(false);

        // Play end-of-day dialogue.
        if (dayDialogue != null)
        {
            yield return StartCoroutine(
                dayDialogue.PlayDayEndDialogue(currentDay)
            );
        }

        // Stop after Day 3.
        if (currentDay >= 3)
        {
            Debug.Log("All three days completed!");
            yield break;
        }

        // Show the next-day starting UI.
        if (DayStarting != null)
            DayStarting.SetActive(true);

        yield return new WaitForSeconds(dayStartingDuration);

        if (DayStarting != null)
            DayStarting.SetActive(false);

        // Start the next day.
        StartNewDay(currentDay + 1);

        // Unfreeze both players after the starting UI finishes.
        FreezePlayers(false);
    }




    public void UpdateUI()
    {
        if (dayText != null)
        {
            dayText.text = "Day " + currentDay;
        }

        if (monsterText != null)
        {
            monsterText.text = monstersLeft + " / " + totalMonsters;
        }

        if (monsterProgressBar != null)
        {
            monsterProgressBar.fillAmount = totalMonsters > 0
                ? (float)monstersLeft / totalMonsters
                : 0;
        }
    }

    public void NextDay()
    {
        currentDay++;
        StartNewDay(currentDay);
    }
    private void FreezePlayers(bool freeze)
    {
        if (player1Controls != null)
            player1Controls.enabled = !freeze;

        if (player2Controls != null)
            player2Controls.enabled = !freeze;
    }
}
