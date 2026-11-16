
using UnityEngine;
using UnityEngine.AI;
using System.Collections;
using System.Collections.Generic;

public class MonsterAI : MonoBehaviour
{
    private AudioManager audioManager;

    [Header("Spawner")]
    public MonsterSpawner monsterSpawner;

    [Header("Movement")]
    public NavMeshAgent agent;
    public float walkSpeed = 2f;
    public float berserkSpeed = 5f;

    [Header("Animations")]
    [SerializeField] private Animator animator;

    private bool isWalking;
    private bool isSitting;

    [Header("Sitting")]
    [SerializeField] private float sittingRotationSpeed = 360f;

    [Header("Chair Assignment")]
    [SerializeField] private float chairRetryInterval = 2f;
    private float chairRetryTimer;

    [Header("Wandering")]
    public bool wanderBeforeTable = true;
    public float wanderDuration = 7f;
    public float wanderRadius = 5f;
    public float minWanderWait = 1f;
    public float maxWanderWait = 2f;

    private bool isWandering = false;
    private float wanderTimer;
    private float wanderWaitTimer;

    [Header("Restaurant Tables")]
    public RestrauntTable[] tables;

    public int assignedTableNumber = -1;

    private RestrauntTable assignedTable;
    private Transform assignedChair;

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
        if (animator == null)
            animator = GetComponentInChildren<Animator>();

        GameObject audioObject =
            GameObject.FindGameObjectWithTag("Audio");

        if (audioObject != null)
            audioManager = audioObject.GetComponent<AudioManager>();

        originalSpawnPosition = transform.position;
        originalSpawnRotation = transform.rotation;

        if (happyUI != null)
            happyUI.SetActive(false);
    }


    void Start()
    {
        if (agent == null)
            agent = GetComponent<NavMeshAgent>();

        if (order == null)
            order = GetComponent<CustomerOrder>();

        if (agent == null)
        {
            Debug.LogError("NavMeshAgent is missing!");
            enabled = false;
            return;
        }

        agent.speed = walkSpeed;
        agent.updateRotation = true;

        if (wanderBeforeTable)
        {
            isWandering = true;
            wanderTimer = wanderDuration;
            wanderWaitTimer = 0f;

            WanderToRandomPoint();
        }
        else
        {
            ChooseRandomTable();
        }

        UpdateAnimations();
    }


    void Update()
    {
        if (isSatisfied)
        {
            UpdateAnimations();
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
        else if (isWandering)
        {
            WanderUpdate();
        }
        else
        {
            RestaurantUpdate();
        }

        UpdateAnimations();
    }


    // =========================================================
    // ANIMATIONS
    // =========================================================

    void UpdateAnimations()
    {
        if (animator == null || agent == null)
            return;

        isSitting = hasReachedTable &&
                    !isBerserk &&
                    !isReturningToSpawn &&
                    !isSatisfied;

        isWalking = agent.velocity.sqrMagnitude > 0.05f &&
                    !isSitting &&
                    !isSatisfied;

        animator.SetBool("isWalking", isWalking);
        animator.SetBool("isSitting", isSitting);
    }


    // =========================================================
    // RESTAURANT
    // =========================================================

    void RestaurantUpdate()
    {
        // No chair assigned? Keep trying to find one.
        if (assignedTable == null || assignedChair == null)
        {
            chairRetryTimer -= Time.deltaTime;

            if (chairRetryTimer <= 0f)
            {
                ChooseRandomTable();
            }

            return;
        }

        // Stay facing the table while sitting.
        if (hasReachedTable)
        {
            FaceTable();
            return;
        }

        if (!agent.pathPending &&
            agent.remainingDistance <= agent.stoppingDistance + 0.2f)
        {
            agent.isStopped = true;
            agent.updateRotation = false;

            hasReachedTable = true;

            FaceTable();

            Debug.Log(
                gameObject.name +
                " has arrived at Chair at Table " +
                assignedTableNumber
            );

            if (order != null)
                order.StartWaitingForOrder();

            return;
        }

        agent.isStopped = false;
        agent.updateRotation = true;

        agent.SetDestination(assignedChair.position);
    }


    void FaceTable()
    {
        if (assignedTable == null)
            return;

        Vector3 direction =
            assignedTable.transform.position - transform.position;

        direction.y = 0f;

        if (direction.sqrMagnitude < 0.001f)
            return;

        Quaternion targetRotation =
            Quaternion.LookRotation(direction);

        transform.rotation = Quaternion.RotateTowards(
            transform.rotation,
            targetRotation,
            sittingRotationSpeed * Time.deltaTime
        );
    }


    void ChooseRandomTable()
    {
        chairRetryTimer = chairRetryInterval;

        if (tables == null || tables.Length == 0)
        {
            Debug.LogWarning(
                "No restaurant tables assigned to " +
                gameObject.name
            );

            return;
        }

        List<RestrauntTable> availableTables =
            new List<RestrauntTable>();

        foreach (RestrauntTable table in tables)
        {
            if (table != null && table.HasAvailableChair())
                availableTables.Add(table);
        }

        if (availableTables.Count == 0)
        {
            Debug.Log(
                gameObject.name +
                " is waiting for an available chair."
            );

            return;
        }

        int randomIndex =
            Random.Range(0, availableTables.Count);

        assignedTable = availableTables[randomIndex];

        if (!assignedTable.TryReserveChair(out assignedChair))
        {
            assignedTable = null;
            return;
        }

        hasReachedTable = false;

        assignedTableNumber =
            assignedTable.tableNumber;

        agent.isStopped = false;
        agent.updateRotation = true;

        agent.SetDestination(assignedChair.position);

        Debug.Log(
            gameObject.name +
            " assigned to Chair at Table " +
            assignedTableNumber
        );
    }


    // =========================================================
    // SATISFIED
    // =========================================================

    public void BecomeSatisfied()
    {
        if (isSatisfied || isBerserk)
            return;

        Debug.Log(gameObject.name + " is satisfied!");

        isSatisfied = true;

        agent.isStopped = true;

        LeaveTable();

        currentTarget = null;

        if (happyUI != null)
            happyUI.SetActive(true);

        UpdateAnimations();

        StartCoroutine(SatisfiedRoutine());
    }


    IEnumerator SatisfiedRoutine()
    {
        yield return new WaitForSeconds(satisfiedWaitTime);

        if (happyUI != null)
            happyUI.SetActive(false);

        isSatisfied = false;

        isReturningToSpawn = true;

        agent.speed = walkSpeed;
        agent.updateRotation = true;
        agent.isStopped = false;

        SetSpawnDestination();

        UpdateAnimations();

        Debug.Log(gameObject.name + " is leaving the restaurant.");
    }


    // =========================================================
    // RETURN TO SPAWN
    // =========================================================

    void ReturnToSpawnUpdate()
    {
        Vector3 targetPosition;

        if (spawnPoint != null)
            targetPosition = spawnPoint.position;
        else
            targetPosition = originalSpawnPosition;

        if (!agent.pathPending &&
            agent.remainingDistance <= agent.stoppingDistance + 0.2f)
        {
            agent.isStopped = true;

            UpdateAnimations();

            Debug.Log(gameObject.name + " has left the restaurant.");

            if (monsterSpawner != null)
                monsterSpawner.Served();

            Destroy(gameObject);
            return;
        }

        agent.isStopped = false;
        agent.updateRotation = true;
        agent.SetDestination(targetPosition);
    }


    void SetSpawnDestination()
    {
        if (spawnPoint != null)
            agent.SetDestination(spawnPoint.position);
        else
            agent.SetDestination(originalSpawnPosition);
    }


    // =========================================================
    // BERSERK
    // =========================================================

    public void StartBerserk()
    {
        if (isBerserk)
            return;

        isBerserk = true;

        berserkTimer = berserkTimeLimit;

        Debug.Log("MONSTER HAS GONE BERSERK!");

        agent.speed = berserkSpeed;
        agent.updateRotation = true;

        LeaveTable();

        FindNearestPlayer();

        UpdateAnimations();
    }


    void BerserkUpdate()
    {
        berserkTimer -= Time.deltaTime;

        if (berserkTimer <= 0f)
        {
            LeaveAfterBerserk();
            return;
        }

        attackTimer -= Time.deltaTime;

        if (currentTarget == null)
            FindNearestPlayer();

        if (currentTarget == null)
            return;

        PlayerHP playerHealth =
            currentTarget.GetComponent<PlayerHP>();

        if (playerHealth != null &&
            playerHealth.GetCurrentHealth() <= 0f)
        {
            LeaveAfterBerserk();
            return;
        }

        float distance =
            Vector3.Distance(transform.position, currentTarget.position);

        if (distance > detectionRange)
        {
            currentTarget = null;
            return;
        }

        if (distance > attackRange)
        {
            agent.isStopped = false;

            agent.SetDestination(currentTarget.position);

            if (audioManager != null)
                audioManager.PlayMonsterMove();
        }
        else
        {
            agent.isStopped = true;

            AttackPlayer();

            if (audioManager != null)
                audioManager.StopMonsterMove();
        }
    }


    // =========================================================
    // LEAVE AFTER BERSERK
    // =========================================================

    void LeaveAfterBerserk()
    {
        if (isReturningToSpawn)
            return;

        Debug.Log(gameObject.name + " is leaving after berserk.");

        isBerserk = false;

        currentTarget = null;

        LeaveTable();

        agent.speed = walkSpeed;
        agent.updateRotation = true;

        isReturningToSpawn = true;

        agent.isStopped = false;

        SetSpawnDestination();

        UpdateAnimations();
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
                Vector3.Distance(transform.position, player.transform.position);

            if (distance < closestDistance &&
                distance <= detectionRange)
            {
                closestDistance = distance;
                closestPlayer = player.transform;
            }
        }

        currentTarget = closestPlayer;
    }


    void AttackPlayer()
    {
        if (attackTimer > 0f)
            return;

        attackTimer = attackCooldown;

        Debug.Log("MONSTER ATTACKED THE PLAYER!");

        PlayerHP playerHealth =
            currentTarget.GetComponent<PlayerHP>();

        if (playerHealth != null)
        {
            playerHealth.TakeDamage(attackDamage);

            Vector3 knockbackDirection =
                currentTarget.position - transform.position;

            playerHealth.Knockback(knockbackDirection);

            if (playerHealth.GetCurrentHealth() <= 0f)
            {
                Debug.Log("PLAYER HAS BEEN DEFEATED!");

                LeaveAfterBerserk();
            }
        }
    }


    // =========================================================
    // TABLE
    // =========================================================

    void LeaveTable()
    {
        hasReachedTable = false;

        if (assignedTable != null && assignedChair != null)
        {
            assignedTable.ReleaseChair(assignedChair);
        }

        assignedChair = null;
        assignedTable = null;

        assignedTableNumber = -1;

        if (agent != null)
            agent.updateRotation = true;
    }


    private void OnDestroy()
    {
        LeaveTable();
    }


    // =========================================================
    // WANDERING
    // =========================================================

    void WanderUpdate()
    {
        wanderTimer -= Time.deltaTime;

        if (wanderTimer <= 0f)
        {
            isWandering = false;

            agent.isStopped = false;

            ChooseRandomTable();
            return;
        }

        if (agent.pathPending)
            return;

        if (agent.remainingDistance <= agent.stoppingDistance + 0.2f)
        {
            agent.isStopped = true;

            wanderWaitTimer -= Time.deltaTime;

            if (wanderWaitTimer <= 0f)
                WanderToRandomPoint();
        }
    }


    void WanderToRandomPoint()
    {
        for (int i = 0; i < 10; i++)
        {
            Vector3 randomOffset =
                Random.insideUnitSphere * wanderRadius;

            Vector3 randomPosition =
                originalSpawnPosition + randomOffset;

            NavMeshHit hit;

            if (NavMesh.SamplePosition(
                randomPosition,
                out hit,
                2f,
                NavMesh.AllAreas))
            {
                agent.isStopped = false;
                agent.updateRotation = true;

                agent.SetDestination(hit.position);

                wanderWaitTimer = Random.Range(
                    minWanderWait,
                    maxWanderWait
                );

                return;
            }
        }

        wanderWaitTimer = 1f;
    }
}
