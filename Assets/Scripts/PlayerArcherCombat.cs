using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerArcherCombat : MonoBehaviour
{
    public PlayerMimic playerMimic;
    public GameObject arrowPrefab;
    public Transform arrowSpawnPoint;
    public float attackCooldown = 0.6f;
    public Animator archerAnimator;
    public bool HasPendingShot { get; private set; }
    public bool IsShooting { get; private set; }
    private float nextAttackTime;
    private Vector3 shotDirection;
    private Quaternion shotRotation;
    private PlayerHealth health;
    private CharacterSfx sfx;

    private void Awake()
    {
        health = GetComponent<PlayerHealth>();
        sfx = CharacterSfx.Get(gameObject);
    }

    private void Update()
    {
        if (playerMimic == null || !playerMimic.IsArcher() ||
            (health != null && health.GetCurrentHealth() <= 0f))
        {
            if (IsShooting) CancelShot();
            return;
        }
        if (Time.timeScale == 0f || Mouse.current == null) return;
        if (Mouse.current.leftButton.wasPressedThisFrame && !IsShooting && Time.time >= nextAttackTime)
            Shoot();
    }

    private void Shoot()
    {
        if (arrowPrefab == null || arrowSpawnPoint == null || archerAnimator == null ||
            playerMimic == null || !playerMimic.IsArcher() ||
            (health != null && health.GetCurrentHealth() <= 0f)) return;
        shotDirection = playerMimic.archerVisual.transform.forward;
        shotDirection.y = 0f;
        if (shotDirection.sqrMagnitude < 0.001f) return;
        shotDirection.Normalize();
        shotRotation = Quaternion.LookRotation(shotDirection, Vector3.up);
        HasPendingShot = true;
        IsShooting = true;
        nextAttackTime = Time.time + attackCooldown;
        archerAnimator.ResetTrigger("Shoot");
        archerAnimator.SetTrigger("Shoot");
    }

    private void LateUpdate()
    {
        if (HasPendingShot && playerMimic != null && playerMimic.IsArcher())
            playerMimic.archerVisual.transform.rotation = shotRotation;
    }

    public void ReleaseArrow()
    {
        if (!HasPendingShot || playerMimic == null || !playerMimic.IsArcher() ||
            (health != null && health.GetCurrentHealth() <= 0f)) return;
        HasPendingShot = false;
        var arrow = Instantiate(arrowPrefab, arrowSpawnPoint.position, Quaternion.LookRotation(shotDirection));
        var projectile = arrow.GetComponent<PlayerArrowProjectile>();
        if (projectile != null) projectile.SetDirection(shotDirection);
        sfx.Attack();
    }

    public void FinishShot()
    {
        HasPendingShot = false;
        IsShooting = false;
    }

    public void CancelShot()
    {
        FinishShot();
        if (archerAnimator != null) archerAnimator.ResetTrigger("Shoot");
    }

    private void OnDisable() { CancelShot(); }
}