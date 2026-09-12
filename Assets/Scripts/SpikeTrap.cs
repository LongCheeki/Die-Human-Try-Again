using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(BoxCollider))]
public sealed class SpikeTrap : MonoBehaviour
{
    [Min(0.01f)] public float damageInterval = 1f;
    [Min(1)] public int damagePerTick = 1;
    [Min(0.01f)] public float enemyDamageInterval = 2f;
    [Min(0.01f)] public float enemyDamagePerTick = 0.5f;
    private BoxCollider area;
    private readonly Dictionary<Component, float> timers = new Dictionary<Component, float>();
    private readonly HashSet<Component> occupants = new HashSet<Component>();
    private readonly List<Component> expired = new List<Component>();

    private void Awake() { area = GetComponent<BoxCollider>(); area.isTrigger = true; }
    private void OnDisable() { timers.Clear(); occupants.Clear(); }

    private void FixedUpdate()
    {
        occupants.Clear();
        var size = Vector3.Scale(area.size, transform.lossyScale) * 0.5f;
        var hits = Physics.OverlapBox(transform.TransformPoint(area.center), size,
            transform.rotation, ~0, QueryTriggerInteraction.Ignore);
        foreach (var hit in hits)
        {
            if (!Physics.ComputePenetration(area, transform.position, transform.rotation,
                hit, hit.transform.position, hit.transform.rotation, out _, out _)) continue;
            var player = hit.GetComponentInParent<PlayerHealth>();
            if (player != null && player.GetCurrentHealth() > 0) { occupants.Add(player); continue; }
            var enemy = hit.GetComponentInParent<EnemyHealth>();
            if (enemy != null && enemy.GetCurrentHealth() > 0) occupants.Add(enemy);
        }
        expired.Clear();
        foreach (var pair in timers) if (pair.Key == null || !occupants.Contains(pair.Key)) expired.Add(pair.Key);
        foreach (var key in expired) timers.Remove(key);
        foreach (var target in occupants)
        {
            if (!timers.TryGetValue(target, out float elapsed))
            {
                timers[target] = 0;
                if (target is PlayerHealth) DealDamage(target);
                continue;
            }
            elapsed += Time.fixedDeltaTime;
            float interval = Mathf.Max(0.01f, target is EnemyHealth ? enemyDamageInterval : damageInterval);
            while (elapsed + 0.00001f >= interval && target != null)
            {
                elapsed -= interval;
                DealDamage(target);
            }
            timers[target] = Mathf.Max(0, elapsed);
        }
    }

    private void DealDamage(Component target)
    {
        if (target is PlayerHealth player) player.TakeTrapDamage(damagePerTick);
        else if (target is EnemyHealth enemy) enemy.TakeDamage(enemyDamagePerTick, Vector3.zero);
    }
}
