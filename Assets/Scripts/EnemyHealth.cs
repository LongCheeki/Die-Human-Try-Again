using UnityEngine;
using System.Collections;

public class EnemyHealth : MonoBehaviour
{
    [Header("Health")]
    public int maxHealth = 3;

    [Header("Knockback")]
    public float knockbackSpeed = 8f;
    public float knockbackDuration = 0.25f;

    private float currentHealth;

    // =============================
    // PUBLIC HEALTH INFO
    // =============================

    public float GetCurrentHealth()
    {
        return currentHealth;
    }

    public float GetMaxHealth()
    {
        return maxHealth;
    }

    public float GetHealthPercent()
    {
        if (maxHealth <= 0)
            return 0f;

        return currentHealth / maxHealth;
    }

    // =============================
    // EVENTS
    // =============================

    public event System.Action<float> HealthChanged;
    public event System.Action Died;

    // =============================
    // REFERENCES
    // =============================

    private Rigidbody rb;
    private EnemyAI enemyAI;
    private GolelingBossAI bossAI;

    private bool isKnockedBack = false;

    // =============================
    // AWAKE
    // =============================

    void Awake()
    {
        rb = GetComponent<Rigidbody>();

        enemyAI =
            GetComponent<EnemyAI>();

        bossAI =
            GetComponent<GolelingBossAI>();
    }

    // =============================
    // START
    // =============================

    void Start()
    {
        currentHealth =
            maxHealth;

        HealthChanged?.Invoke(
            currentHealth
        );
    }

    // =============================
    // DAMAGE
    // =============================

    public void TakeDamage(
        float damage,
        Vector3 knockbackDirection,
        PlayerMimic attacker = null)
    {
        if (currentHealth <= 0f)
            return;

        // =========================
        // BOSS SHIELD
        // =========================

        BossShield shield =
            GetComponent<BossShield>();

        if (shield != null &&
            shield.AbsorbHit(attacker))
        {
            return;
        }

        // =========================
        // EXISTING GOLELING BOSS
        // =========================

        if (bossAI != null)
        {
            damage *=
                bossAI.GetIncomingDamageMultiplier();
        }

        // =========================
        // APPLY DAMAGE
        // =========================

        currentHealth =
            Mathf.Max(
                0f,
                currentHealth - damage
            );

        HealthChanged?.Invoke(
            currentHealth
        );

        Debug.Log(
            "Enemy HP: " +
            currentHealth +
            "/" +
            maxHealth
        );

        // =========================
        // DEATH
        // =========================

        if (currentHealth <= 0f)
        {
            Die();
            return;
        }

        // =========================
        // KNOCKBACK
        // =========================

        if (rb != null &&
            knockbackDirection != Vector3.zero &&
            knockbackSpeed > 0f)
        {
            StartCoroutine(
                KnockbackCoroutine(
                    knockbackDirection
                )
            );
        }
    }

    // =============================
    // KNOCKBACK
    // =============================

    IEnumerator KnockbackCoroutine(
        Vector3 direction)
    {
        if (isKnockedBack)
            yield break;

        if (rb == null)
            yield break;

        isKnockedBack = true;

        // ∆’Õ® EnemyAI ‘›Õ£
        if (enemyAI != null)
        {
            enemyAI.enabled =
                false;
        }

        direction.y = 0f;
        direction.Normalize();

        float timer = 0f;

        while (
            timer <
            knockbackDuration)
        {
            timer +=
                Time.fixedDeltaTime;

            float strength =
                1f -
                (timer /
                 knockbackDuration);

            Vector3 velocity =
                direction *
                knockbackSpeed *
                strength;

            rb.linearVelocity =
                new Vector3(
                    velocity.x,
                    rb.linearVelocity.y,
                    velocity.z
                );

            yield return
                new WaitForFixedUpdate();
        }

        rb.linearVelocity =
            new Vector3(
                0f,
                rb.linearVelocity.y,
                0f
            );

        if (enemyAI != null)
        {
            enemyAI.enabled =
                true;
        }

        isKnockedBack = false;
    }

    // =============================
    // DEATH
    // =============================

    void Die()
    {
        Debug.Log(
            "Enemy Dead"
        );

        if (enemyAI != null)
        {
            enemyAI.enabled =
                false;
        }

        Died?.Invoke();

        Destroy(
            gameObject
        );
    }
}