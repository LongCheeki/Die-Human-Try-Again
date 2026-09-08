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

    [Header("References")]
    public Transform visual;
    public Animator animator;

    private Rigidbody rb;
    private PlayerHealth playerHealth;

    private bool isChasing = false;
    private bool isAttacking = false;

    private float attackTimer = 0f;

    void Awake()
    {
        rb =
            GetComponent<Rigidbody>();
    }

    void Start()
    {
        if (player != null)
        {
            playerHealth =
                player.GetComponent
                    <PlayerHealth>();
        }
    }

    void Update()
    {
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
            0.8f
        );
    }

    // =============================
    // ANIMATION EVENT DAMAGE
    // =============================

    public void DealDamage()
    {
        if (player == null ||
            playerHealth == null)
            return;

        float distance =
            Vector3.Distance(
                transform.position,
                player.position
            );

        if (distance <=
            attackRange + 0.3f)
        {
            // Enemy -> Player
            Vector3 knockbackDirection =
                player.position -
                transform.position;

            knockbackDirection.y =
                0f;

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

    void FinishAttack()
    {
        isAttacking =
            false;
    }

    // =============================
    // ANIMATION
    // =============================

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