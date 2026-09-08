using UnityEngine;
using System.Collections;

public class EnemyHealth : MonoBehaviour
{
    [Header("Health")]
    public int maxHealth = 3;

    [Header("Knockback")]
    public float knockbackSpeed = 8f;
    public float knockbackDuration = 0.25f;

    private int currentHealth;

    private Rigidbody rb;
    private EnemyAI enemyAI;

    private bool isKnockedBack = false;

    void Awake()
    {
        rb = GetComponent<Rigidbody>();
        enemyAI = GetComponent<EnemyAI>();
    }

    void Start()
    {
        currentHealth = maxHealth;
    }

    public void TakeDamage(int damage, Vector3 knockbackDirection)
    {
        if (currentHealth <= 0)
            return;

        currentHealth -= damage;

        Debug.Log("Enemy HP: " + currentHealth);

        if (currentHealth <= 0)
        {
            Die();
            return;
        }

        // 开始击退
        StartCoroutine(
            KnockbackCoroutine(knockbackDirection)
        );
    }

    IEnumerator KnockbackCoroutine(Vector3 direction)
    {
        // 防止同一时间启动多个击退
        if (isKnockedBack)
            yield break;

        isKnockedBack = true;

        // 击退的时候暂时关闭 AI
        if (enemyAI != null)
        {
            enemyAI.enabled = false;
        }

        direction.y = 0f;
        direction.Normalize();

        float timer = 0f;

        while (timer < knockbackDuration)
        {
            timer += Time.fixedDeltaTime;

            // 随时间逐渐减速
            float strength =
                1f - (timer / knockbackDuration);

            Vector3 velocity =
                direction *
                knockbackSpeed *
                strength;

            rb.linearVelocity = new Vector3(
                velocity.x,
                rb.linearVelocity.y,
                velocity.z
            );

            yield return new WaitForFixedUpdate();
        }

        // 停止水平移动
        rb.linearVelocity = new Vector3(
            0f,
            rb.linearVelocity.y,
            0f
        );

        // 恢复 AI
        if (enemyAI != null)
        {
            enemyAI.enabled = true;
        }

        isKnockedBack = false;
    }

    void Die()
    {
        Debug.Log("Enemy Dead");

        if (enemyAI != null)
        {
            enemyAI.enabled = false;
        }

        Destroy(gameObject);
    }
}