using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

[RequireComponent(typeof(EnemyHealth), typeof(Rigidbody), typeof(CapsuleCollider))]
public sealed class GolelingBossAI : MonoBehaviour
{
    public enum AttackKind { Punch, Bullets, Charge, Headbutt }
    private enum Phase { Idle, Windup, Attack, Recovery, Dead }
    public Transform visual;
    public Animation animationPlayer;
    public GolelingProjectile projectilePrefab;
    public Material warningMaterial;
    public Vector3 arenaCenter = new Vector3(0f, 0f, 62f);
    public Vector2 arenaHalfSize = new Vector2(8.5f, 7.5f);
    public float moveSpeed = 2f;
    public float punchRange = 2.3f;
    public float punchDamage = 1f;
    public float headbuttDamage = 2f;
    public float chargeDamage = 2f;
    public float chargeSpeed = 24f;
    public float chargeRange = 15f;
    public float attackInterval = 1.2f;
    public int navigationAgentType;
    public AttackKind CurrentAttack { get; private set; }
    public bool IsActive { get; private set; }
    private Phase phase;
    private EnemyHealth health;
    private Rigidbody body;
    private CapsuleCollider capsule;
    private PlayerHealth player;
    private PlayerMimic mimic;
    private GolelingBossEncounter encounter;
    private CharacterSfx sfx;
    private float phaseTime;
    private float chargeDistance;
    private float nextAttack;
    private bool impactDone;
    private bool rushHit;
    private bool alternateRanged;
    private bool alternateMelee;
    private Vector3 lockedDirection;
    private LineRenderer warning;
    private readonly Dictionary<string, string> clips = new Dictionary<string, string>();
    private string playingClip;
    private NavMeshPath chasePath;
    private int chaseCorner;
    private float nextPathUpdate;

    private void Awake()
    {
        health = GetComponent<EnemyHealth>();
        body = GetComponent<Rigidbody>();
        capsule = GetComponent<CapsuleCollider>();
        sfx = CharacterSfx.Get(gameObject);
        health.Died += Die;
        health.HealthChanged += OnHurt;
        if (animationPlayer != null)
            foreach (AnimationState state in animationPlayer)
            {
                var shortName = state.name.Substring(state.name.LastIndexOf('|') + 1);
                clips[shortName] = state.name;
                state.wrapMode = shortName == "Fast_Flying" || shortName == "Flying_Idle" ? WrapMode.Loop : WrapMode.Once;
            }
    }

    public void Begin(PlayerHealth target, GolelingBossEncounter room)
    {
        player = target;
        mimic = target.GetComponent<PlayerMimic>();
        encounter = room;
        chasePath = new NavMeshPath();
        chaseCorner = 1;
        nextPathUpdate = 0f;
        IsActive = true;
        phase = Phase.Idle;
        nextAttack = Time.time + 1.2f;
        Play("Flying_Idle");
    }

    public float GetIncomingDamageMultiplier()
    {
        return mimic != null && mimic.IsArcher() ? 0.75f : 1f;
    }

    private void FixedUpdate()
    {
        if (!IsActive || phase == Phase.Dead || Time.timeScale == 0f) return;
        if (player == null || player.GetCurrentHealth() <= 0f) { IsActive = false; ClearWarning(); return; }
        var dt = Time.fixedDeltaTime;
        phaseTime += dt;
        var toPlayer = player.transform.position - body.position;
        toPlayer.y = 0f;
        var distance = toPlayer.magnitude;
        if (phase == Phase.Idle)
        {
            var ranged = mimic != null && mimic.IsArcher();
            var clearApproach = distance < 0.01f || MovementClearance(toPlayer.normalized, distance) >= distance - 0.1f;
            if (!clearApproach || distance > (ranged ? 3.4f : punchRange * 0.9f))
            {
                var direction = ChaseDirection();
                Face(direction);
                Move(direction * moveSpeed * dt);
                Play("Fast_Flying");
            }
            else { Face(toPlayer); Play("Flying_Idle"); }
            if (!clearApproach) return;
            if (Time.time < nextAttack) return;
            if (ranged && alternateRanged && distance > 3.4f) return;
            if (ranged)
            {
                CurrentAttack = alternateRanged ? AttackKind.Headbutt : AttackKind.Charge;
                alternateRanged = !alternateRanged;
            }
            else
            {
                CurrentAttack = distance <= punchRange && !alternateMelee ? AttackKind.Punch : AttackKind.Bullets;
                alternateMelee = CurrentAttack == AttackKind.Punch;
            }
            lockedDirection = toPlayer.sqrMagnitude > 0.001f ? toPlayer.normalized : transform.forward;
            Face(lockedDirection);
            phase = Phase.Windup;
            phaseTime = 0f;
            impactDone = false;
            rushHit = false;
            chargeDistance = 0f;
            Play("Flying_Idle");
            ShowWarning();
            return;
        }
        if (phase == Phase.Windup)
        {
            if (warning != null)
            {
                var color = Color.Lerp(new Color(1f, 0.55f, 0.2f, 0.35f), new Color(1f, 0.04f, 0.03f, 0.85f), phaseTime / WindupTime());
                warning.startColor = color;
                warning.endColor = color;
            }
            if (phaseTime < WindupTime()) return;
            ClearWarning();
            phase = Phase.Attack;
            phaseTime = 0f;
            Play(CurrentAttack == AttackKind.Charge ? "Fast_Flying" : CurrentAttack == AttackKind.Headbutt ? "Headbutt" : "Punch", true);
            sfx.EnemyAttack();
            return;
        }
        if (phase == Phase.Attack)
        {
            if (CurrentAttack == AttackKind.Punch || CurrentAttack == AttackKind.Bullets)
            {
                if (!impactDone && phaseTime >= 0.4f)
                {
                    impactDone = true;
                    if (CurrentAttack == AttackKind.Punch) Punch(); else Shoot();
                }
                if (phaseTime >= 1.17f) Recover();
            }
            else
            {
                var speed = CurrentAttack == AttackKind.Charge ? chargeSpeed : 5.5f;
                var limit = CurrentAttack == AttackKind.Charge ? chargeRange : 2.6f;
                var movement = Move(lockedDirection * Mathf.Min(speed * dt, Mathf.Max(0f, limit - chargeDistance)));
                chargeDistance += movement;
                RushDamage();
                if (movement < speed * dt * 0.4f || chargeDistance >= limit || phaseTime >= (CurrentAttack == AttackKind.Charge ? chargeRange / Mathf.Max(0.1f, chargeSpeed) + 0.25f : 0.6f))
                {
                    if (CurrentAttack == AttackKind.Charge) Play("Headbutt", true);
                    Recover();
                }
            }
            return;
        }
        if (phase == Phase.Recovery && phaseTime >= (CurrentAttack == AttackKind.Charge ? 1.5f : CurrentAttack == AttackKind.Headbutt ? 1f : 0.8f))
        {
            phase = Phase.Idle;
            phaseTime = 0f;
            nextAttack = Time.time + attackInterval;
            Play("Flying_Idle");
        }
    }

    private float WindupTime()
    {
        return CurrentAttack == AttackKind.Punch ? 0.4f : CurrentAttack == AttackKind.Charge ? 0.75f : 0.6f;
    }

    private void Punch()
    {
        var collider = player.GetComponent<Collider>();
        var nearest = collider != null ? collider.ClosestPoint(body.position) : player.transform.position;
        var delta = nearest - body.position;
        delta.y = 0f;
        if (delta.magnitude <= punchRange && Vector3.Dot(lockedDirection, delta.normalized) >= 0.4f)
            player.TakeDamage(punchDamage, lockedDirection);
    }

    private void Shoot()
    {
        if (projectilePrefab == null) return;
        var spawn = body.position + lockedDirection * (capsule.radius + 0.35f);
        spawn.y = 0.65f;
        var heading = player.transform.position - spawn;
        heading.y = 0f;
        heading = heading.normalized;
        foreach (var angle in new[] { -15f, 0f, 15f })
        {
            var projectile = Instantiate(projectilePrefab, spawn, Quaternion.identity);
            projectile.gameObject.SetActive(true);
            projectile.Initialize(Quaternion.Euler(0f, angle, 0f) * heading, this);
        }
    }

    private void RushDamage()
    {
        if (rushHit) return;
        var other = player.GetComponent<Collider>();
        if (other == null) return;
        if (!Physics.ComputePenetration(capsule, body.position, body.rotation, other, other.transform.position, other.transform.rotation, out _, out _))
        {
            var point = other.ClosestPoint(body.position);
            point.y = body.position.y;
            if (Vector3.Distance(point, body.position) > capsule.radius + 0.25f) return;
        }
        rushHit = true;
        player.TakeDamage(CurrentAttack == AttackKind.Charge ? chargeDamage : headbuttDamage, lockedDirection);
    }

    private Vector3 ChaseDirection()
    {
        if (Time.time >= nextPathUpdate)
        {
            nextPathUpdate = Time.time + 0.25f;
            chaseCorner = 1;
            chasePath.ClearCorners();
            var filter = new NavMeshQueryFilter { agentTypeID = navigationAgentType, areaMask = NavMesh.AllAreas };
            if (NavMesh.SamplePosition(body.position, out var start, 2f, filter) &&
                NavMesh.SamplePosition(player.transform.position, out var target, 2f, filter))
                NavMesh.CalculatePath(start.position, target.position, filter, chasePath);
        }
        var corners = chasePath.corners;
        while (chaseCorner < corners.Length)
        {
            var delta = corners[chaseCorner] - body.position;
            delta.y = 0f;
            if (delta.sqrMagnitude > 0.04f) return delta.normalized;
            chaseCorner++;
        }
        var direct = player.transform.position - body.position;
        direct.y = 0f;
        return direct.normalized;
    }

    private float Move(Vector3 delta)
    {
        var start = body.position;
        var desired = start + delta;
        desired.x = Mathf.Clamp(desired.x, arenaCenter.x - arenaHalfSize.x, arenaCenter.x + arenaHalfSize.x);
        desired.z = Mathf.Clamp(desired.z, arenaCenter.z - arenaHalfSize.y, arenaCenter.z + arenaHalfSize.y);
        delta = desired - start;
        var length = delta.magnitude;
        if (length <= 0.0001f) return 0f;
        length = MovementClearance(delta / length, length);
        body.MovePosition(start + delta.normalized * length);
        return length;
    }

    private float MovementClearance(Vector3 direction, float length)
    {
        var center = body.position + capsule.center;
        var half = Mathf.Max(0f, capsule.height * 0.5f - capsule.radius);
        var hits = Physics.CapsuleCastAll(center + Vector3.up * half, center - Vector3.up * half, capsule.radius, direction, length, ~0, QueryTriggerInteraction.Ignore);
        foreach (var hit in hits)
        {
            if (hit.collider.GetComponentInParent<GolelingBossAI>() == this || hit.collider.GetComponentInParent<PlayerHealth>() != null) continue;
            length = Mathf.Min(length, Mathf.Max(0f, hit.distance - 0.04f));
        }
        return length;
    }

    private void Face(Vector3 direction)
    {
        if (direction.sqrMagnitude > 0.001f) body.rotation = Quaternion.LookRotation(direction);
    }

    private void Recover() { phase = Phase.Recovery; phaseTime = 0f; }

    private void ShowWarning()
    {
        var marker = new GameObject("Boss attack warning");
        marker.transform.rotation = Quaternion.Euler(90f, 0f, 0f);
        warning = marker.AddComponent<LineRenderer>();
        warning.sharedMaterial = warningMaterial;
        warning.useWorldSpace = true;
        warning.alignment = LineAlignment.TransformZ;
        warning.shadowCastingMode = UnityEngine.Rendering.ShadowCastingMode.Off;
        warning.receiveShadows = false;
        warning.startWidth = warning.endWidth = CurrentAttack == AttackKind.Charge ? capsule.radius * 2f + 0.3f : 0.16f;
        warning.startColor = warning.endColor = new Color(1f, 0.25f, 0.03f, 0.65f);
        var origin = body.position;
        origin.y = -0.045f;
        if (CurrentAttack == AttackKind.Punch || CurrentAttack == AttackKind.Bullets)
        {
            warning.positionCount = 17;
            for (var i = 0; i < 17; i++)
                warning.SetPosition(i, origin + Quaternion.Euler(0f, -65f + i * 130f / 16f, 0f) * lockedDirection * (CurrentAttack == AttackKind.Punch ? punchRange : 1.5f));
        }
        else
        {
            warning.positionCount = 2;
            warning.SetPosition(0, origin);
            var distance = CurrentAttack == AttackKind.Charge ? chargeRange : 2.6f;
            if (Mathf.Abs(lockedDirection.x) > 0.0001f)
                distance = Mathf.Min(distance, (arenaCenter.x + Mathf.Sign(lockedDirection.x) * arenaHalfSize.x - origin.x) / lockedDirection.x);
            if (Mathf.Abs(lockedDirection.z) > 0.0001f)
                distance = Mathf.Min(distance, (arenaCenter.z + Mathf.Sign(lockedDirection.z) * arenaHalfSize.y - origin.z) / lockedDirection.z);
            distance = MovementClearance(lockedDirection, Mathf.Max(0f, distance));
            var end = origin + lockedDirection * Mathf.Max(0f, distance);
            warning.SetPosition(1, end);
            if (CurrentAttack == AttackKind.Charge && distance > 0.5f)
            {
                var side = Vector3.Cross(Vector3.up, lockedDirection);
                var halfWidth = warning.startWidth * 0.5f;
                AddWarningLine(marker.transform, new[] { origin - side * halfWidth, end - side * halfWidth }, 0.1f);
                AddWarningLine(marker.transform, new[] { origin + side * halfWidth, end + side * halfWidth }, 0.1f);
                var arrowLength = Mathf.Min(1.2f, distance * 0.4f);
                AddWarningLine(marker.transform, new[] { end - lockedDirection * arrowLength - side * halfWidth, end, end - lockedDirection * arrowLength + side * halfWidth }, 0.2f);
                for (var step = 3f; step < distance - 0.75f; step += 3f)
                {
                    var tip = origin + lockedDirection * step;
                    AddWarningLine(marker.transform, new[] { tip - lockedDirection * 0.8f - side * halfWidth * 0.8f, tip, tip - lockedDirection * 0.8f + side * halfWidth * 0.8f }, 0.14f);
                }
            }
        }
    }

    private void AddWarningLine(Transform parent, Vector3[] points, float width)
    {
        var child = new GameObject("Charge direction outline");
        child.transform.SetParent(parent, false);
        var line = child.AddComponent<LineRenderer>();
        line.sharedMaterial = warningMaterial;
        line.useWorldSpace = true;
        line.alignment = LineAlignment.TransformZ;
        line.startWidth = line.endWidth = width;
        line.startColor = line.endColor = new Color(1f, 0.8f, 0.15f, 1f);
        line.shadowCastingMode = UnityEngine.Rendering.ShadowCastingMode.Off;
        line.receiveShadows = false;
        line.positionCount = points.Length;
        for (var i = 0; i < points.Length; i++) line.SetPosition(i, points[i] + Vector3.up * 0.005f);
    }

    private void ClearWarning() { if (warning != null) Destroy(warning.gameObject); warning = null; }

    private void Play(string name, bool restart = false)
    {
        if (animationPlayer == null || !clips.TryGetValue(name, out var clip)) return;
        if (!restart && playingClip == clip) return;
        playingClip = clip;
        if (restart) animationPlayer.Play(clip); else animationPlayer.CrossFade(clip, 0.12f);
    }

    private void OnHurt(float remaining)
    {
        if (remaining > 0f && phase == Phase.Recovery) Play("HitReact", true);
    }

    private void Die()
    {
        IsActive = false;
        phase = Phase.Dead;
        ClearWarning();
        if (visual != null)
        {
            visual.SetParent(null, true);
            var animation = visual.GetComponentInChildren<Animation>();
            if (animation != null && clips.TryGetValue("Death", out var clip)) animation.Play(clip);
            Destroy(visual.gameObject, 1.5f);
        }
        if (encounter != null) encounter.Complete();
    }

    private void OnDestroy()
    {
        ClearWarning();
        if (health != null) { health.Died -= Die; health.HealthChanged -= OnHurt; }
    }
}
