using UnityEngine;

public sealed class GolelingProjectile : MonoBehaviour
{
    public float speed = 7f;
    public float damage = 1f;
    public float radius = 0.16f;
    public float lifeTime = 5f;
    private Vector3 direction;
    private GolelingBossAI owner;
    private float elapsed;

    public void Initialize(Vector3 heading, GolelingBossAI source)
    {
        direction = heading.normalized;
        owner = source;
    }

    private void FixedUpdate()
    {
        if (Time.timeScale == 0f) return;
        if (owner == null || !owner.IsActive) { Destroy(gameObject); return; }
        elapsed += Time.fixedDeltaTime;
        if (elapsed >= lifeTime) { Destroy(gameObject); return; }
        foreach (var collider in Physics.OverlapSphere(transform.position, radius, ~0, QueryTriggerInteraction.Ignore))
        {
            if (collider.GetComponentInParent<GolelingBossAI>() == owner) continue;
            var player = collider.GetComponentInParent<PlayerHealth>();
            if (player != null) player.TakeDamage(damage, direction);
            Destroy(gameObject);
            return;
        }
        var distance = speed * Time.fixedDeltaTime;
        var hits = Physics.SphereCastAll(transform.position, radius, direction, distance, ~0, QueryTriggerInteraction.Ignore);
        System.Array.Sort(hits, (a, b) => a.distance.CompareTo(b.distance));
        foreach (var hit in hits)
        {
            if (hit.collider.GetComponentInParent<GolelingBossAI>() == owner) continue;
            var player = hit.collider.GetComponentInParent<PlayerHealth>();
            if (player != null) player.TakeDamage(damage, direction);
            Destroy(gameObject);
            return;
        }
        transform.position += direction * distance;
    }
}
