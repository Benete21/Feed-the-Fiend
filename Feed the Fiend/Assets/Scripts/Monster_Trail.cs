using UnityEngine;

public class Monster_Trail : MonoBehaviour
{
    public enum HazardType
    {
        ToxicSlime,
        Fire
    }

    [Header("Hazard Type")]
    public HazardType hazardType = HazardType.ToxicSlime;

    [Header("Duration")]
    public float lifetime = 6f;

    [Header("Damage")]
    public float damage = 10f;
    public float damageInterval = 1f;

    [Header("Slow Effect")]
    public bool slowsPlayer = true;
    [Range(0.1f, 1f)]
    public float slowMultiplier = 0.5f;
    public float slowDuration = 2f;

    private void Start()
    {
        Destroy(gameObject, lifetime);
    }

    private void OnTriggerStay(Collider other)
    {
        if (!other.CompareTag("Player"))
            return;

        PlayerHP health = other.GetComponentInParent<PlayerHP>();

        if (health == null)
            return;

        // Damage is handled by a separate timed routine per player.
        PlayerHazardContact contact =
            other.GetComponent<PlayerHazardContact>();

        if (contact == null)
            contact = other.gameObject.AddComponent<PlayerHazardContact>();

        contact.TryApplyHazard(
            this,
            health,
            slowsPlayer,
            slowMultiplier,
            slowDuration,
            damage,
            damageInterval
        );
    }
}

public class PlayerHazardContact : MonoBehaviour
{
    private Monster_Trail activeHazard;
    private float nextDamageTime;

    public void TryApplyHazard(
        Monster_Trail hazard,
        PlayerHP health,
        bool applySlow,
        float multiplier,
        float slowDuration,
        float damage,
        float damageInterval)
    {
        if (Time.time < nextDamageTime)
            return;

        nextDamageTime = Time.time + damageInterval;
        activeHazard = hazard;

        if (health != null)
            health.TakeDamage(damage);

        if (applySlow)
        {
            PlayerStatus effects =
                GetComponentInParent<PlayerStatus>();

            if (effects != null)
                effects.ApplySlow(slowDuration, multiplier);
        }
    }
}

