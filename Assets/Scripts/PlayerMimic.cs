using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerMimic : MonoBehaviour
{
    // =============================
    // FORMS
    // =============================

    [Header("Forms")]
    public GameObject slimeVisual;
    public GameObject warriorVisual;
    public GameObject archerVisual;

    // =============================
    // ANIMATORS
    // =============================

    [Header("Animators")]
    public Animator slimeAnimator;
    public Animator warriorAnimator;
    public Animator archerAnimator;

    // =============================
    // WEAPON
    // =============================

    [Header("Weapon")]
    public Transform weaponPivot;
    public Transform slimeWeaponAnchor;
    public Transform warriorWeaponAnchor;

    // =============================
    // FORM STATS
    // =============================

    [Header("Form Stats")]
    public float slimeMoveSpeedMultiplier = 1f;
    public float warriorMoveSpeedMultiplier = 0.8f;
    public float archerMoveSpeedMultiplier = 1f;

    // =============================
    // UNLOCKS
    // =============================

    [Header("Unlocks")]
    public bool warriorUnlocked = false;
    public bool archerUnlocked = false;

    // =============================
    // TRANSFORMATION EFFECT
    // =============================

    [Header("Transformation Effect")]
    public GameObject transformEffectPrefab;
    public Transform effectSpawnPoint;

    // =============================
    // REFERENCES
    // =============================

    [Header("References")]
    public PlayerMovement playerMovement;

    // =============================
    // CURRENT FORM
    // =============================

    private enum MimicForm
    {
        Slime,
        Warrior,
        Archer
    }

    private MimicForm currentForm =
        MimicForm.Slime;

    // =============================
    // START
    // =============================

    void Start()
    {
        SetSlimeForm(false);
    }

    // =============================
    // INPUT
    // =============================

    void Update()
    {
        if (Keyboard.current == null)
            return;

        // =========================
        // KEY 1 = WARRIOR
        // =========================

        if (Keyboard.current.digit1Key.wasPressedThisFrame)
        {
            if (!warriorUnlocked)
            {
                Debug.Log(
                    "Warrior Form is not unlocked yet."
                );

                return;
            }

            // 如果已经是战士
            // 再按1变回史莱姆
            if (currentForm == MimicForm.Warrior)
            {
                SetSlimeForm(true);
            }
            else
            {
                SetWarriorForm(true);
            }
        }

        // =========================
        // KEY 2 = ARCHER
        // =========================

        if (Keyboard.current.digit2Key.wasPressedThisFrame)
        {
            if (!archerUnlocked)
            {
                Debug.Log(
                    "Archer Form is not unlocked yet."
                );

                return;
            }

            // 如果已经是弓箭手
            // 再按2变回史莱姆
            if (currentForm == MimicForm.Archer)
            {
                SetSlimeForm(true);
            }
            else
            {
                SetArcherForm(true);
            }
        }
    }

    // =============================
    // UNLOCK WARRIOR
    // =============================

    public void UnlockWarrior()
    {
        warriorUnlocked = true;

        Debug.Log(
            "Warrior Form Unlocked!"
        );
    }

    // =============================
    // UNLOCK ARCHER
    // =============================

    public void UnlockArcher()
    {
        archerUnlocked = true;

        Debug.Log(
            "Archer Form Unlocked!"
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

        currentForm =
            MimicForm.Warrior;

        // 打开战士
        if (warriorVisual != null)
        {
            warriorVisual.SetActive(true);
        }

        // 关闭其他形态
        if (slimeVisual != null)
        {
            slimeVisual.SetActive(false);
        }

        if (archerVisual != null)
        {
            archerVisual.SetActive(false);
        }

        // 显示近战武器
        if (weaponPivot != null)
        {
            weaponPivot.gameObject.SetActive(true);
        }

        // 武器放到战士锚点
        MoveWeaponToAnchor(
            warriorWeaponAnchor
        );

        // 修改移动控制
        if (playerMovement != null &&
            warriorVisual != null)
        {
            playerMovement.SetCurrentVisual(
                warriorVisual.transform,
                warriorAnimator
            );

            playerMovement.SetMoveSpeedMultiplier(
                warriorMoveSpeedMultiplier
            );
        }

        Debug.Log(
            "Changed to Warrior Form."
        );
    }

    // =============================
    // ARCHER FORM
    // =============================

    public void SetArcherForm(
        bool playEffect = true)
    {
        if (!archerUnlocked)
            return;

        if (playEffect)
        {
            PlayTransformEffect();
        }

        currentForm =
            MimicForm.Archer;

        // 打开弓箭手
        if (archerVisual != null)
        {
            archerVisual.SetActive(true);
        }

        // 关闭其他形态
        if (slimeVisual != null)
        {
            slimeVisual.SetActive(false);
        }

        if (warriorVisual != null)
        {
            warriorVisual.SetActive(false);
        }

        // 弓箭手不用当前近战武器
        if (weaponPivot != null)
        {
            weaponPivot.gameObject.SetActive(false);
        }

        // 修改移动控制
        if (playerMovement != null &&
            archerVisual != null)
        {
            playerMovement.SetCurrentVisual(
                archerVisual.transform,
                archerAnimator
            );

            playerMovement.SetMoveSpeedMultiplier(
                archerMoveSpeedMultiplier
            );
        }

        Debug.Log(
            "Changed to Archer Form."
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

        currentForm =
            MimicForm.Slime;

        // 打开史莱姆
        if (slimeVisual != null)
        {
            slimeVisual.SetActive(true);
        }

        // 关闭其他形态
        if (warriorVisual != null)
        {
            warriorVisual.SetActive(false);
        }

        if (archerVisual != null)
        {
            archerVisual.SetActive(false);
        }

        // 恢复近战武器
        if (weaponPivot != null)
        {
            weaponPivot.gameObject.SetActive(true);
        }

        // 武器返回史莱姆锚点
        MoveWeaponToAnchor(
            slimeWeaponAnchor
        );

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
            "Changed to Slime Form."
        );
    }

    // =============================
    // DEATH -> SLIME
    // =============================

    public void ReturnToSlimeForDeath()
    {
        // 如果本来就是史莱姆
        // 不需要重新切换
        if (currentForm == MimicForm.Slime)
            return;

        PlayTransformEffect();

        currentForm =
            MimicForm.Slime;

        if (slimeVisual != null)
        {
            slimeVisual.SetActive(true);
        }

        if (warriorVisual != null)
        {
            warriorVisual.SetActive(false);
        }

        if (archerVisual != null)
        {
            archerVisual.SetActive(false);
        }

        // 恢复近战武器
        if (weaponPivot != null)
        {
            weaponPivot.gameObject.SetActive(true);
        }

        MoveWeaponToAnchor(
            slimeWeaponAnchor
        );

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
            "Mimic form collapsed back into Slime."
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

        Quaternion spawnRotation =
            Quaternion.identity;

        if (effectSpawnPoint != null)
        {
            spawnPosition =
                effectSpawnPoint.position;

            spawnRotation =
                effectSpawnPoint.rotation;
        }

        GameObject effect =
            Instantiate(
                transformEffectPrefab,
                spawnPosition,
                spawnRotation
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
        return currentForm ==
               MimicForm.Warrior;
    }

    public bool IsArcher()
    {
        return currentForm ==
               MimicForm.Archer;
    }

    public bool IsSlime()
    {
        return currentForm ==
               MimicForm.Slime;
    }

    public Animator GetCurrentAnimator()
    {
        if (currentForm == MimicForm.Warrior)
        {
            return warriorAnimator;
        }

        if (currentForm == MimicForm.Archer)
        {
            return archerAnimator;
        }

        return slimeAnimator;
    }
}