using UnityEngine;

public class ArcherEnemyAI : MonoBehaviour
{
    [Header("Target")]
    public Transform player;

    [Header("Movement")]
    public float moveSpeed = 2.5f;
    public float rotationSpeed = 8f;

    [Header("Ranges")]
    public float detectionRange = 15f;
    public float preferredDistance = 8f;
    public float tooCloseDistance = 2f;

    [Header("Attack")]
    public GameObject arrowPrefab;
    public Transform arrowSpawnPoint;
    public float attackCooldown = 1.5f;

    private float nextAttackTime = 0f;

    void Update()
    {
        if (player == null)
            return;

        // 只计算水平距离
        Vector3 flatDirection =
            player.position -
            transform.position;

        flatDirection.y = 0f;

        float distance =
            flatDirection.magnitude;

        // 玩家没进入侦测范围
        if (distance > detectionRange)
            return;

        // 始终朝向玩家
        FacePlayer();

        // 太近就往后退
        if (distance < tooCloseDistance)
        {
            MoveAwayFromPlayer();
            return;
        }

        // 太远就靠近
        if (distance > preferredDistance)
        {
            MoveTowardsPlayer();
            return;
        }

        // 到合适距离后开始射箭
        if (Time.time >= nextAttackTime)
        {
            ShootArrow();

            nextAttackTime =
                Time.time +
                attackCooldown;
        }
    }

    void MoveTowardsPlayer()
    {
        Vector3 direction =
            player.position -
            transform.position;

        direction.y = 0f;

        if (direction.sqrMagnitude < 0.001f)
            return;

        direction.Normalize();

        transform.position +=
            direction *
            moveSpeed *
            Time.deltaTime;
    }

    void MoveAwayFromPlayer()
    {
        Vector3 direction =
            transform.position -
            player.position;

        direction.y = 0f;

        if (direction.sqrMagnitude < 0.001f)
            return;

        direction.Normalize();

        transform.position +=
            direction *
            moveSpeed *
            Time.deltaTime;
    }

    void FacePlayer()
    {
        Vector3 direction =
            player.position -
            transform.position;

        direction.y = 0f;

        if (direction.sqrMagnitude < 0.001f)
            return;

        Quaternion targetRotation =
            Quaternion.LookRotation(direction);

        transform.rotation =
            Quaternion.Slerp(
                transform.rotation,
                targetRotation,
                rotationSpeed *
                Time.deltaTime
            );
    }

    void ShootArrow()
    {
        if (arrowPrefab == null)
        {
            Debug.LogWarning(
                "Archer: Arrow Prefab missing!"
            );

            return;
        }

        if (arrowSpawnPoint == null)
        {
            Debug.LogWarning(
                "Archer: Arrow Spawn Point missing!"
            );

            return;
        }

        if (player == null)
            return;

        Debug.Log("ARCHER FIRED ARROW");

        // 记录这一瞬间玩家的位置
        Vector3 targetPosition =
            player.position;

        Vector3 direction =
            targetPosition -
            arrowSpawnPoint.position;

        if (direction.sqrMagnitude < 0.001f)
            return;

        GameObject arrow =
            Instantiate(
                arrowPrefab,
                arrowSpawnPoint.position,
                Quaternion.identity
            );

        ArrowProjectile projectile =
            arrow.GetComponent<ArrowProjectile>();

        if (projectile != null)
        {
            projectile.SetDirection(direction);
        }
        else
        {
            Debug.LogWarning(
                "Arrow Prefab has no ArrowProjectile script!"
            );
        }
    }
}