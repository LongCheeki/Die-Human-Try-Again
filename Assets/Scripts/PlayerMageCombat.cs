using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerMageCombat : MonoBehaviour
{
    [Header("References")]
    public PlayerMimic playerMimic;

    public GameObject magicProjectilePrefab;

    public Transform magicSpawnPoint;

    [Header("Attack")]
    public float attackCooldown = 0.5f;

    [Header("Animation")]
    public Animator mageAnimator;

    private float nextAttackTime = 0f;

    void Update()
    {
        if (Mouse.current == null)
            return;

        // 只有法师形态才能使用
        if (playerMimic == null ||
            !playerMimic.IsMage())
        {
            return;
        }

        if (
            Mouse.current.leftButton.wasPressedThisFrame &&
            Time.time >= nextAttackTime)
        {
            ShootMagic();

            nextAttackTime =
                Time.time +
                attackCooldown;
        }
    }

    void ShootMagic()
    {
        if (magicProjectilePrefab == null)
        {
            Debug.LogWarning(
                "Player Mage: Magic Projectile Prefab missing!"
            );

            return;
        }

        if (magicSpawnPoint == null)
        {
            Debug.LogWarning(
                "Player Mage: Magic Spawn Point missing!"
            );

            return;
        }

        // 如果以后有攻击动画
        // 可以在这里触发
        if (mageAnimator != null)
        {
            // 暂时不强制写具体动画参数
        }

        // 发射方向直接使用发射点的 forward
        Vector3 direction =
            magicSpawnPoint.forward;

        direction.y = 0f;

        if (direction.sqrMagnitude <
            0.001f)
        {
            return;
        }

        direction.Normalize();

        GameObject projectile =
            Instantiate(
                magicProjectilePrefab,
                magicSpawnPoint.position,
                Quaternion.identity
            );

        PlayerMagicProjectile magic =
            projectile.GetComponent<
                PlayerMagicProjectile
            >();

        if (magic != null)
        {
            magic.SetDirection(
                direction
            );
        }
        else
        {
            Debug.LogWarning(
                "Player Magic Projectile has no PlayerMagicProjectile script!"
            );
        }
    }
}