using UnityEngine;
using UnityEngine.InputSystem;
using System.Collections;

public class PlayerCombat : MonoBehaviour
{
    [Header("Weapon")]
    public Transform weaponPivot;
    public SwordHitbox swordHitbox;

    [Header("Mimic")]
    public PlayerMimic playerMimic;

    [Header("Warrior Animation")]
    public Animator warriorAnimator;

    [Header("Attack")]
    public float attackDuration = 0.3f;
    public float attackCooldown = 0.4f;

    public float startAngle = 180f;
    public float endAngle = -180f;

    private bool isAttacking = false;
    private bool canAttack = true;

    private Quaternion originalRotation;
    private CharacterSfx sfx;
    private PlayerHealth health;

    void Awake()
    {
        sfx = CharacterSfx.Get(gameObject);
        health = GetComponent<PlayerHealth>();
    }

    void Start()
    {
        if (weaponPivot != null)
        {
            originalRotation =
                weaponPivot.localRotation;
        }

        if (swordHitbox != null)
        {
            swordHitbox.DisableDamage();
        }
    }

    void Update()
    {
        if (Time.timeScale == 0f || (health != null && health.GetCurrentHealth() <= 0f))
            return;

        if (Mouse.current == null)
            return;

        if (Mouse.current.leftButton.wasPressedThisFrame &&
            canAttack &&
            !isAttacking)
        {
            StartCoroutine(
                Attack()
            );
        }
    }

    // =========================
    // ATTACK
    // =========================

    IEnumerator Attack()
    {
        isAttacking = true;
        canAttack = false;
        sfx.Attack();

        // 如果当前是战士形态
        // 播放战士攻击动画
        if (playerMimic != null &&
            playerMimic.IsWarrior())
        {
            if (warriorAnimator != null)
            {
                warriorAnimator.SetTrigger(
                    "Attack"
                );
            }
        }

        // 开启当前武器伤害
        if (swordHitbox != null)
        {
            swordHitbox.EnableDamage();
        }

        float timer = 0f;

        while (timer < attackDuration)
        {
            timer += Time.deltaTime;

            float t =
                timer / attackDuration;

            float angle =
                Mathf.Lerp(
                    startAngle,
                    endAngle,
                    t
                );

            if (weaponPivot != null)
            {
                weaponPivot.localRotation =
                    originalRotation *
                    Quaternion.Euler(
                        0f,
                        0f,
                        angle
                    );
            }

            yield return null;
        }

        // 攻击结束
        // 武器回到原来的角度
        if (weaponPivot != null)
        {
            weaponPivot.localRotation =
                originalRotation;
        }

        // 关闭伤害
        if (swordHitbox != null)
        {
            swordHitbox.DisableDamage();
        }

        isAttacking = false;

        yield return new WaitForSeconds(
            attackCooldown
        );

        canAttack = true;
    }

    // =========================
    // CHANGE WEAPON
    // =========================

    public void SetWeapon(
        Transform newWeapon,
        SwordHitbox newHitbox)
    {
        /*
         * newWeapon：
         * 以后 WeaponManager 会传进新武器
         *
         * 目前真正负责旋转的仍然是
         * weaponPivot，所以暂时不需要
         * 把 weaponPivot 替换掉。
         */

        swordHitbox = newHitbox;

        // 新武器刚装备时
        // 确保不能莫名其妙造成伤害
        if (swordHitbox != null)
        {
            swordHitbox.DisableDamage();
        }

        Debug.Log(
            "PlayerCombat weapon updated."
        );
    }
}