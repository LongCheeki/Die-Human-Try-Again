using UnityEngine;
using System.Collections;

public class ArcherBossAI : MonoBehaviour
{
    [Header("Target")]
    public Transform player;

    // =============================
    // MOVEMENT
    // =============================

    [Header("Movement")]
    public float moveSpeed = 2.5f;
    public float rotationSpeed = 8f;

    [Header("Combat Distance")]
    public float detectionRange = 20f;
    public float preferredDistance = 8f;
    public float tooCloseDistance = 3f;

    // =============================
    // NORMAL ATTACK
    // =============================

    [Header("Normal Attack")]
    public GameObject arrowPrefab;
    public Transform arrowSpawnPoint;
    public float normalAttackCooldown = 1.2f;

    // =============================
    // VOLLEY
    // =============================

    [Header("Arrow Volley")]
    public int attacksBeforeVolley = 5;
    public int volleyArrowCount = 7;
    public float volleyAngle = 70f;
    public float volleyChargeTime = 1f;

    // =============================
    // AOE
    // =============================

    [Header("AOE")]
    public GameObject aoePrefab;
    public float aoeCooldown = 8f;

    // =============================
    // VISUAL
    // =============================

    [Header("Boss Visual")]
    public Renderer bossRenderer;

    public Color normalColor =
        Color.white;

    public Color chargeColor =
        Color.red;

    // =============================
    // INTERNAL
    // =============================

    private int normalAttackCount = 0;

    private float nextNormalAttackTime = 0f;
    private float nextAOETime = 0f;

    private bool isUsingSpecial = false;

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

        // 特殊技能期间不移动
        if (isUsingSpecial)
            return;

        Vector3 flatDirection =
            player.position -
            transform.position;

        flatDirection.y = 0f;

        float distance =
            flatDirection.magnitude;

        // 玩家太远，不管
        if (distance > detectionRange)
            return;

        FacePlayer();

        // =========================
        // AOE优先
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
        // MOVEMENT
        // =========================

        if (distance <
            tooCloseDistance)
        {
            MoveAwayFromPlayer();
            return;
        }

        if (distance >
            preferredDistance)
        {
            MoveTowardsPlayer();
            return;
        }

        // =========================
        // NORMAL ATTACK
        // =========================

        if (Time.time >=
            nextNormalAttackTime)
        {
            NormalAttack();

            nextNormalAttackTime =
                Time.time +
                normalAttackCooldown;
        }
    }

    // =============================
    // MOVE TOWARDS
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

    // =============================
    // MOVE AWAY
    // =============================

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
    // NORMAL ATTACK
    // =============================

    void NormalAttack()
    {
        FireArrowAtPlayer();

        normalAttackCount++;

        Debug.Log(
            "Boss attack: " +
            normalAttackCount +
            "/" +
            attacksBeforeVolley
        );

        if (normalAttackCount >=
            attacksBeforeVolley)
        {
            normalAttackCount = 0;

            StartCoroutine(
                VolleyAttack()
            );
        }
    }

    // =============================
    // FIRE ONE ARROW
    // =============================

    void FireArrowAtPlayer()
    {
        if (arrowPrefab == null ||
            arrowSpawnPoint == null)
            return;

        Vector3 direction =
            player.position -
            arrowSpawnPoint.position;

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
            projectile.SetDirection(
                direction
            );
        }
    }

    // =============================
    // VOLLEY ATTACK
    // =============================

    IEnumerator VolleyAttack()
    {
        isUsingSpecial = true;

        SetBossColor(
            chargeColor
        );

        Debug.Log(
            "Boss charging volley!"
        );

        yield return new WaitForSeconds(
            volleyChargeTime
        );

        FireArrowVolley();

        SetBossColor(
            normalColor
        );

        isUsingSpecial = false;
    }

    // =============================
    // FIRE VOLLEY
    // =============================

    void FireArrowVolley()
    {
        if (arrowPrefab == null ||
            arrowSpawnPoint == null)
            return;

        Vector3 baseDirection =
            player.position -
            arrowSpawnPoint.position;

        baseDirection.y = 0f;

        if (baseDirection.sqrMagnitude <
            0.001f)
            return;

        baseDirection.Normalize();

        float startAngle =
            -volleyAngle / 2f;

        float angleStep = 0f;

        if (volleyArrowCount > 1)
        {
            angleStep =
                volleyAngle /
                (volleyArrowCount - 1);
        }

        for (
            int i = 0;
            i < volleyArrowCount;
            i++)
        {
            float angle =
                startAngle +
                angleStep * i;

            Vector3 direction =
                Quaternion.Euler(
                    0f,
                    angle,
                    0f
                ) *
                baseDirection;

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
                projectile.SetDirection(
                    direction
                );
            }
        }

        Debug.Log(
            "Boss fired volley!"
        );
    }

    // =============================
    // AOE
    // =============================

    IEnumerator CastAOE()
    {
        isUsingSpecial = true;

        if (aoePrefab != null)
        {
            Vector3 spawnPosition =
                player.position;

            // 这里就是你之前调圈高度的位置
            spawnPosition.y += -0.7f;

            GameObject zone =
                Instantiate(
                    aoePrefab,
                    spawnPosition,
                    Quaternion.identity
                );

            BossAOEZone aoe =
                zone.GetComponent<
                    BossAOEZone
                >();

            if (aoe != null)
            {
                aoe.Setup(
                    player
                );
            }
        }

        yield return new WaitForSeconds(
            0.5f
        );

        isUsingSpecial = false;
    }

    // =============================
    // FACE PLAYER
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

    void SetBossColor(Color color)
    {
        if (bossRenderer == null)
            return;

        bossRenderer.material.color =
            color;
    }
}