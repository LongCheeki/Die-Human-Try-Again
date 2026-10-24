using UnityEngine;

public class EnemyAI : MonoBehaviour
{
    [Header("Target")]
    public Transform player;

    [Header("Movement")]
    public float moveSpeed = 2.5f;

    [Header("Detection")]
    public float detectionRange = 8f;

    [Header("Attack")]
    public float attackRange = 1.5f;
    public float attackCooldown = 1.2f;
    public float attackDamage = 1f;
    public float attackRecovery = 0.8f;
    public bool directionalMelee;
    public float meleeReach = 1.15f;
    [Range(0f, 180f)] public float meleeHalfAngle = 50f;

    [Header("References")]
    public Transform visual;
    public Animator animator;

    private Rigidbody rb;
    private CharacterSfx sfx;
    private PlayerHealth playerHealth;

    private bool isChasing = false;
    private bool isAttacking = false;

    private float attackTimer = 0f;
    private BossShield shield;
    private bool shieldHeld;
    private bool hitThisAttack;

    void Awake()
    {
        sfx = CharacterSfx.Get(gameObject);
        shield = GetComponent<BossShield>();
        rb =
            GetComponent<Rigidbody>();
    }

    void Start()
    {
        ConfigureSwordMelee();
        if (player != null)
        {
            playerHealth =
                player.GetComponent
                    <PlayerHealth>();
        }
    }

    private void ConfigureSwordMelee()
    {
        if (animator == null) return;
        foreach (var renderer in animator.GetComponentsInChildren<SkinnedMeshRenderer>(true))
        {
            if (renderer.sharedMesh == null || renderer.sharedMesh.name != "WK_HeavyIntantry") continue;
            directionalMelee = true;
            var scale = renderer.transform.lossyScale;
            meleeReach = Mathf.Max(0.3f, 1.35f * Mathf.Max(Mathf.Abs(scale.x), Mathf.Abs(scale.z)));
            meleeHalfAngle = 50f;
            attackRange = meleeReach + 0.25f;
            attackRecovery = 1.05f;
            if (animator.runtimeAnimatorController != null)
                foreach (var clip in animator.runtimeAnimatorController.animationClips)
                    if (clip.name == "WK_heavy_infantry_08_attack_B")
                    {
                        attackRecovery = clip.length / Mathf.Max(0.1f, animator.speed) + 0.05f;
                        break;
                    }
            return;
        }
    }
    void Update()
    {
        if (Time.timeScale == 0f) return;
        if (HoldForShield()) return;
        if (player == null)
            return;

        float distance =
            Vector3.Distance(
                transform.position,
                player.position
            );

        // Attack cooldown
        if (attackTimer > 0f)
        {
            attackTimer -=
                Time.deltaTime;
        }

        // Detection
        isChasing =
            distance <=
            detectionRange;

        // Attack
        if (distance <= attackRange &&
            attackTimer <= 0f &&
            !isAttacking)
        {
            AttackPlayer();
        }

        UpdateAnimation();
    }

    void FixedUpdate()
    {
        if (shield != null && shield.HasShield) { StopMoving(); return; }
        if (player == null)
            return;

        if (isAttacking)
        {
            StopMoving();
            return;
        }

        if (isChasing)
        {
            ChasePlayer();
        }
        else
        {
            StopMoving();
        }
    }

    // =============================
    // CHASE
    // =============================

    void ChasePlayer()
    {
        Vector3 direction =
            player.position -
            transform.position;

        direction.y = 0f;

        float distance =
            direction.magnitude;

        if (distance <= attackRange)
        {
            StopMoving();

            FacePlayer(
                direction
            );

            return;
        }

        direction.Normalize();

        Vector3 velocity =
            direction *
            moveSpeed;

        rb.linearVelocity =
            new Vector3(
                velocity.x,
                rb.linearVelocity.y,
                velocity.z
            );

        FacePlayer(
            direction
        );
    }

    // =============================
    // FACE PLAYER
    // =============================

    void FacePlayer(
        Vector3 direction)
    {
        if (visual == null)
            return;

        direction.y = 0f;

        if (direction.sqrMagnitude <
            0.01f)
            return;

        visual.rotation =
            Quaternion.LookRotation(
                direction,
                Vector3.up
            );
    }

    // =============================
    // STOP
    // =============================

    void StopMoving()
    {
        if (rb == null)
            return;

        rb.linearVelocity =
            new Vector3(
                0f,
                rb.linearVelocity.y,
                0f
            );
    }

    // =============================
    // START ATTACK
    // =============================

    void AttackPlayer()
    {
        isAttacking = true;
        hitThisAttack = false;
        sfx.EnemyAttack();

        attackTimer =
            attackCooldown;

        StopMoving();

        Vector3 direction =
            player.position -
            transform.position;

        FacePlayer(
            direction
        );

        if (animator != null)
        {
            animator.SetTrigger(
                "Attack"
            );
        }

        // 不在这里扣血
        // Animation Event调用DealDamage

        Invoke(
            nameof(FinishAttack),
            attackRecovery
        );
    }

    // =============================
    // ANIMATION EVENT DAMAGE
    // =============================

    public void DealDamage()
    {
        if (shield != null && shield.HasShield) return;
        if (player == null ||
            playerHealth == null)
            return;

        if (directionalMelee && (!isAttacking || hitThisAttack)) return;
        float distance = Vector3.Distance(transform.position, player.position);
        if (directionalMelee ? CanHitPlayer() : distance <= attackRange + 0.3f)
        {
            // Enemy -> Player
            Vector3 knockbackDirection =
                player.position -
                transform.position;

            knockbackDirection.y =
                0f;

            hitThisAttack = true;
            playerHealth.TakeDamage(
                attackDamage,
                knockbackDirection
            );

            Debug.Log(
                "Enemy hit player!"
            );
        }
        else
        {
            Debug.Log(
                "Enemy attack missed!"
            );
        }
    }

    private bool CanHitPlayer()
    {
        var direction = player.position - transform.position;
        if (Mathf.Abs(direction.y) > 1.5f) return false;
        direction.y = 0f;
        var forward = visual != null ? visual.forward : transform.forward;
        forward.y = 0f;
        if (direction.sqrMagnitude > 0.0001f && Vector3.Angle(forward, direction) > meleeHalfAngle)
            return false;
        foreach (var collider in playerHealth.GetComponentsInChildren<Collider>())
        {
            if (!collider.enabled || collider.isTrigger) continue;
            var distance = collider.ClosestPoint(transform.position) - transform.position;
            distance.y = 0f;
            if (distance.sqrMagnitude <= meleeReach * meleeReach) return true;
        }
        return false;
    }
    void FinishAttack()
    {
        isAttacking =
            false;
    }

    // =============================
    // ANIMATION
    // =============================

    private bool HoldForShield()
    {
        if (shield == null || !shield.HasShield)
        {
            shieldHeld = false;
            return false;
        }
        isChasing = false;
        isAttacking = false;
        CancelInvoke(nameof(FinishAttack));
        StopMoving();
        if (animator != null)
        {
            animator.ResetTrigger("Attack");
            animator.SetBool("IsMoving", false);
            int idle = Animator.StringToHash("WK_heavy_infantry_05_combat_idle");
            if (!shieldHeld && animator.HasState(0, idle))
                animator.CrossFadeInFixedTime(idle, 0.1f);
        }
        shieldHeld = true;
        return true;
    }
    void UpdateAnimation()
    {
        if (animator == null ||
            rb == null)
            return;

        if (isAttacking)
        {
            animator.SetBool(
                "IsMoving",
                false
            );

            return;
        }

        Vector3 horizontalVelocity =
            new Vector3(
                rb.linearVelocity.x,
                0f,
                rb.linearVelocity.z
            );

        bool moving =
            horizontalVelocity
                .sqrMagnitude >
            0.01f;

        animator.SetBool(
            "IsMoving",
            moving
        );
    }
}