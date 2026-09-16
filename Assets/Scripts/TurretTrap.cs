using UnityEngine;

public class TurretTrap : MonoBehaviour
{
    [Header("Target")]
    public Transform player;

    [Header("Shooting")]
    public GameObject projectilePrefab;
    public Transform firePoint;

    public float detectionRange = 12f;
    public float fireCooldown = 2f;

    [Header("Rotation")]
    public Transform turretVisual;

    public float rotationSpeed = 180f;

    [Tooltip("如果炮口朝向和玩家方向有偏差，可以试 90、-90 或 180")]
    public float rotationOffset = 0f;

    private float nextFireTime;

    void Update()
    {
        if (player == null)
            return;

        float distance =
            Vector3.Distance(
                transform.position,
                player.position
            );

        // 玩家不在范围内就不工作
        if (distance > detectionRange)
            return;

        RotateTowardsPlayer();

        if (Time.time >= nextFireTime)
        {
            Fire();

            nextFireTime =
                Time.time +
                fireCooldown;
        }
    }

    // =========================
    // ROTATE TOWARDS PLAYER
    // =========================

    void RotateTowardsPlayer()
    {
        if (turretVisual == null)
            return;

        Vector3 direction =
            player.position -
            turretVisual.position;

        // 只在水平面旋转
        direction.y = 0f;

        if (direction.sqrMagnitude < 0.001f)
            return;

        // 算出玩家相对炮塔的水平角度
        float targetAngle =
            Mathf.Atan2(
                direction.x,
                direction.z
            ) * Mathf.Rad2Deg;

        targetAngle += rotationOffset;

        Quaternion targetRotation =
            Quaternion.Euler(
                0f,
                targetAngle,
                0f
            );

        // 平滑旋转
        turretVisual.rotation =
            Quaternion.RotateTowards(
                turretVisual.rotation,
                targetRotation,
                rotationSpeed *
                Time.deltaTime
            );
    }

    // =========================
    // FIRE
    // =========================

    void Fire()
    {
        if (projectilePrefab == null)
        {
            Debug.LogWarning(
                "TurretTrap: Projectile Prefab is missing!"
            );

            return;
        }

        if (firePoint == null)
        {
            Debug.LogWarning(
                "TurretTrap: Fire Point is missing!"
            );

            return;
        }

        if (player == null)
            return;

        // 只记录开火这一瞬间玩家的位置
        Vector3 targetPosition =
            player.position;

        Vector3 direction =
            targetPosition -
            firePoint.position;

        direction.y = 0f;

        GameObject projectile =
            Instantiate(
                projectilePrefab,
                firePoint.position,
                Quaternion.identity
            );

        TrapProjectile projectileScript =
            projectile.GetComponent<TrapProjectile>();

        if (projectileScript != null)
        {
            projectileScript.SetDirection(
                direction
            );
        }
        else
        {
            Debug.LogWarning(
                "TurretTrap: Projectile has no TrapProjectile script!"
            );
        }
    }
}