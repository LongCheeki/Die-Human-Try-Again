using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerArcherCombat : MonoBehaviour
{
    [Header("References")]
    public PlayerMimic playerMimic;

    public GameObject arrowPrefab;
    public Transform arrowSpawnPoint;

    [Header("Attack")]
    public float attackCooldown = 0.6f;

    [Header("Animation")]
    public Animator archerAnimator;

    private float nextAttackTime = 0f;

    void Update()
    {
        if (Mouse.current == null)
            return;

        // 只有弓箭手形态才能射箭
        if (playerMimic == null ||
            !playerMimic.IsArcher())
        {
            return;
        }

        if (Mouse.current.leftButton.wasPressedThisFrame &&
            Time.time >= nextAttackTime)
        {
            Shoot();

            nextAttackTime =
                Time.time +
                attackCooldown;
        }
    }

    void Shoot()
    {
        if (arrowPrefab == null)
        {
            Debug.LogWarning(
                "Player Archer: Arrow Prefab missing!"
            );

            return;
        }

        if (arrowSpawnPoint == null)
        {
            Debug.LogWarning(
                "Player Archer: Arrow Spawn Point missing!"
            );

            return;
        }

        // =========================
        // ATTACK ANIMATION
        // =========================

        if (archerAnimator != null)
        {
            archerAnimator.SetInteger(
                "Trigger Number",
                2
            );

            archerAnimator.SetTrigger(
                "Trigger"
            );
        }

        // =========================
        // SHOOT DIRECTION
        // =========================

        Vector3 direction;

        // 使用弓箭手当前视觉模型的朝向
        if (playerMimic != null &&
            playerMimic.archerVisual != null)
        {
            direction =
                playerMimic.archerVisual.transform.forward;
        }
        else
        {
            // 找不到时才使用 Player 根物体方向
            direction =
                transform.forward;
        }

        // 不让箭往上或往下乱飞
        direction.y = 0f;
        direction.Normalize();

        // =========================
        // CREATE ARROW
        // =========================

        GameObject arrow =
            Instantiate(
                arrowPrefab,
                arrowSpawnPoint.position,
                Quaternion.identity
            );

        PlayerArrowProjectile projectile =
            arrow.GetComponent<PlayerArrowProjectile>();

        if (projectile != null)
        {
            projectile.SetDirection(
                direction
            );
        }
        else
        {
            Debug.LogWarning(
                "Player Arrow has no PlayerArrowProjectile script!"
            );
        }
    }
}