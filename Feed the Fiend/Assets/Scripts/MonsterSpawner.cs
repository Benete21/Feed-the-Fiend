
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

    private AudioManager audioManager;

    private void Awake()
    {
        GameObject audioObject =
            GameObject.FindGameObjectWithTag("Audio");

        if (audioObject != null)
            audioManager = audioObject.GetComponent<AudioManager>();
    }

    private void Start()
    {
        StartNewDay(currentDay);
    }

    public void StartNewDay(int day)
    {
        ClearMonsterTrails();

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

        UpdateUI();

        if (totalMonsters <= 0)
        {
            StartCoroutine(HandleDayComplete());
            return;
        }

        spawnCoroutine = StartCoroutine(SpawnMonstersOverTime());
    }

    private IEnumerator SpawnMonstersOverTime()
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

    private void SpawnMonster()
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

        if (audioManager != null)
            audioManager.playMonsterEnter();

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
        if (dayCompletionStarted == false)
            dayCompletionStarted = true;

        FreezePlayers(true);

        if (spawnCoroutine != null)
        {
            StopCoroutine(spawnCoroutine);
            spawnCoroutine = null;
        }

        Debug.Log("Day " + currentDay + " completed!");

        if (audioManager != null)
            audioManager.PlayDayComplete();

        GameObject finishScreen = null;

        if (currentDay == 1)
            finishScreen = Day1Finish;
        else if (currentDay == 2)
            finishScreen = Day2Finish;

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

        // Day 2 is the final day.
        if (currentDay >= 2)
        {
            Debug.Log("All days completed! Day 2 was the final day.");
            yield break;
        }

        // Show the next-day starting UI.
        if (DayStarting != null)
            DayStarting.SetActive(true);

        yield return new WaitForSeconds(dayStartingDuration);

        if (DayStarting != null)
            DayStarting.SetActive(false);

        // Start Day 2.
        StartNewDay(currentDay + 1);

        FreezePlayers(false);
    }

    public void UpdateUI()
    {
        if (dayText != null)
            dayText.text = "Day " + currentDay;

        if (monsterText != null)
            monsterText.text = monstersLeft + " / " + totalMonsters;

        if (monsterProgressBar != null)
        {
            monsterProgressBar.fillAmount = totalMonsters > 0
                ? (float)monstersLeft / totalMonsters
                : 0;
        }
    }

    public void NextDay()
    {
        if (currentDay >= 2)
        {
            Debug.Log("Day 2 is the final day!");
            return;
        }

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

    private void ClearMonsterTrails()
    {
        Monster_Trail[] trails =
            FindObjectsOfType<Monster_Trail>();

        foreach (Monster_Trail trail in trails)
        {
            Destroy(trail.gameObject);
        }

        Debug.Log("All monster trails cleared!");
    }
}
