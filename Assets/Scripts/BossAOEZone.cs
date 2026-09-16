using UnityEngine;
using System.Collections;

public class BossAOEZone : MonoBehaviour
{
    [Header("AOE")]
    public float radius = 4f;

    public float orangeWarningTime = 2f;
    public float redWarningTime = 0.6f;

    public float damage = 3f;

    [Header("Visual")]
    public Renderer zoneRenderer;

    public Color orangeColor =
        new Color(
            1f,
            0.5f,
            0f,
            1f
        );

    public Color redColor =
        new Color(
            1f,
            0f,
            0f,
            1f
        );

    private Transform player;

    public void Setup(Transform target)
    {
        player = target;

        StartCoroutine(
            WarningSequence()
        );
    }

    IEnumerator WarningSequence()
    {
        // =========================
        // ORANGE WARNING
        // =========================

        SetColor(
            orangeColor
        );

        yield return new WaitForSeconds(
            orangeWarningTime
        );

        // =========================
        // RED WARNING
        // =========================

        SetColor(
            redColor
        );

        yield return new WaitForSeconds(
            redWarningTime
        );

        // =========================
        // DAMAGE
        // =========================

        DealDamage();

        Destroy(gameObject);
    }

    void DealDamage()
    {
        if (player == null)
            return;

        Vector3 zonePosition =
            transform.position;

        Vector3 playerPosition =
            player.position;

        // 只计算地面距离
        zonePosition.y = 0f;
        playerPosition.y = 0f;

        float distance =
            Vector3.Distance(
                zonePosition,
                playerPosition
            );

        if (distance <= radius)
        {
            PlayerHealth playerHealth =
                player.GetComponent<PlayerHealth>();

            if (playerHealth != null)
            {
                Vector3 knockbackDirection =
                    player.position -
                    transform.position;

                knockbackDirection.y = 0f;

                playerHealth.TakeDamage(
                    damage,
                    knockbackDirection
                );
            }
        }
    }

    void SetColor(Color color)
    {
        if (zoneRenderer == null)
            return;

        zoneRenderer.material.color =
            color;
    }
}