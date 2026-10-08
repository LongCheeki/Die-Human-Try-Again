using UnityEngine;
using System.Collections;

public class MageEnemyAI : MonoBehaviour
{
    [Header("Target")]
    public Transform player;

    // =============================
    // MOVEMENT
    // =============================

    [Header("Movement")]
    public float moveSpeed = 2f;
    public float rotationSpeed = 8f;

    [Header("Combat Distance")]
    public float detectionRange = 15f;
    public float preferredDistance = 8f;
    public float tooCloseDistance = 3f;

    // =============================
    // NORMAL MAGIC
    // =============================

    [Header("Normal Magic Attack")]
    public GameObject magicProjectilePrefab;
    public Transform magicSpawnPoint;

    public float attackCooldown = 1.8f;
    public float normalChargeTime = 0.6f;

    // =============================
    // AOE
    // =============================

    [Header("AOE Attack")]
    public GameObject aoePrefab;
    public float aoeCooldown = 7f;
    public float aoeChargeTime = 1f;

    [Header("AOE Height")]
    public float aoeGroundY = 0.02f;

    // =============================
    // CHARGE COLOR
    // =============================

    [Header("Charge Visual")]
    public Renderer[] mageRenderers;

    public Color normalColor =
        Color.white;

    public Color normalChargeColor =
        new Color(
            0.2f,
            0.6f,
            1f,
            1f
        );

    public Color aoeChargeColor =
        new Color(
            1f,
            0.2f,
            0.8f,
            1f
        );

    // =============================
    // INTERNAL
    // =============================

    private float nextAttackTime = 0f;
    private float nextAOETime = 0f;

    private bool isCasting = false;

    void Start()
    {
        nextAOETime =
            Time.time +
            aoeCooldown;
    }

    void Update()
    {
        if (player == null)
            return;

        if (isCasting)
        {
            FacePlayer();
            return;
        }

        Vector3 flatDirection =
            player.position -
            transform.position;

        flatDirection.y = 0f;

        float distance =
            flatDirection.magnitude;

        if (distance > detectionRange)
            return;

        FacePlayer();

        // =========================
        // AOE
        // =========================

        if (Time.time >= nextAOETime)
        {
            StartCoroutine(
                CastAOE()
            );

            nextAOETime =
                Time.time +
                aoeCooldown;

            return;
        }

        // =========================
        // TOO CLOSE
        // =========================

        if (distance <
            tooCloseDistance)
        {
            MoveAwayFromPlayer();
            return;
        }

        // =========================
        // TOO FAR
        // =========================

        if (distance >
            preferredDistance)
        {
            MoveTowardsPlayer();
            return;
        }

        // =========================
        // NORMAL ATTACK
        // =========================

        if (Time.time >= nextAttackTime)
        {
            StartCoroutine(
                CastNormalMagic()
            );

            nextAttackTime =
                Time.time +
                attackCooldown;
        }
    }

    // =============================
    // NORMAL ATTACK
    // =============================

    IEnumerator CastNormalMagic()
    {
        isCasting = true;

        // 蓝色蓄力
        SetMageColor(
            normalChargeColor
        );

        yield return new WaitForSeconds(
            normalChargeTime
        );

        FireMagicProjectile();

        // 恢复
        SetMageColor(
            normalColor
        );

        isCasting = false;
    }

    void FireMagicProjectile()
    {
        if (magicProjectilePrefab == null ||
            magicSpawnPoint == null ||
            player == null)
            return;

        Vector3 targetPosition =
            player.position;

        Vector3 direction =
            targetPosition -
            magicSpawnPoint.position;

        GameObject projectile =
            Instantiate(
                magicProjectilePrefab,
                magicSpawnPoint.position,
                Quaternion.identity
            );

        MagicProjectile magic =
            projectile.GetComponent<
                MagicProjectile
            >();

        if (magic != null)
        {
            magic.SetDirection(
                direction
            );
        }
    }

    // =============================
    // AOE
    // =============================

    IEnumerator CastAOE()
    {
        isCasting = true;

        // 紫红色蓄力
        SetMageColor(
            aoeChargeColor
        );

        yield return new WaitForSeconds(
            aoeChargeTime
        );

        if (aoePrefab != null &&
            player != null)
        {
            // 锁定释放瞬间玩家的位置
            Vector3 spawnPosition =
                player.position;

            // 固定圆圈高度
            spawnPosition.y =
                aoeGroundY;

            GameObject zone =
                Instantiate(
                    aoePrefab,
                    spawnPosition,
                    Quaternion.identity
                );

            MageAOEZone aoe =
                zone.GetComponent<
                    MageAOEZone
                >();

            if (aoe != null)
            {
                aoe.Setup(
                    player
                );
            }
        }

        SetMageColor(
            normalColor
        );

        isCasting = false;
    }

    // =============================
    // MOVEMENT
    // =============================

    void MoveTowardsPlayer()
    {
        Vector3 direction =
            player.position -
            transform.position;

        direction.y = 0f;

        if (direction.sqrMagnitude <
            0.001f)
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

        if (direction.sqrMagnitude <
            0.001f)
            return;

        direction.Normalize();

        transform.position +=
            direction *
            moveSpeed *
            Time.deltaTime;
    }

    // =============================
    // ROTATION
    // =============================

    void FacePlayer()
    {
        Vector3 direction =
            player.position -
            transform.position;

        direction.y = 0f;

        if (direction.sqrMagnitude <
            0.001f)
            return;

        Quaternion targetRotation =
            Quaternion.LookRotation(
                direction
            );

        transform.rotation =
            Quaternion.Slerp(
                transform.rotation,
                targetRotation,
                rotationSpeed *
                Time.deltaTime
            );
    }

    // =============================
    // COLOR
    // =============================

    void SetMageColor(Color color)
    {
        if (mageRenderers == null)
            return;

        foreach (
            Renderer renderer
            in mageRenderers)
        {
            if (renderer == null)
                continue;

            renderer.material.color =
                color;
        }
    }
}