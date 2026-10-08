using UnityEngine;

public class MagicTrapDamageRing : MonoBehaviour
{
    [Header("Target")]
    public Transform player;

    [Header("Damage Ring")]
    public float radius = 3f;
    public float damagePerTick = 1f;
    public float damageInterval = 1f;

    private float nextDamageTime = 0f;

    void Update()
    {
        if (player == null)
            return;

        Vector3 ringPosition =
            transform.position;

        Vector3 playerPosition =
            player.position;

        ringPosition.y = 0f;
        playerPosition.y = 0f;

        float distance =
            Vector3.Distance(
                ringPosition,
                playerPosition
            );

        if (distance > radius)
            return;

        if (Time.time < nextDamageTime)
            return;

        PlayerHealth playerHealth =
            player.GetComponent<PlayerHealth>();

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