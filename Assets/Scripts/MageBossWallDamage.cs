using UnityEngine;

public class MageBossWallDamage : MonoBehaviour
{
    [Header("Damage")]
    public float damagePerTick = 1f;
    public float damageInterval = 1f;

    private float nextDamageTime = 0f;

    private void OnTriggerStay(Collider other)
    {
        if (!other.CompareTag("Player"))
            return;

        if (Time.time < nextDamageTime)
            return;

        PlayerHealth playerHealth =
            other.GetComponent<PlayerHealth>();

        if (playerHealth == null)
        {
            playerHealth =
                other.GetComponentInParent<PlayerHealth>();
        }

        if (playerHealth == null)
            return;

        Vector3 knockbackDirection =
            other.transform.position -
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