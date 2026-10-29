using UnityEngine;
using System.Collections;

public class KingBossAI : MonoBehaviour
{
    // =====================================================
    // REFERENCES
    // =====================================================

    [Header("References")]
    public Transform player;
    public EnemyHealth enemyHealth;
    public Animator animator;

    public Transform attackPoint;
    public Transform magicSpawnPoint;
    public Transform auraSpawnPoint;

    // =====================================================
    // MOVEMENT
    // =====================================================

    [Header("Movement")]
    public float moveSpeed = 3f;
    public float rotationSpeed = 8f;
    public float detectionRange = 30f;
    public float meleeRange = 2.2f;

    // =====================================================
    // MELEE
    // =====================================================

    [Header("Melee Attack")]
    public float meleeDamage = 2f;
    public float meleeRadius = 2.2f;
    public float meleeCooldown = 1.4f;
    public float meleeHitDelay = 0.45f;

    // =====================================================
    // ROYAL CHARGE
    // =====================================================

    [Header("Royal Charge")]
    public float chargeCooldown = 8f;
    public float chargeWarningTime = 0.8f;
    public float chargeSpeed = 12f;
    public float chargeDuration = 0.7f;
    public float chargeDamage = 2f;
    public float chargeHitDistance = 1.4f;

    // =====================================================
    // GROUND SLAM
    // =====================================================

    [Header("Ground Slam")]
    public GameObject aoePrefab;
    public float slamCooldown = 7f;
    public float slamChargeTime = 0.8f;

    // =====================================================
    // ROYAL AURA
    // =====================================================

    [Header("Royal Aura")]
    public GameObject damageAuraPrefab;
    public float auraCooldown = 10f;
    public float auraWarningTime = 0.8f;

    // =====================================================
    // MAGIC
    // =====================================================

    [Header("Magic Attack")]
    public GameObject magicProjectilePrefab;
    public float magicCooldown = 2.2f;
    public float magicChargeTime = 0.6f;

    // =====================================================
    // ARROW VOLLEY
    // =====================================================

    [Header("Arrow Volley")]
    public GameObject arrowPrefab;
    public int volleyArrowCount = 7;
    public float volleyAngle = 70f;
    public float volleyCooldown = 7f;
    public float volleyChargeTime = 1f;

    // =====================================================
    // TELEPORT
    // =====================================================

    [Header("Teleport")]
    public Transform[] teleportPoints;
    public float teleportCooldown = 9f;
    public float teleportChargeTime = 0.6f;

    // =====================================================
    // KING'S JUDGMENT
    // =====================================================

    [Header("King's Judgment")]
    public Transform[] judgementPoints;
    public float judgementCooldown = 12f;
    public float judgementChargeTime = 1.2f;
    public float judgementSecondWaveDelay = 1f;

    // =====================================================
    // VISUAL WARNING
    // =====================================================

    [Header("Boss Visual")]
    public Renderer[] bossRenderers;

    public Color normalColor = Color.white;
    public Color chargeColor = Color.red;

    public Color magicColor =
        new Color(0.2f, 0.6f, 1f, 1f);

    public Color auraColor =
        new Color(1f, 0.5f, 0f, 1f);

    public Color judgementColor =
        new Color(1f, 0.8f, 0.1f, 1f);

    // =====================================================
    // ANIMATION STATE NAMES
    // =====================================================

    [Header("Animation State Names")]

    public string idleAnimation = "idle1";

    public string walkAnimation = "walk";

    public string runAnimation = "run";

    public string attack1Animation = "atack1";

    public string attack2Animation = "atack2";

    public string attack3Animation = "atack3";

    public string heavyAttackAnimation =
        "atack shield";

    // =====================================================
    // INTERNAL
    // =====================================================

    private bool isBusy = false;

    private int currentPhase = 1;

    private float nextMeleeTime;
    private float nextChargeTime;
    private float nextSlamTime;
    private float nextAuraTime;
    private float nextMagicTime;
    private float nextVolleyTime;
    private float nextTeleportTime;
    private float nextJudgementTime;

    // =====================================================
    // START
    // =====================================================

    void Start()
    {
        if (enemyHealth == null)
        {
            enemyHealth =
                GetComponent<EnemyHealth>();
        }

        nextMeleeTime =
            Time.time + 1f;

        nextChargeTime =
            Time.time + chargeCooldown;

        nextSlamTime =
            Time.time + slamCooldown;

        nextAuraTime =
            Time.time + auraCooldown;

        nextMagicTime =
            Time.time + magicCooldown;

        nextVolleyTime =
            Time.time + volleyCooldown;

        nextTeleportTime =
            Time.time + teleportCooldown;

        nextJudgementTime =
            Time.time + judgementCooldown;

        SetBossColor(normalColor);
    }

    // =====================================================
    // UPDATE
    // =====================================================

    void Update()
    {
        if (player == null)
            return;

        if (enemyHealth == null)
            return;

        UpdatePhase();

        if (isBusy)
            return;

        Vector3 direction =
            player.position -
            transform.position;

        direction.y = 0f;

        float distance =
            direction.magnitude;

        if (distance > detectionRange)
        {
            PlayAnimation(idleAnimation);

            return;
        }

        FacePlayer();

        // =================================================
        // PHASE 3 - KING'S JUDGMENT
        // =================================================

        if (currentPhase >= 3 &&
            Time.time >= nextJudgementTime)
        {
            StartCoroutine(
                KingsJudgment()
            );

            nextJudgementTime =
                Time.time +
                judgementCooldown;

            return;
        }

        // =================================================
        // ROYAL AURA
        // =================================================

        if (Time.time >= nextAuraTime)
        {
            StartCoroutine(
                ActivateAura()
            );

            nextAuraTime =
                Time.time +
                GetAuraCooldown();

            return;
        }

        // =================================================
        // PHASE 2+ TELEPORT
        // =================================================

        if (currentPhase >= 2 &&
            Time.time >= nextTeleportTime)
        {
            StartCoroutine(
                Teleport()
            );

            nextTeleportTime =
                Time.time +
                teleportCooldown;

            return;
        }

        // =================================================
        // CHARGE
        // =================================================

        if (Time.time >= nextChargeTime &&
            distance > meleeRange + 1f)
        {
            StartCoroutine(
                RoyalCharge()
            );

            nextChargeTime =
                Time.time +
                chargeCooldown;

            return;
        }

        // =================================================
        // GROUND SLAM
        // =================================================

        if (Time.time >= nextSlamTime &&
            distance < meleeRange + 2f)
        {
            StartCoroutine(
                GroundSlam()
            );

            nextSlamTime =
                Time.time +
                slamCooldown;

            return;
        }

        // =================================================
        // PHASE 2+ ARROW VOLLEY
        // =================================================

        if (currentPhase >= 2 &&
            Time.time >= nextVolleyTime)
        {
            StartCoroutine(
                ArrowVolley()
            );

            nextVolleyTime =
                Time.time +
                GetVolleyCooldown();

            return;
        }

        // =================================================
        // PHASE 2+ MAGIC
        // =================================================

        if (currentPhase >= 2 &&
            Time.time >= nextMagicTime)
        {
            StartCoroutine(
                CastMagic()
            );

            nextMagicTime =
                Time.time +
                GetMagicCooldown();

            return;
        }

        // =================================================
        // MELEE
        // =================================================

        if (distance <= meleeRange)
        {
            if (Time.time >= nextMeleeTime)
            {
                StartCoroutine(
                    MeleeAttack()
                );

                nextMeleeTime =
                    Time.time +
                    GetMeleeCooldown();
            }

            return;
        }

        // =================================================
        // MOVE
        // =================================================

        MoveTowardsPlayer();
    }

    // =====================================================
    // PHASE SYSTEM
    // =====================================================

    void UpdatePhase()
    {
        float currentHealth =
            enemyHealth.GetCurrentHealth();

        float maxHealth =
            enemyHealth.GetMaxHealth();

        if (maxHealth <= 0f)
            return;

        float healthPercent =
            currentHealth /
            maxHealth;

        int newPhase;

        // 40 - 29 HP
        if (healthPercent > 0.70f)
        {
            newPhase = 1;
        }

        // 28 - 15 HP
        else if (healthPercent > 0.35f)
        {
            newPhase = 2;
        }

        // 14 - 0 HP
        else
        {
            newPhase = 3;
        }

        if (newPhase ==
            currentPhase)
        {
            return;
        }

        currentPhase =
            newPhase;

        Debug.Log(
            "KING PHASE " +
            currentPhase
        );
    }

    // =====================================================
    // MELEE
    // =====================================================

    IEnumerator MeleeAttack()
    {
        isBusy = true;

        FacePlayer();

        int attackNumber =
            Random.Range(1, 4);

        if (attackNumber == 1)
        {
            PlayAnimation(
                attack1Animation
            );
        }
        else if (attackNumber == 2)
        {
            PlayAnimation(
                attack2Animation
            );
        }
        else
        {
            PlayAnimation(
                attack3Animation
            );
        }

        yield return new WaitForSeconds(
            meleeHitDelay
        );

        DealMeleeDamage();

        yield return new WaitForSeconds(
            0.35f
        );

        isBusy = false;
    }

    void DealMeleeDamage()
    {
        if (attackPoint == null)
            return;

        Collider[] hits =
            Physics.OverlapSphere(
                attackPoint.position,
                meleeRadius
            );

        foreach (
            Collider hit
            in hits)
        {
            PlayerHealth playerHealth =
                hit.GetComponentInParent<
                    PlayerHealth
                >();

            if (playerHealth == null)
                continue;

            Vector3 knockback =
                playerHealth.transform.position -
                transform.position;

            knockback.y = 0f;

            playerHealth.TakeDamage(
                meleeDamage,
                knockback
            );

            break;
        }
    }

    // =====================================================
    // ROYAL CHARGE
    // =====================================================

    IEnumerator RoyalCharge()
    {
        isBusy = true;

        SetBossColor(
            chargeColor
        );

        FacePlayer();

        PlayAnimation(
            runAnimation
        );

        yield return new WaitForSeconds(
            chargeWarningTime
        );

        Vector3 chargeDirection =
            player.position -
            transform.position;

        chargeDirection.y = 0f;

        if (chargeDirection.sqrMagnitude <
            0.001f)
        {
            SetBossColor(
                normalColor
            );

            isBusy = false;

            yield break;
        }

        chargeDirection.Normalize();

        Quaternion targetRotation =
            Quaternion.LookRotation(
                chargeDirection
            );

        transform.rotation =
            targetRotation;

        float timer = 0f;

        bool damagedPlayer =
            false;

        while (
            timer <
            chargeDuration)
        {
            timer +=
                Time.deltaTime;

            transform.position +=
                chargeDirection *
                chargeSpeed *
                Time.deltaTime;

            if (!damagedPlayer)
            {
                Vector3 bossPosition =
                    transform.position;

                Vector3 playerPosition =
                    player.position;

                bossPosition.y = 0f;
                playerPosition.y = 0f;

                float distance =
                    Vector3.Distance(
                        bossPosition,
                        playerPosition
                    );

                if (distance <=
                    chargeHitDistance)
                {
                    PlayerHealth playerHealth =
                        player.GetComponent<
                            PlayerHealth
                        >();

                    if (playerHealth == null)
                    {
                        playerHealth =
                            player.GetComponentInParent<
                                PlayerHealth
                            >();
                    }

                    if (playerHealth != null)
                    {
                        playerHealth.TakeDamage(
                            chargeDamage,
                            chargeDirection
                        );

                        damagedPlayer =
                            true;
                    }
                }
            }

            yield return null;
        }

        SetBossColor(
            normalColor
        );

        isBusy = false;
    }

    // =====================================================
    // GROUND SLAM
    // =====================================================

    IEnumerator GroundSlam()
    {
        isBusy = true;

        SetBossColor(
            chargeColor
        );

        FacePlayer();

        PlayAnimation(
            heavyAttackAnimation
        );

        yield return new WaitForSeconds(
            slamChargeTime
        );

        if (aoePrefab != null)
        {
            Vector3 spawnPosition =
                transform.position;

            GameObject aoeObject =
                Instantiate(
                    aoePrefab,
                    spawnPosition,
                    Quaternion.identity
                );

            MageAOEZone aoe =
                aoeObject.GetComponent<
                    MageAOEZone
                >();

            if (aoe != null)
            {
                aoe.Setup(
                    player
                );
            }
        }

        SetBossColor(
            normalColor
        );

        yield return new WaitForSeconds(
            0.3f
        );

        isBusy = false;
    }

    // =====================================================
    // ROYAL AURA
    // =====================================================

    IEnumerator ActivateAura()
    {
        isBusy = true;

        SetBossColor(
            auraColor
        );

        yield return new WaitForSeconds(
            auraWarningTime
        );

        if (damageAuraPrefab != null)
        {
            Transform spawnPoint =
                auraSpawnPoint != null
                    ? auraSpawnPoint
                    : transform;

            GameObject auraObject =
                Instantiate(
                    damageAuraPrefab,
                    spawnPoint.position,
                    Quaternion.identity,
                    transform
                );

            KingDamageAura aura =
                auraObject.GetComponent<
                    KingDamageAura
                >();

            if (aura != null)
            {
                aura.Setup(
                    player
                );
            }
        }

        SetBossColor(
            normalColor
        );

        isBusy = false;
    }

    // =====================================================
    // MAGIC
    // =====================================================

    IEnumerator CastMagic()
    {
        isBusy = true;

        SetBossColor(
            magicColor
        );

        FacePlayer();

        yield return new WaitForSeconds(
            magicChargeTime
        );

        if (magicProjectilePrefab != null &&
            magicSpawnPoint != null)
        {
            Vector3 direction =
                player.position -
                magicSpawnPoint.position;

            direction.y = 0f;

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

        SetBossColor(
            normalColor
        );

        isBusy = false;
    }

    // =====================================================
    // ARROW VOLLEY
    // =====================================================

    IEnumerator ArrowVolley()
    {
        isBusy = true;

        SetBossColor(
            chargeColor
        );

        FacePlayer();

        yield return new WaitForSeconds(
            volleyChargeTime
        );

        FireArrowVolley();

        SetBossColor(
            normalColor
        );

        isBusy = false;
    }

    void FireArrowVolley()
    {
        if (arrowPrefab == null)
            return;

        if (magicSpawnPoint == null)
            return;

        if (volleyArrowCount <= 0)
            return;

        Vector3 baseDirection =
            player.position -
            magicSpawnPoint.position;

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
                angleStep *
                i;

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
                    magicSpawnPoint.position,
                    Quaternion.identity
                );

            ArrowProjectile projectile =
                arrow.GetComponent<
                    ArrowProjectile
                >();

            if (projectile != null)
            {
                projectile.SetDirection(
                    direction
                );
            }
        }
    }

    // =====================================================
    // TELEPORT
    // =====================================================

    IEnumerator Teleport()
    {
        if (teleportPoints == null ||
            teleportPoints.Length == 0)
        {
            yield break;
        }

        isBusy = true;

        SetBossColor(
            magicColor
        );

        yield return new WaitForSeconds(
            teleportChargeTime
        );

        Transform targetPoint =
            FindBestTeleportPoint();

        if (targetPoint != null)
        {
            transform.position =
                targetPoint.position;

            FacePlayer();
        }

        SetBossColor(
            normalColor
        );

        isBusy = false;
    }

    Transform FindBestTeleportPoint()
    {
        Transform bestPoint =
            null;

        float bestDistance =
            -1f;

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

    // =====================================================
    // KING'S JUDGMENT
    // =====================================================

    IEnumerator KingsJudgment()
    {
        if (judgementPoints == null ||
            judgementPoints.Length == 0)
        {
            yield break;
        }

        isBusy = true;

        SetBossColor(
            judgementColor
        );

        FacePlayer();

        PlayAnimation(
            heavyAttackAnimation
        );

        yield return new WaitForSeconds(
            judgementChargeTime
        );

        // 第一波
        // 0 2 4 6
        for (
            int i = 0;
            i < judgementPoints.Length;
            i += 2)
        {
            SpawnJudgementAOE(
                judgementPoints[i]
            );
        }

        yield return new WaitForSeconds(
            judgementSecondWaveDelay
        );

        // 第二波
        // 1 3 5 7
        for (
            int i = 1;
            i < judgementPoints.Length;
            i += 2)
        {
            SpawnJudgementAOE(
                judgementPoints[i]
            );
        }

        SetBossColor(
            normalColor
        );

        isBusy = false;
    }

    void SpawnJudgementAOE(
        Transform point)
    {
        if (point == null)
            return;

        if (aoePrefab == null)
            return;

        GameObject aoeObject =
            Instantiate(
                aoePrefab,
                point.position,
                Quaternion.identity
            );

        MageAOEZone aoe =
            aoeObject.GetComponent<
                MageAOEZone
            >();

        if (aoe != null)
        {
            aoe.Setup(
                player
            );
        }
    }

    // =====================================================
    // MOVEMENT
    // =====================================================

    void MoveTowardsPlayer()
    {
        Vector3 direction =
            player.position -
            transform.position;

        direction.y = 0f;

        if (direction.sqrMagnitude <
            0.001f)
        {
            return;
        }

        direction.Normalize();

        float currentMoveSpeed =
            moveSpeed;

        // Phase 3 更快
        if (currentPhase == 3)
        {
            currentMoveSpeed *=
                1.2f;
        }

        transform.position +=
            direction *
            currentMoveSpeed *
            Time.deltaTime;

        PlayAnimation(
            walkAnimation
        );
    }

    // =====================================================
    // FACE PLAYER
    // =====================================================

    void FacePlayer()
    {
        if (player == null)
            return;

        Vector3 direction =
            player.position -
            transform.position;

        direction.y = 0f;

        if (direction.sqrMagnitude <
            0.001f)
        {
            return;
        }

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

    // =====================================================
    // PHASE SPEED
    // =====================================================

    float GetMeleeCooldown()
    {
        if (currentPhase == 3)
        {
            return meleeCooldown *
                   0.7f;
        }

        return meleeCooldown;
    }

    float GetMagicCooldown()
    {
        if (currentPhase == 3)
        {
            return magicCooldown *
                   0.75f;
        }

        return magicCooldown;
    }

    float GetVolleyCooldown()
    {
        if (currentPhase == 3)
        {
            return volleyCooldown *
                   0.75f;
        }

        return volleyCooldown;
    }

    float GetAuraCooldown()
    {
        if (currentPhase == 3)
        {
            return auraCooldown *
                   0.65f;
        }

        return auraCooldown;
    }

    // =====================================================
    // COLOR
    // =====================================================

    void SetBossColor(
        Color color)
    {
        if (bossRenderers == null)
            return;

        foreach (
            Renderer renderer
            in bossRenderers)
        {
            if (renderer == null)
                continue;

            renderer.material.color =
                color;
        }
    }

    // =====================================================
    // ANIMATION
    // =====================================================

    void PlayAnimation(
        string stateName)
    {
        if (animator == null)
            return;

        if (string.IsNullOrEmpty(
            stateName))
        {
            return;
        }

        AnimatorStateInfo stateInfo =
            animator.GetCurrentAnimatorStateInfo(
                0
            );

        // 已经在播放这个动画就不要每帧重新播放
        if (stateInfo.IsName(
            stateName))
        {
            return;
        }

        animator.CrossFade(
            stateName,
            0.1f
        );
    }

    // =====================================================
    // DEBUG
    // =====================================================

    void OnDrawGizmosSelected()
    {
        if (attackPoint == null)
            return;

        Gizmos.DrawWireSphere(
            attackPoint.position,
            meleeRadius
        );
    }
}