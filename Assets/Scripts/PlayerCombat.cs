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
        bool warrior = playerMimic != null && playerMimic.IsWarrior();
        if (swordHitbox != null) swordHitbox.DisableDamage();
        if (warrior)
        {
            if (warriorAnimator == null)
            {
                isAttacking = false;
                canAttack = true;
                yield break;
            }
            warriorAnimator.cullingMode = AnimatorCullingMode.AlwaysAnimate;
            warriorAnimator.SetTrigger("Attack");
            float timeout = 2f;
            while (timeout > 0f && playerMimic.IsWarrior() &&
                (health == null || health.GetCurrentHealth() > 0f))
            {
                var state = warriorAnimator.GetCurrentAnimatorStateInfo(0);
                if (state.IsName("infantry_04_attack_A") && state.normalizedTime >= 0.23f) break;
                timeout -= Time.deltaTime;
                yield return null;
            }
            var ready = warriorAnimator.GetCurrentAnimatorStateInfo(0);
            if (timeout <= 0f || !playerMimic.IsWarrior() ||
                (health != null && health.GetCurrentHealth() <= 0f) ||
                !ready.IsName("infantry_04_attack_A") || ready.normalizedTime >= 0.43f)
            {
                isAttacking = false;
                canAttack = true;
                yield break;
            }
        }

        sfx.Attack();
        if (swordHitbox != null) swordHitbox.EnableDamage();
        float timer = 0f;
        while (health == null || health.GetCurrentHealth() > 0f)
        {
            float progress;
            if (warrior)
            {
                if (!playerMimic.IsWarrior()) break;
                var state = warriorAnimator.GetCurrentAnimatorStateInfo(0);
                if (!state.IsName("infantry_04_attack_A") || state.normalizedTime >= 0.43f) break;
                progress = Mathf.InverseLerp(0.23f, 0.43f, state.normalizedTime);
            }
            else
            {
                if (timer >= attackDuration) break;
                timer += Time.deltaTime;
                progress = timer / attackDuration;
            }
            if (weaponPivot != null)
                weaponPivot.localRotation = originalRotation *
                    Quaternion.Euler(0f, 0f, Mathf.Lerp(startAngle, endAngle, progress));
            yield return null;
        }
        if (swordHitbox != null) swordHitbox.DisableDamage();
        if (weaponPivot != null) weaponPivot.localRotation = originalRotation;
        if (warrior)
            while (playerMimic.IsWarrior() && (health == null || health.GetCurrentHealth() > 0f))
            {
                var state = warriorAnimator.GetCurrentAnimatorStateInfo(0);
                if (!state.IsName("infantry_04_attack_A") || state.normalizedTime >= 0.9f) break;
                yield return null;
            }
        isAttacking = false;
        yield return new WaitForSeconds(attackCooldown);
        canAttack = true;
    }
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