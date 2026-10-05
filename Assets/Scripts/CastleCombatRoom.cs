using UnityEngine;

[RequireComponent(typeof(BoxCollider))]
public sealed class CastleCombatRoom : MonoBehaviour
{
    public RoomGate[] gates;
    public EnemyHealth[] enemies;
    public GameObject[] clearRewards;
    public CastleCombatRoom prerequisite;
    public bool IsFighting { get; private set; }
    public bool IsCleared { get; private set; }
    private BoxCollider area;
    private void Awake()
    {
        area = GetComponent<BoxCollider>();
        area.isTrigger = true;
        foreach (var enemy in enemies) if (enemy != null) enemy.gameObject.SetActive(false);
        foreach (var reward in clearRewards) if (reward != null) reward.SetActive(false);
    }
    private void Start() { SetGates(true); }
    private void OnTriggerEnter(Collider other) { TryBegin(other); }
    private void OnTriggerStay(Collider other) { TryBegin(other); }
    private void TryBegin(Collider other)
    {
        if (IsFighting || IsCleared || Time.timeScale == 0f) return;
        if (prerequisite != null && !prerequisite.IsCleared) return;
        var player = other.GetComponentInParent<PlayerHealth>();
        if (player == null || player.GetCurrentHealth() <= 0f) return;
        if (!area.bounds.Contains(player.transform.position + Vector3.up * 0.5f)) return;
        IsFighting = true;
        SetGates(false);
        foreach (var enemy in enemies)
        {
            if (enemy == null) continue;
            var melee = enemy.GetComponent<EnemyAI>();
            if (melee != null) { melee.player = player.transform; melee.detectionRange = 40f; }
            var archer = enemy.GetComponent<ArcherEnemyAI>();
            if (archer != null) { archer.player = player.transform; archer.detectionRange = 40f; }
            enemy.gameObject.SetActive(true);
        }
    }
    private void Update()
    {
        if (!IsFighting) return;
        foreach (var enemy in enemies) if (enemy != null) return;
        IsFighting = false;
        IsCleared = true;
        SetGates(true);
        foreach (var reward in clearRewards) if (reward != null) reward.SetActive(true);
    }
    private void SetGates(bool open)
    {
        foreach (var gate in gates) if (gate != null) gate.SetOpen(open, !Application.isPlaying);
    }
}
