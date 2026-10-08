using UnityEngine;
using System.Collections;

public class MageBossAI : MonoBehaviour
{
    // =============================
    // TARGET
    // =============================

    [Header("Target")]
    public Transform player;

    // =============================
    // MOVEMENT
    // =============================

    [Header("Movement")]
    public float moveSpeed = 2f;
    public float rotationSpeed = 8f;

    public float detectionRange = 25f;
    public float preferredDistance = 10f;
    public float tooCloseDistance = 4f;

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

    [Header("AOE")]
    public GameObject aoePrefab;

    public float aoeCooldown = 8f;
    public float aoeChargeTime = 1f;

    public float aoeGroundY = 0.02f;

    // =============================
    // MAGIC WALL
    // =============================

    [Header("Magic Wall")]
    public GameObject magicWallPrefab;

    public float wallCooldown = 10f;

    public float wallSpawnDistance =
        3f;

    public float wallChargeTime =
        1f;

    // =============================
    // TELEPORT
    // =============================

    [Header("Teleport")]
    public Transform[] teleportPoints;

    public float teleportCooldown =
        7f;

    public float teleportChargeTime =
        0.3f;

    // =============================
    // VISUALS
    // =============================

    [Header("Boss Visual")]
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

    public Color wallChargeColor =
        Color.red;

    public Color teleportColor =
        new Color(
            0.5f,
            0f,
            1f,
            1f
        );

    // =============================
    // INTERNAL
    // =============================

    private float nextAttackTime;
    private float nextAOETime;
    private float nextWallTime;
    private float nextTeleportTime;

    private bool isCasting = false;

    // =============================
    // START
    // =============================

    void Start()
    {
        nextAttackTime =
            Time.time + 1f;

        nextAOETime =
            Time.time + aoeCooldown;

        nextWallTime =
            Time.time + wallCooldown;

        nextTeleportTime =
            Time.time + teleportCooldown;
    }

    // =============================
    // UPDATE
    // =============================

    void Update()
    {
        if (player == null)
            return;

        if (isCasting)
        {
            FacePlayer();
            return;
        }

        Vector3 direction =
            player.position -
            transform.position;

        direction.y = 0f;

        float distance =
            direction.magnitude;

        if (distance >
            detectionRange)
            return;

        FacePlayer();

        // =========================
        // TELEPORT
        // Highest priority
        // =========================

        if (Time.time >=
            nextTeleportTime)
        {
            StartCoroutine(
                Teleport()
            );

            nextTeleportTime =
                Time.time +
                teleportCooldown;

            return;
        }

        // =========================
        // MAGIC WALL
        // =========================

        if (Time.time >=
            nextWallTime)
        {
            StartCoroutine(
                CastMagicWall()
            );

            nextWallTime =
                Time.time +
                wallCooldown;

            return;
        }

        // =========================
        // AOE
        // =========================

        if (Time.time >=
            nextAOETime)
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
            nextAttackTime)
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
    // NORMAL MAGIC
    // =============================

    IEnumerator CastNormalMagic()
    {
        isCasting = true;

        SetMageColor(
            normalChargeColor
        );

        yield return new WaitForSeconds(
            normalChargeTime
        );

        FireMagicProjectile();

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

        Vector3 direction =
            player.position -
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

        SetMageColor(
            aoeChargeColor
        );

        yield return new WaitForSeconds(
            aoeChargeTime
        );

        if (aoePrefab != null &&
            player != null)
        {
            Vector3 spawnPosition =
                player.position;

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
    // MAGIC WALL
    // =============================

    IEnumerator CastMagicWall()
    {
        isCasting = true;

        // 红色 = 即将放墙
        SetMageColor(
            wallChargeColor
        );

        yield return new WaitForSeconds(
            wallChargeTime
        );

        if (magicWallPrefab != null &&
            player != null)
        {
            Vector3 direction =
                player.position -
                transform.position;

            direction.y = 0f;

            if (direction.sqrMagnitude >
                0.001f)
            {
                direction.Normalize();

                // 墙生成在Boss和玩家之间
                Vector3 spawnPosition =
                    transform.position +
                    direction *
                    wallSpawnDistance;

                spawnPosition.y =
                    transform.position.y;

                // 墙的Z轴朝玩家
                Quaternion wallRotation =
                    Quaternion.LookRotation(
                        direction
                    );

                GameObject wall =
                    Instantiate(
                        magicWallPrefab,
                        spawnPosition,
                        wallRotation
                    );

                MageBossWall wallScript =
                    wall.GetComponent<
                        MageBossWall
                    >();

                if (wallScript != null)
                {
                    wallScript.Setup(
                        player
                    );
                }
            }
        }

        SetMageColor(
            normalColor
        );

        isCasting = false;
    }

    // =============================
    // TELEPORT
    // =============================

    IEnumerator Teleport()
    {
        if (teleportPoints == null ||
            teleportPoints.Length == 0)
            yield break;

        isCasting = true;

        SetMageColor(
            teleportColor
        );

        yield return new WaitForSeconds(
            teleportChargeTime
        );

        Transform chosenPoint =
            ChooseTeleportPoint();

        if (chosenPoint != null)
        {
            transform.position =
                chosenPoint.position;

            transform.rotation =
                chosenPoint.rotation;
        }

        SetMageColor(
            normalColor
        );

        isCasting = false;
    }

    Transform ChooseTeleportPoint()
    {
        if (teleportPoints == null ||
            teleportPoints.Length == 0)
            return null;

        // 尽量不要闪到玩家脸上
        Transform bestPoint = null;

        float bestDistance = 0f;

        foreach (
            Transform point
            in teleportPoints)
        {
            if (point == null)
                continue;

            float distance =
                Vector3.Distance(
                    point.position,
                    player.position
                );

            if (distance >
                bestDistance)
            {
                bestDistance =
                    distance;

                bestPoint =
                    point;
            }
        }

        return bestPoint;
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

    void SetMageColor(
        Color color)
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