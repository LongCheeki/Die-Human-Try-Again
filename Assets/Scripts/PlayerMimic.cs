using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerMimic : MonoBehaviour
{
    [Header("Forms")]
    public GameObject slimeVisual;
    public GameObject warriorVisual;

    [Header("Animators")]
    public Animator slimeAnimator;
    public Animator warriorAnimator;

    [Header("Weapon")]
    public Transform weaponPivot;
    public Transform slimeWeaponAnchor;
    public Transform warriorWeaponAnchor;

    [Header("Form Stats")]
    public float slimeMoveSpeedMultiplier = 1f;
    public float warriorMoveSpeedMultiplier = 0.8f;

    [Header("Unlocks")]
    public bool warriorUnlocked = false;

    [Header("Transformation Effect")]
    public GameObject transformEffectPrefab;
    public Transform effectSpawnPoint;

    [Header("References")]
    public PlayerMovement playerMovement;

    private bool isWarrior = false;

    void Start()
    {
        SetSlimeForm(false);
    }

    void Update()
    {
        if (Keyboard.current == null)
            return;

        if (Keyboard.current.digit1Key.wasPressedThisFrame)
        {
            if (!warriorUnlocked)
            {
                Debug.Log(
                    "Warrior Form is not unlocked yet."
                );

                return;
            }

            if (isWarrior)
            {
                SetSlimeForm(true);
            }
            else
            {
                SetWarriorForm(true);
            }
        }
    }

    // =============================
    // UNLOCK
    // =============================

    public void UnlockWarrior()
    {
        warriorUnlocked = true;

        Debug.Log(
            "Warrior Form Unlocked!"
        );
    }

    // =============================
    // WARRIOR FORM
    // =============================

    public void SetWarriorForm(
        bool playEffect = true)
    {
        if (!warriorUnlocked)
            return;

        if (playEffect)
        {
            PlayTransformEffect();
        }

        isWarrior = true;

        // 先打开战士
        if (warriorVisual != null)
        {
            warriorVisual.SetActive(true);
        }

        // 武器放到战士右手
        MoveWeaponToAnchor(
            warriorWeaponAnchor
        );

        // 关闭史莱姆
        if (slimeVisual != null)
        {
            slimeVisual.SetActive(false);
        }

        // 修改移动控制
        if (playerMovement != null &&
            warriorVisual != null)
        {
            playerMovement.SetCurrentVisual(
                warriorVisual.transform,
                warriorAnimator
            );

            // 战士更慢
            playerMovement.SetMoveSpeedMultiplier(
                warriorMoveSpeedMultiplier
            );
        }

        Debug.Log(
            "Changed to Warrior Form."
        );
    }

    // =============================
    // SLIME FORM
    // =============================

    public void SetSlimeForm(
        bool playEffect = true)
    {
        if (playEffect)
        {
            PlayTransformEffect();
        }

        isWarrior = false;

        // 打开史莱姆
        if (slimeVisual != null)
        {
            slimeVisual.SetActive(true);
        }

        // 武器返回史莱姆
        MoveWeaponToAnchor(
            slimeWeaponAnchor
        );

        // 关闭战士
        if (warriorVisual != null)
        {
            warriorVisual.SetActive(false);
        }

        if (playerMovement != null &&
            slimeVisual != null)
        {
            playerMovement.SetCurrentVisual(
                slimeVisual.transform,
                slimeAnimator
            );

            // 恢复正常速度
            playerMovement.SetMoveSpeedMultiplier(
                slimeMoveSpeedMultiplier
            );
        }

        Debug.Log(
            "Changed to Slime Form."
        );
    }

    // =============================
    // DEATH -> SLIME
    // =============================

    public void ReturnToSlimeForDeath()
    {
        // 本来就是史莱姆就不处理
        if (!isWarrior)
            return;

        PlayTransformEffect();

        isWarrior = false;

        if (slimeVisual != null)
        {
            slimeVisual.SetActive(true);
        }

        MoveWeaponToAnchor(
            slimeWeaponAnchor
        );

        if (warriorVisual != null)
        {
            warriorVisual.SetActive(false);
        }

        if (playerMovement != null &&
            slimeVisual != null)
        {
            playerMovement.SetCurrentVisual(
                slimeVisual.transform,
                slimeAnimator
            );

            playerMovement.SetMoveSpeedMultiplier(
                slimeMoveSpeedMultiplier
            );
        }

        Debug.Log(
            "Warrior collapsed back into Slime."
        );
    }

    // =============================
    // WEAPON
    // =============================

    void MoveWeaponToAnchor(
        Transform newAnchor)
    {
        if (weaponPivot == null ||
            newAnchor == null)
            return;

        weaponPivot.SetParent(
            newAnchor,
            false
        );

        weaponPivot.localPosition =
            Vector3.zero;

        weaponPivot.localRotation =
            Quaternion.identity;
    }

    // =============================
    // EFFECT
    // =============================

    void PlayTransformEffect()
    {
        if (transformEffectPrefab == null)
            return;

        Vector3 spawnPosition =
            transform.position;

        if (effectSpawnPoint != null)
        {
            spawnPosition =
                effectSpawnPoint.position;
        }

        GameObject effect =
            Instantiate(
                transformEffectPrefab,
                spawnPosition,
                Quaternion.identity
            );

        Destroy(
            effect,
            2f
        );
    }

    // =============================
    // PUBLIC INFO
    // =============================

    public bool IsWarrior()
    {
        return isWarrior;
    }

    public Animator GetCurrentAnimator()
    {
        if (isWarrior)
            return warriorAnimator;

        return slimeAnimator;
    }
}