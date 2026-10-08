using UnityEngine;
using UnityEngine.SceneManagement;

[RequireComponent(typeof(BoxCollider))]
public sealed class LevelExit : MonoBehaviour
{
    public BossEncounter encounter;
    public GolelingBossEncounter golelingEncounter;
    public EnemyHealth requiredBoss;
    public RoomGate gate;
    public string destinationScene = "";
    public bool requireSceneClear;
    private bool unlocked;
    private bool loading;
    private BoxCollider travelArea;
    private PlayerHealth currentPlayer;
    private bool bossDefeated;

    private void Start()
    {
        travelArea = GetComponent<BoxCollider>();
        gate.SetOpen(false, true);
        if (requiredBoss != null) requiredBoss.Died += OnBossDefeated;
    }

    private void OnBossDefeated() { bossDefeated = true; }

    private void OnDestroy()
    {
        if (requiredBoss != null) requiredBoss.Died -= OnBossDefeated;
    }

    private void Update()
    {
        if (!unlocked && CanUnlock())
        {
            unlocked = true;
            gate.SetOpen(true);
        }
        if (!unlocked || loading || Time.timeScale == 0) return;
        if (!gate.IsOpen) gate.SetOpen(true);
        if (currentPlayer == null) currentPlayer = FindFirstObjectByType<PlayerHealth>();
        if (currentPlayer == null || travelArea == null) return;
        var position = transform.InverseTransformPoint(currentPlayer.transform.position);
        var area = new Bounds(travelArea.center, travelArea.size);
        if (area.Contains(position)) TryTravel(currentPlayer);
    }

    private bool CanUnlock()
    {
        if (!requireSceneClear)
            return bossDefeated || (golelingEncounter != null ? golelingEncounter.IsComplete : encounter != null && encounter.IsComplete);
        foreach (var enemy in FindObjectsByType<EnemyHealth>(FindObjectsInactive.Include, FindObjectsSortMode.None))
            if (enemy.gameObject.scene == gameObject.scene) return false;
        return true;
    }

    private void OnTriggerEnter(Collider other) { TryTravel(other); }
    private void OnTriggerStay(Collider other) { TryTravel(other); }

    private void TryTravel(Collider other)
    {
        var player = other.GetComponentInParent<PlayerHealth>();
        TryTravel(player);
    }

    private void TryTravel(PlayerHealth player)
    {
        if (!unlocked || loading || Time.timeScale == 0 || string.IsNullOrWhiteSpace(destinationScene)) return;
        if (player == null || player.GetCurrentHealth() <= 0 || !Application.CanStreamedLevelBeLoaded(destinationScene)) return;
        loading = true;
        SceneManager.LoadSceneAsync(destinationScene);
    }
}
