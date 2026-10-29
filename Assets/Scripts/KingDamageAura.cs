using UnityEngine;

public class KingDamageAura : MonoBehaviour
{
    [Header("Damage")]
    public float radius = 4f;
    public float damagePerTick = 1f;
    public float damageInterval = 1f;

    [Header("Duration")]
    public float duration = 5f;

    private Transform player;

    private float nextDamageTime = 0f;

    public void Setup(Transform target)
    {
        player = target;
    }

    void Start()
    {
        Destroy(
            gameObject,
            duration
        );
    }

    void Update()
    {
        if (player == null)
            return;

        Vector3 auraPosition =
            transform.position;

        Vector3 playerPosition =
            player.position;

        auraPosition.y = 0f;
        playerPosition.y = 0f;

        float distance =
            Vector3.Distance(
                auraPosition,
                playerPosition
            );

        if (distance > radius)
            return;

        if (Time.time < nextDamageTime)
            return;

        PlayerHealth playerHealth =
            player.GetComponent<PlayerHealth>();

        if (playerHealth == null)
        {
            playerHealth =
                player.GetComponentInParent<PlayerHealth>();
        }

        if (playerHealth == null)
            return;

        Vector3 knockbackDirection =
            player.position -
            transform.position;

        knockbackDirection.y = 0f;

        playerHealth.TakeDamage(
            damagePerTick,
            knockbackDirection
        );

        nextDamageTime =
            Time.time +
            damageInterval;
    }
}