using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerMovement : MonoBehaviour
{
    [Header("Movement")]
    public float moveSpeed = 5f;

    [Header("Dash")]
    public float dashSpeed = 12f;
    public float dashDuration = 0.18f;
    public float dashCooldown = 0.8f;

    [Header("Current Form")]
    public Transform currentVisual;
    public Animator currentAnimator;

    private Rigidbody rb;
    private CharacterSfx sfx;

    private Vector3 moveInput;
    private Vector3 lastMoveDirection = Vector3.forward;

    private bool slimeIsMoving = false;

    private bool isDashing = false;
    private bool canDash = true;

    private float dashTimer = 0f;
    private float dashCooldownTimer = 0f;

    // 不同形态的移动速度倍率
    private float moveSpeedMultiplier = 1f;

    void Awake()
    {
        rb = GetComponent<Rigidbody>();
        sfx = CharacterSfx.Get(gameObject);
    }

    void Update()
    {
        if (Time.timeScale == 0f) return;
        ReadInput();

        if (!isDashing)
        {
            RotateCharacterInstantly();
            UpdateAnimation();
        }

        HandleDash();
    }

    void OnEnable()
    {
        if (sfx != null) sfx.ResetMovement();
    }

    void FixedUpdate()
    {
        sfx.Movement(!isDashing && moveInput.sqrMagnitude > 0.01f &&
            Mathf.Abs(rb.linearVelocity.y) < 0.5f);
        if (isDashing)
        {
            DashMovement();
        }
        else
        {
            NormalMovement();
        }
    }

    // =========================
    // INPUT
    // =========================

    void ReadInput()
    {
        float x = 0f;
        float z = 0f;

        if (Keyboard.current == null)
            return;

        if (Keyboard.current.wKey.isPressed)
            z += 1f;

        if (Keyboard.current.sKey.isPressed)
            z -= 1f;

        if (Keyboard.current.aKey.isPressed)
            x -= 1f;

        if (Keyboard.current.dKey.isPressed)
            x += 1f;

        moveInput = new Vector3(x, 0f, z).normalized;

        if (moveInput.sqrMagnitude > 0.01f)
        {
            lastMoveDirection = moveInput;
        }
    }

    // =========================
    // NORMAL MOVEMENT
    // =========================

    void NormalMovement()
    {
        Vector3 velocity =
            moveInput *
            moveSpeed *
            moveSpeedMultiplier;

        rb.linearVelocity = new Vector3(
            velocity.x,
            rb.linearVelocity.y,
            velocity.z
        );
    }

    // =========================
    // ROTATION
    // =========================

    void RotateCharacterInstantly()
    {
        if (currentVisual == null)
            return;

        if (moveInput.sqrMagnitude < 0.01f)
            return;

        currentVisual.rotation =
            Quaternion.LookRotation(
                moveInput,
                Vector3.up
            );
    }

    // =========================
    // DASH
    // =========================

    void HandleDash()
    {
        if (!canDash)
        {
            dashCooldownTimer -= Time.deltaTime;

            if (dashCooldownTimer <= 0f)
            {
                canDash = true;
            }
        }

        if (Keyboard.current != null &&
            Keyboard.current.spaceKey.wasPressedThisFrame &&
            canDash &&
            !isDashing)
        {
            StartDash();
        }

        if (isDashing)
        {
            dashTimer -= Time.deltaTime;

            if (dashTimer <= 0f)
            {
                EndDash();
            }
        }
    }

    void StartDash()
    {
        isDashing = true;
        sfx.Dash();
        canDash = false;

        dashTimer = dashDuration;
        dashCooldownTimer = dashCooldown;

        if (currentVisual != null)
        {
            currentVisual.rotation =
                Quaternion.LookRotation(
                    lastMoveDirection,
                    Vector3.up
                );
        }
    }

    void DashMovement()
    {
        Vector3 dashVelocity =
            lastMoveDirection * dashSpeed;

        rb.linearVelocity = new Vector3(
            dashVelocity.x,
            rb.linearVelocity.y,
            dashVelocity.z
        );
    }

    void EndDash()
    {
        isDashing = false;
    }

    // =========================
    // ANIMATION
    // =========================

    void UpdateAnimation()
    {
        if (currentAnimator == null)
            return;

        bool movingNow =
            moveInput.sqrMagnitude > 0.01f;

        // Warrior：
        // Bool = IsMoving
        if (HasParameter(
            currentAnimator,
            "IsMoving"))
        {
            currentAnimator.SetBool(
                "IsMoving",
                movingNow
            );

            return;
        }

        // Slime：
        // Trigger = Idle / Move
        if (movingNow && !slimeIsMoving)
        {
            if (HasParameter(currentAnimator, "Idle"))
            {
                currentAnimator.ResetTrigger("Idle");
            }

            if (HasParameter(currentAnimator, "Move"))
            {
                currentAnimator.SetTrigger("Move");
            }

            slimeIsMoving = true;
        }
        else if (!movingNow && slimeIsMoving)
        {
            if (HasParameter(currentAnimator, "Move"))
            {
                currentAnimator.ResetTrigger("Move");
            }

            if (HasParameter(currentAnimator, "Idle"))
            {
                currentAnimator.SetTrigger("Idle");
            }

            slimeIsMoving = false;
        }
    }

    // =========================
    // FORM SWITCH
    // =========================

    public void SetCurrentVisual(
        Transform newVisual,
        Animator newAnimator)
    {
        currentVisual = newVisual;
        currentAnimator = newAnimator;

        slimeIsMoving = false;

        if (currentAnimator != null &&
            HasParameter(currentAnimator, "IsMoving"))
        {
            currentAnimator.SetBool(
                "IsMoving",
                false
            );
        }
    }

    // 改变当前形态的速度倍率
    public void SetMoveSpeedMultiplier(
        float multiplier)
    {
        moveSpeedMultiplier = multiplier;
    }

    // =========================
    // PARAMETER CHECK
    // =========================

    bool HasParameter(
        Animator animator,
        string parameterName)
    {
        foreach (
            AnimatorControllerParameter parameter
            in animator.parameters)
        {
            if (parameter.name == parameterName)
            {
                return true;
            }
        }

        return false;
    }
}