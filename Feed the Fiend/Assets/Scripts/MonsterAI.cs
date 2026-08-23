using UnityEngine;
using UnityEngine.AI;

public class MonsterAI : MonoBehaviour
{
    [Header("Movement")]
    public NavMeshAgent agent;
    public float walkSpeed = 2f;
    public float berserkSpeed = 5f;

    [Header("Restaurant Tables")]
    public RestrauntTable[] tables;

    public int assignedTableNumber = -1;

    private RestrauntTable assignedTable;

    [Header("Player Detection")]
    public float detectionRange = 20f;
    public string playerTag = "Player";

    [Header("Attack")]
    public float attackRange = 2f;
    public float attackDamage = 20f;
    public float attackCooldown = 1.5f;

    [Header("Berserk")]
    public bool isBerserk = false;

    private float attackTimer;
    private Transform currentTarget;


    void Start()
    {
        if (agent == null)
            agent = GetComponent<NavMeshAgent>();

        agent.speed = walkSpeed;

        // Choose a restaurant table
        ChooseRandomTable();
    }


    void Update()
    {
        if (isBerserk)
        {
            BerserkUpdate();
        }
        else
        {
            RestaurantUpdate();
        }
    }

    void RestaurantUpdate()
    {
        if (assignedTable == null)
            return;

        // Walk toward the assigned table
        if (agent.remainingDistance <= 0.5f)
        {
            agent.isStopped = true;

            // Monster has arrived at its table
            return;
        }

        agent.isStopped = false;
        agent.SetDestination(assignedTable.transform.position);
    }


    void ChooseRandomTable()
    {
        if (tables == null || tables.Length == 0)
        {
            Debug.LogWarning("No restaurant tables assigned to " + gameObject.name);
            return;
        }

        // Create a list of tables that aren't occupied
        System.Collections.Generic.List<RestrauntTable> availableTables =
            new System.Collections.Generic.List<RestrauntTable>();

        foreach (RestrauntTable table in tables)
        {
            if (table != null && !table.isOccupied)
            {
                availableTables.Add(table);
            }
        }

        // No available tables
        if (availableTables.Count == 0)
        {
            Debug.Log("No available tables for " + gameObject.name);
            return;
        }

        // Pick a random available table
        int randomIndex = Random.Range(0, availableTables.Count);

        assignedTable = availableTables[randomIndex];

        // Reserve the table
        assignedTable.isOccupied = true;

        // Store the table number
        assignedTableNumber = assignedTable.tableNumber;

        Debug.Log(
            gameObject.name +
            " has been assigned to Table " +
            assignedTableNumber
        );

        // Start walking there
        agent.isStopped = false;
        agent.SetDestination(assignedTable.transform.position);
    }

    public void StartBerserk()
    {
        if (isBerserk)
            return;

        isBerserk = true;

        Debug.Log("MONSTER HAS GONE BERSERK!");

        agent.speed = berserkSpeed;

        // Free the table
        LeaveTable();

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
            agent.SetDestination(currentTarget.position);
        }
        else
        {
            // Stop when close enough to attack
            agent.isStopped = true;

            AttackPlayer();
        }
    }


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
