using UnityEngine;
using System.Collections;

public class MageAOEZone : MonoBehaviour
{
    [Header("AOE")]
    public float radius = 3f;
    public float warningTime = 1.2f;
    public float damage = 2f;

    [Header("Visual")]
    public Renderer zoneRenderer;

    public Color warningColor =
        new Color(0.7f, 0.2f, 1f, 1f);

    public Color dangerColor =
        Color.red;

    private Transform player;

    public void Setup(Transform target)
    {
        player = target;

        StartCoroutine(
            AOESequence()
        );
    }

    IEnumerator AOESequence()
    {
        // 初始警告颜色
        SetColor(warningColor);

        // 大部分时间保持预警色
        yield return new WaitForSeconds(
            warningTime * 0.7f
        );

        // 即将爆炸，变红
        SetColor(dangerColor);

        yield return new WaitForSeconds(
            warningTime * 0.3f
        );

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