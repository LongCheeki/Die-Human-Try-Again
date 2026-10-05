using UnityEngine;

[RequireComponent(typeof(BoxCollider))]
public sealed class GolelingBossEncounter : MonoBehaviour
{
    public GolelingBossAI boss;
    public CastleBossAccess access;
    public RoomGate[] gates;
    public bool IsFighting { get; private set; }
    public bool IsComplete { get; private set; }
    private BoxCollider area;

    private void Awake()
    {
        area = GetComponent<BoxCollider>();
        area.isTrigger = true;
        if (boss != null) boss.gameObject.SetActive(false);
    }

    private void OnTriggerEnter(Collider other) { TryBegin(other); }
    private void OnTriggerStay(Collider other) { TryBegin(other); }

    private void TryBegin(Collider other)
    {
        if (IsFighting || IsComplete || Time.timeScale == 0f || boss == null) return;
        var player = other.GetComponentInParent<PlayerHealth>();
        if (player == null || player.GetCurrentHealth() <= 0f) return;
        if (!area.bounds.Contains(player.transform.position + Vector3.up * 0.5f)) return;
        var routeCleared = access == null;
        if (access != null)
            foreach (var room in access.gatePrerequisites)
                if (room != null && room.IsCleared) routeCleared = true;
        if (!routeCleared) return;
        IsFighting = true;
        foreach (var gate in gates) if (gate != null) gate.SetOpen(false);
        boss.gameObject.SetActive(true);
        boss.Begin(player, this);
    }

    public void Complete()
    {
        if (!IsFighting) return;
        IsFighting = false;
        IsComplete = true;
        foreach (var gate in gates) if (gate != null) gate.SetOpen(true);
    }
}
