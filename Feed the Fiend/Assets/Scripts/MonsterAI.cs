using UnityEngine;
using UnityEngine.AI;

public class MonsterAI : MonoBehaviour
{
    [Header("Movement")]
    public NavMeshAgent agent;
    public float walkSpeed = 2f;
    public float berserkSpeed = 5f;

    [Header("Wander")]
    public float wanderRadius = 10f;
    public float wanderInterval = 4f;

    [Header("Player Detection")]
    public float detectionRange = 20f;
    public string playerTag = "Player";

    [Header("Attack")]
    public float attackRange = 2f;
    public float attackDamage = 20f;
    public float attackCooldown = 1.5f;

    [Header("Berserk")]
    public bool isBerserk = false;

    private float wanderTimer;
    private float attackTimer;

    private Transform currentTarget;


    void Start()
    {
        if (agent == null)
            agent = GetComponent<NavMeshAgent>();

        agent.speed = walkSpeed;

        wanderTimer = wanderInterval;

        // Start walking around
        SetRandomWanderPoint();
    }


    void Update()
    {
        if (isBerserk)
        {
            BerserkUpdate();
        }
        else
        {
            WanderUpdate();
        }
    }


    // =========================
    // NORMAL WANDERING
    // =========================

    void WanderUpdate()
    {
        wanderTimer -= Time.deltaTime;

        if (wanderTimer <= 0f || agent.remainingDistance <= 0.5f)
        {
            SetRandomWanderPoint();

            wanderTimer = wanderInterval;
        }
    }


    void SetRandomWanderPoint()
    {
        Vector3 randomDirection =
            Random.insideUnitSphere * wanderRadius;

        randomDirection += transform.position;

        NavMeshHit hit;

        if (NavMesh.SamplePosition(
            randomDirection,
            out hit,
            wanderRadius,
            NavMesh.AllAreas))
        {
            agent.SetDestination(hit.position);
        }
    }


    // =========================
    // BERSERK
    // =========================

    public void StartBerserk()
    {
        if (isBerserk)
            return;

        isBerserk = true;

        Debug.Log("MONSTER HAS GONE BERSERK!");

        agent.speed = berserkSpeed;

        // Find a player immediately
        FindNearestPlayer();
    }


    void BerserkUpdate()
    {
        attackTimer -= Time.deltaTime;

        // Find a target if we don't have one
        if (currentTarget == null)
        {
            FindNearestPlayer();
        }

        if (currentTarget == null)
        {
            return;
        }

        float distance =
            Vector3.Distance(transform.position, currentTarget.position);

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
            agent.SetDestination(currentTarget.position);
        }
        else
        {
            // Stop when close enough to attack
            agent.isStopped = true;

            AttackPlayer();
        }
    }


    // =========================
    // FIND PLAYER
    // =========================

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
                closestPlayer = player.transform;
            }
        }

        currentTarget = closestPlayer;
    }


    // =========================
    // ATTACK
    // =========================

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
        }
    }
}