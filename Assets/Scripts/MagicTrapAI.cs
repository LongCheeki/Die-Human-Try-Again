using UnityEngine;
using System.Collections;

public class MagicTrapAI : MonoBehaviour
{
    [Header("Target")]
    public Transform player;

    [Header("Attack")]
    public GameObject magicProjectilePrefab;
    public Transform magicSpawnPoint;

    public float detectionRange = 15f;
    public float attackCooldown = 1.5f;
    public float chargeTime = 0.5f;

    [Header("Rotation")]
    public Transform rotatingVisual;
    public float rotationSpeed = 8f;

    [Header("Charge Visual")]
    public Renderer[] trapRenderers;

    public Color normalColor =
        Color.white;

    public Color chargeColor =
        Color.red;

    private float nextAttackTime = 0f;
    private bool isCharging = false;

    void Update()
    {
        if (player == null)
            return;

        Vector3 direction =
            player.position -
            transform.position;

        direction.y = 0f;

        float distance =
            direction.magnitude;

        if (distance > detectionRange)
            return;

        FacePlayer();

        if (isCharging)
            return;

        if (Time.time >= nextAttackTime)
        {
            StartCoroutine(
                ChargeAndFire()
            );

            nextAttackTime =
                Time.time +
                attackCooldown;
        }
    }

    IEnumerator ChargeAndFire()
    {
        isCharging = true;

        SetTrapColor(
            chargeColor
        );

        yield return new WaitForSeconds(
            chargeTime
        );

        FireMagicProjectile();

        SetTrapColor(
            normalColor
        );

        isCharging = false;
    }

    void FireMagicProjectile()
    {
        if (magicProjectilePrefab == null ||
            magicSpawnPoint == null ||
            player == null)
            return;

        Vector3 direction =
            player.position -
            magicSpawnPoint.position;

        GameObject projectile =
            Instantiate(
                magicProjectilePrefab,
                magicSpawnPoint.position,
                Quaternion.identity
            );

        MagicProjectile magic =
            projectile.GetComponent<
                MagicProjectile
            >();

        if (magic != null)
        {
            magic.SetDirection(
                direction
            );
        }
    }

    void FacePlayer()
    {
        if (rotatingVisual == null)
            return;

        Vector3 direction =
            player.position -
            rotatingVisual.position;

        direction.y = 0f;

        if (direction.sqrMagnitude <
            0.001f)
            return;

        Quaternion targetRotation =
            Quaternion.LookRotation(
                direction
            );

        rotatingVisual.rotation =
            Quaternion.Slerp(
                rotatingVisual.rotation,
                targetRotation,
                rotationSpeed *
                Time.deltaTime
            );
    }

    void SetTrapColor(
        Color color)
    {
        if (trapRenderers == null)
            return;

        foreach (
            Renderer renderer
            in trapRenderers)
        {
            if (renderer == null)
                continue;

            renderer.material.color =
                color;
        }
    }
}