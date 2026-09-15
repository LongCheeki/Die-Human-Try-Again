using UnityEngine;
using System.Collections;

public class PlayerHealth : MonoBehaviour
{
    [Header("Health")]
    public float maxHealth = 5f;
    private float currentHealth;

    [Header("Invincibility")]
    public float invincibleTime = 0.5f;
    private bool isInvincible = false;

    [Header("Warrior")]
    public float warriorDamageMultiplier = 0.5f;

    [Header("Knockback")]
    public float knockbackSpeed = 7f;
    public float knockbackDuration = 0.2f;

    [Header("Respawn")]
    public Transform respawnPoint;
    public float respawnDelay = 2f;
    public bool restartSceneOnDeath;

    [Header("References")]
    public Animator slimeAnimator;
    public PlayerMovement playerMovement;
    public PlayerMimic playerMimic;

    [Header("UI")]
    public PlayerHealthUI healthUI;

    private Rigidbody rb;
    private CharacterSfx sfx;

    void Awake()
    {
        sfx = CharacterSfx.Get(gameObject);
        CastleBattleMusic.Ensure(gameObject);
    }

    private bool isDead = false;

    void Start()
    {
        rb = GetComponent<Rigidbody>();

        currentHealth = maxHealth;

        if (healthUI != null)
        {
            healthUI.UpdateHealthUI();
        }
    }

    // 普通受伤
    public void TakeDamage(float damage)
    {
        TakeDamage(damage, Vector3.zero);
    }

    // 带击退方向的受伤
    public void TakeDamage(float damage, Vector3 knockbackDirection)
    {
        ApplyDamage(damage, knockbackDirection, false);
    }

    public void TakeTrapDamage(float damage)
    {
        ApplyDamage(damage, Vector3.zero, true);
    }

    private void ApplyDamage(float damage, Vector3 knockbackDirection, bool trap)
    {
        if (isDead)
            return;

        if (isInvincible && !trap)
            return;

        float finalDamage = damage;

        // Warrior 受到更少伤害
        if (!trap && playerMimic != null &&
            playerMimic.IsWarrior())
        {
            finalDamage *= warriorDamageMultiplier;
        }

        float healthBeforeDamage = currentHealth;
        currentHealth -= finalDamage;

        currentHealth = Mathf.Clamp(
            currentHealth,
            0f,
            maxHealth
        );

        if (currentHealth < healthBeforeDamage) sfx.Hurt();

        Debug.Log(
            "Player took " +
            finalDamage +
            " damage. HP: " +
            currentHealth +
            "/" +
            maxHealth
        );

        // 更新血条 + 受击红闪
        if (healthUI != null)
        {
            healthUI.PlayDamageFeedback();
        }

        // Slime 受击动画
        if (playerMimic == null ||
            !playerMimic.IsWarrior())
        {
            if (slimeAnimator != null)
            {
                slimeAnimator.SetTrigger("Damage");
            }
        }

        // 死亡
        if (currentHealth <= 0f)
        {
            Die();
            return;
        }

        // 击退
        if (knockbackDirection != Vector3.zero)
        {
            StartCoroutine(
                KnockbackCoroutine(
                    knockbackDirection
                )
            );
        }

        if (!trap) StartCoroutine(InvincibilityCoroutine());
    }

    IEnumerator InvincibilityCoroutine()
    {
        isInvincible = true;

        yield return new WaitForSeconds(
            invincibleTime
        );

        isInvincible = false;
    }

    IEnumerator KnockbackCoroutine(
        Vector3 direction)
    {
        if (rb == null)
            yield break;

        direction.y = 0f;
        direction.Normalize();

        // 暂时关闭玩家移动
        if (playerMovement != null)
        {
            playerMovement.enabled = false;
        }

        float timer = 0f;

        while (timer < knockbackDuration)
        {
            timer += Time.deltaTime;

            float t =
                timer /
                knockbackDuration;

            float currentSpeed =
                Mathf.Lerp(
                    knockbackSpeed,
                    0f,
                    t
                );

            rb.linearVelocity =
                new Vector3(
                    direction.x * currentSpeed,
                    rb.linearVelocity.y,
                    direction.z * currentSpeed
                );

            yield return null;
        }

        rb.linearVelocity =
            new Vector3(
                0f,
                rb.linearVelocity.y,
                0f
            );

        if (!isDead &&
            playerMovement != null)
        {
            playerMovement.enabled = true;
        }
    }

    void Die()
    {
        if (isDead)
            return;

        isDead = true;
        isInvincible = false;

        Debug.Log("Player Died!");

        StopAllCoroutines();

        if (rb != null)
        {
            rb.linearVelocity =
                Vector3.zero;
        }

        if (playerMovement != null)
        {
            playerMovement.enabled = false;
        }

        // Warrior 死亡时先恢复成 Slime
        if (playerMimic != null)
        {
            playerMimic.ReturnToSlimeForDeath();
        }

        // 播放史莱姆死亡动画
        if (slimeAnimator != null)
        {
            slimeAnimator.SetTrigger("Death");
        }

        StartCoroutine(
            RespawnCoroutine()
        );
    }

    IEnumerator RespawnCoroutine()
    {
        if (restartSceneOnDeath)
        {
            var combat = GetComponent<PlayerCombat>();
            if (combat != null)
            {
                combat.StopAllCoroutines();
                combat.enabled = false;
            }
            foreach (var sword in GetComponentsInChildren<SwordHitbox>(true)) sword.DisableDamage();
            if (playerMimic != null) playerMimic.enabled = false;
            yield return new WaitForSecondsRealtime(respawnDelay);
            Time.timeScale = 1;
            UnityEngine.SceneManagement.SceneManager.LoadSceneAsync(gameObject.scene.name);
            yield break;
        }
        yield return new WaitForSeconds(
            respawnDelay
        );

        // 传送回出生点
        if (respawnPoint != null)
        {
            transform.position =
                respawnPoint.position;

            transform.rotation =
                respawnPoint.rotation;
        }

        // 恢复生命
        currentHealth = maxHealth;

        isDead = false;
        isInvincible = false;

        if (rb != null)
        {
            rb.linearVelocity =
                Vector3.zero;
        }

        // 恢复成 Slime
        if (playerMimic != null)
        {
            playerMimic.SetSlimeForm(false);
        }

        if (playerMovement != null)
        {
            playerMovement.enabled = true;
        }

        // 回到 Idle
        if (slimeAnimator != null)
        {
            slimeAnimator.SetTrigger("Idle");
        }

        // 更新血量 UI
        if (healthUI != null)
        {
            healthUI.UpdateHealthUI();
        }

        Debug.Log("Player Respawned!");
    }

    // 给 UI 读取当前生命值
    public float GetCurrentHealth()
    {
        return currentHealth;
    }

    // 给 UI 读取最大生命值
    public float GetMaxHealth()
    {
        return maxHealth;
    }
    public void Heal(float amount)
    {
        if (isDead)
            return;

        if (currentHealth >= maxHealth)
            return;

        currentHealth += amount;

        currentHealth = Mathf.Clamp(
            currentHealth,
            0f,
            maxHealth
        );

        Debug.Log(
            "Player healed " +
            amount +
            ". HP: " +
            currentHealth +
            "/" +
            maxHealth
        );

        if (healthUI != null)
        {
            healthUI.UpdateHealthUI();
        }
    }
}