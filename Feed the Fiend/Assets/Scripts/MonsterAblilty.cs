using UnityEngine;
using UnityEngine.AI;

public class MonsterAblilty : MonoBehaviour
{

    public enum AbilityType
    {
        None,
        ToxicTrail,
        FireTrail
    }

    [Header("Ability")]
    public AbilityType ability = AbilityType.ToxicTrail;

    [Header("Hazard Prefabs")]
    public GameObject toxicTrailPrefab;
    public GameObject fireTrailPrefab;

    [Header("Trail Settings")]
    public float dropInterval = 1.5f;
    public float minimumMoveDistance = 1f;
    public float groundOffset = 0.05f;

    [Header("Movement Check")]
    public float minimumSpeed = 0.2f;

    private MonsterAI monsterAI;
    private NavMeshAgent agent;

    private float dropTimer;
    private Vector3 lastDropPosition;

    private void Awake()
    {
        monsterAI = GetComponent<MonsterAI>();
        agent = GetComponent<NavMeshAgent>();

        lastDropPosition = transform.position;
    }

    private void Update()
    {
        if (ability == AbilityType.None)
            return;

        if (agent == null || !agent.enabled || !agent.isOnNavMesh)
            return;

        if (agent.isStopped || agent.velocity.magnitude < minimumSpeed)
            return;

        dropTimer += Time.deltaTime;

        if (dropTimer < dropInterval)
            return;

        if (Vector3.Distance(transform.position, lastDropPosition)
            < minimumMoveDistance)
            return;

        DropHazard();
    }

    private void DropHazard()
    {
        GameObject prefab = null;

        switch (ability)
        {
            case AbilityType.ToxicTrail:
                prefab = toxicTrailPrefab;
                break;

            case AbilityType.FireTrail:
                prefab = fireTrailPrefab;
                break;
        }

        if (prefab == null)
            return;

        Vector3 position = transform.position;
        position.y += groundOffset;

        Instantiate(prefab, position, Quaternion.identity);

        lastDropPosition = transform.position;
        dropTimer = 0f;
    }
}
