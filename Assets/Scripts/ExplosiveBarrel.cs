using System.Collections.Generic;
using UnityEngine;

public sealed class ExplosiveBarrel : MonoBehaviour
{
    public float blastRadius = 2f;
    public float playerDamage = 2f;
    public float bossDamage = 5f;
    public GameObject explosionEffect;
    private bool exploded;
    public void Explode(PlayerHealth meleeAttacker = null)
    {
        if (exploded) return;
        exploded = true;
        var origin = transform.position;
        if (explosionEffect != null)
        {
            var effect = Instantiate(explosionEffect, origin + Vector3.up * 0.5f, Quaternion.identity);
            effect.SetActive(true);
            Destroy(effect, 3f);
        }
        var players = new HashSet<PlayerHealth>();
        var enemies = new HashSet<EnemyHealth>();
        Physics.SyncTransforms();
        if (meleeAttacker != null) players.Add(meleeAttacker);
        foreach (var hit in Physics.OverlapSphere(origin + Vector3.up, blastRadius + 2f, ~0, QueryTriggerInteraction.Collide))
        {
            var player = hit.GetComponentInParent<PlayerHealth>();
            var enemy = hit.GetComponentInParent<EnemyHealth>();
            if (player != null && Inside(player, origin)) players.Add(player);
            if (enemy != null && Inside(enemy, origin)) enemies.Add(enemy);
        }
        foreach (var player in players)
        {
            var mimic = player.GetComponent<PlayerMimic>();
            player.TakeTrapDamage(mimic != null && mimic.IsWarrior() ? 0.5f : playerDamage);
        }
        foreach (var enemy in enemies)
            enemy.TakeDamage(enemy.GetComponent<GolelingBossAI>() != null ? bossDamage : float.MaxValue, Vector3.zero);
        Destroy(gameObject);
    }
    private bool Inside(Component target, Vector3 origin)
    {
        foreach (var collider in target.GetComponentsInChildren<Collider>())
        {
            if (!collider.enabled || collider.isTrigger) continue;
            var sample = collider.ClosestPoint(origin + Vector3.up * 0.6f);
            var delta = sample - origin;
            if (delta.x * delta.x + delta.z * delta.z <= blastRadius * blastRadius && Mathf.Abs(delta.y) < 3f) return true;
        }
        return false;
    }
}
