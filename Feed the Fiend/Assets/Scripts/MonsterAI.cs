using UnityEngine;
using UnityEngine.AI;

public class MonsterAI : MonoBehaviour
{
    private AudioManager audioManager;

    [Header("Movement")]
    public NavMeshAgent agent;
    public float walkSpeed = 2f;
    public float berserkSpeed = 5f;

    [Header("Restaurant Tables")]
    public RestrauntTable[] tables;

    public int assignedTableNumber = -1;

    private RestrauntTable assignedTable;

    private bool hasReachedTable = false;
    public CustomerOrder order;

    [Header("Player Detection")]
    public float detectionRange = 20f;
    public string playerTag = "Player";

    [Header("Attack")]
    public float attackRange = 2f;
    public float attackDamage = 20f;
    public float attackCooldown = 1.5f;

    [Header("Berserk")]
    public bool isBerserk = false;

    // How long the monster can remain berserk
    public float berserkTimeLimit = 40f;

    private float berserkTimer;

    [Header("Satisfied")]
    public float satisfiedWaitTime = 3f;

    public GameObject happyUI;
    public Transform spawnPoint;

    private float attackTimer;
    private Transform currentTarget;

    private Vector3 originalSpawnPosition;
    private Quaternion originalSpawnRotation;

    private bool isSatisfied = false;
    private bool isReturningToSpawn = false;


    void Awake()
    {
        audioManager =
            GameObject.FindGameObjectWithTag("Audio")
            .GetComponent<AudioManager>();

        // Remember where this monster spawned
        originalSpawnPosition = transform.position;
        originalSpawnRotation = transform.rotation;

        // Hide happy UI
        if (happyUI != null)
        {
            happyUI.SetActive(false);
        }
    }


    void Start()
    {
        if (agent == null)
            agent = GetComponent<NavMeshAgent>();

        if (order == null)
            order = GetComponent<CustomerOrder>();

        agent.speed = walkSpeed;

        ChooseRandomTable();
    }


    void Update()
    {
        // Satisfied monster is handled separately
        if (isSatisfied)
        {
            return;
        }

        if (isBerserk)
        {
            BerserkUpdate();
        }
        else if (isReturningToSpawn)
        {
            ReturnToSpawnUpdate();
        }
        else
        {
            RestaurantUpdate();
        }
    }


    // =========================================================
    // RESTAURANT
    // =========================================================

    void RestaurantUpdate()
    {
        if (assignedTable == null)
            return;

        if (!agent.pathPending &&
            agent.remainingDistance <= 0.5f)
        {
            agent.isStopped = true;

            if (!hasReachedTable)
            {
                hasReachedTable = true;

                Debug.Log(
                    gameObject.name +
                    " has arrived at Table " +
                    assignedTableNumber
                );

                if (order != null)
                {
                    order.StartWaitingForOrder();
                }
            }

            return;
        }

        agent.isStopped = false;

        agent.SetDestination(
            assignedTable.transform.position
        );
    }


    void ChooseRandomTable()
    {
        if (tables == null || tables.Length == 0)
        {
            Debug.LogWarning(
                "No restaurant tables assigned to " +
                gameObject.name
            );

            return;
        }

        System.Collections.Generic.List<RestrauntTable> availableTables =
            new System.Collections.Generic.List<RestrauntTable>();

        foreach (RestrauntTable table in tables)
        {
            if (table != null && !table.isOccupied)
            {
                availableTables.Add(table);
            }
        }

        if (availableTables.Count == 0)
        {
            Debug.Log(
                "No available tables for " +
                gameObject.name
            );

            return;
        }

        int randomIndex =
            Random.Range(0, availableTables.Count);

        assignedTable =
            availableTables[randomIndex];

        assignedTable.isOccupied = true;

        assignedTableNumber =
            assignedTable.tableNumber;

        Debug.Log(
            gameObject.name +
            " has been assigned to Table " +
            assignedTableNumber
        );

        agent.isStopped = false;

        agent.SetDestination(
            assignedTable.transform.position
        );
    }


    // =========================================================
    // SATISFIED
    // =========================================================

    public void BecomeSatisfied()
    {
        if (isSatisfied || isBerserk)
            return;

        Debug.Log(
            gameObject.name +
            " is satisfied!"
        );

        isSatisfied = true;

        agent.isStopped = true;

        LeaveTable();

        currentTarget = null;

        if (happyUI != null)
        {
            happyUI.SetActive(true);
        }

        StartCoroutine(SatisfiedRoutine());
    }


    System.Collections.IEnumerator SatisfiedRoutine()
    {
        Debug.Log(
            gameObject.name +
            " is happy and will leave in " +
            satisfiedWaitTime +
            " seconds."
        );

        yield return new WaitForSeconds(
            satisfiedWaitTime
        );

        if (happyUI != null)
        {
            happyUI.SetActive(false);
        }

        isSatisfied = false;

        isReturningToSpawn = true;

        agent.speed = walkSpeed;
        agent.isStopped = false;

        SetSpawnDestination();

        Debug.Log(
            gameObject.name +
            " is leaving the restaurant."
        );
    }


    // =========================================================
    // RETURN TO SPAWN
    // =========================================================

    void ReturnToSpawnUpdate()
    {
        Vector3 targetPosition;

        if (spawnPoint != null)
        {
            targetPosition = spawnPoint.position;
        }
        else
        {
            targetPosition = originalSpawnPosition;
        }

        if (!agent.pathPending &&
            agent.remainingDistance <= 0.5f)
        {
            agent.isStopped = true;

            Debug.Log(
                gameObject.name +
                " has returned to the spawn position."
            );

            Destroy(gameObject);

            return;
        }

        agent.isStopped = false;

        agent.SetDestination(targetPosition);
    }


    void SetSpawnDestination()
    {
        if (spawnPoint != null)
        {
            agent.SetDestination(
                spawnPoint.position
            );
        }
        else
        {
            agent.SetDestination(
                originalSpawnPosition
            );
        }
    }


    // =========================================================
    // BERSERK
    // =========================================================

    public void StartBerserk()
    {
        if (isBerserk)
            return;

        isBerserk = true;

        // Start the 40 second berserk timer
        berserkTimer = berserkTimeLimit;

        Debug.Log(
            "MONSTER HAS GONE BERSERK!"
        );

        agent.speed = berserkSpeed;

        LeaveTable();

        FindNearestPlayer();
    }


    void BerserkUpdate()
    {
        // Count down berserk time
        berserkTimer -= Time.deltaTime;

        // 40 seconds have passed
        if (berserkTimer <= 0f)
        {
            Debug.Log(
                gameObject.name +
                " survived its berserk attack time. Leaving restaurant."
            );

            LeaveAfterBerserk();
            return;
        }

        attackTimer -= Time.deltaTime;

        // Find a target if we don't have one
        if (currentTarget == null)
        {
            FindNearestPlayer();
        }

        // No player found
        if (currentTarget == null)
        {
            return;
        }

        // Check player's HP
        PlayerHP playerHealth =
            currentTarget.GetComponent<PlayerHP>();

        if (playerHealth != null)
        {
            if (playerHealth.GetCurrentHealth() <= 0f)
            {
                Debug.Log(
                    gameObject.name +
                    " defeated the player! Leaving restaurant."
                );

                LeaveAfterBerserk();
                return;
            }
        }

        float distance =
            Vector3.Distance(
                transform.position,
                currentTarget.position
            );

        // Player is too far away
        if (distance > detectionRange)
        {
            currentTarget = null;
            return;
        }

        // Move toward player
        if (distance > attackRange)
        {
            agent.isStopped = false;

            agent.SetDestination(
                currentTarget.position
            );

            if (audioManager != null)
            {
                audioManager.PlayMonsterMove();
            }
        }
        else
        {
            agent.isStopped = true;

            AttackPlayer();

            if (audioManager != null)
            {
                audioManager.StopMonsterMove();
            }
        }
    }


    // =========================================================
    // LEAVE AFTER BERSERK
    // =========================================================

    void LeaveAfterBerserk()
    {
        if (isReturningToSpawn)
            return;

        Debug.Log(
            gameObject.name +
            " is finished berserking and is leaving."
        );

        isBerserk = false;

        currentTarget = null;

        LeaveTable();

        agent.speed = walkSpeed;

        isReturningToSpawn = true;

        agent.isStopped = false;

        SetSpawnDestination();
    }


    // =========================================================
    // PLAYER
    // =========================================================

    void FindNearestPlayer()
    {
        GameObject[] players =
            GameObject.FindGameObjectsWithTag(playerTag);

        float closestDistance = Mathf.Infinity;

        Transform closestPlayer = null;

        foreach (GameObject player in players)
        {
            float distance =
                Vector3.Distance(
                    transform.position,
                    player.transform.position
                );

            if (distance < closestDistance &&
                distance <= detectionRange)
            {
                closestDistance = distance;

                closestPlayer =
                    player.transform;
            }
        }

        currentTarget = closestPlayer;
    }
    void AttackPlayer()
    {
        if (attackTimer > 0f)
            return;

        attackTimer = attackCooldown;

        Debug.Log(
            "MONSTER ATTACKED THE PLAYER!"
        );

        PlayerHP playerHealth =
            currentTarget.GetComponent<PlayerHP>();

        if (playerHealth != null)
        {
            // Damage player
            playerHealth.TakeDamage(attackDamage);

            // Push player away from monster
            Vector3 knockbackDirection =
                currentTarget.position - transform.position;

            playerHealth.Knockback(
                knockbackDirection
            );

            // Check if player died
            if (playerHealth.GetCurrentHealth() <= 0f)
            {
                Debug.Log(
                    "PLAYER HAS BEEN DEFEATED!"
                );

                LeaveAfterBerserk();
            }
        }
    }
        // =========================================================
        // TABLE
        // =========================================================

        void LeaveTable()
        {
            if (assignedTable != null)
            {
                assignedTable.isOccupied = false;

                assignedTable = null;

                assignedTableNumber = -1;
            }
        }


        private void OnDestroy()
        {
            LeaveTable();
        }
}