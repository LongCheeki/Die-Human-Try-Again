using UnityEngine;
using UnityEngine.SceneManagement;

[RequireComponent(typeof(BoxCollider))]
public sealed class LevelExit : MonoBehaviour
{
    public BossEncounter encounter;
    public RoomGate gate;
    public string destinationScene = "";
    public bool requireSceneClear;
    private bool unlocked;
    private bool loading;

    private void Start() { gate.SetOpen(false, true); }

    private void Update()
    {
        if (!unlocked && CanUnlock())
        {
            unlocked = true;
            gate.SetOpen(true);
        }
    }

    private bool CanUnlock()
    {
        if (!requireSceneClear) return encounter != null && encounter.IsComplete;
        foreach (var enemy in FindObjectsByType<EnemyHealth>(FindObjectsInactive.Include, FindObjectsSortMode.None))
            if (enemy.gameObject.scene == gameObject.scene) return false;
        return true;
    }

    private void OnTriggerEnter(Collider other) { TryTravel(other); }
    private void OnTriggerStay(Collider other) { TryTravel(other); }

    private void TryTravel(Collider other)
    {
        if (!unlocked || loading || Time.timeScale == 0 || string.IsNullOrWhiteSpace(destinationScene)) return;
        var player = other.GetComponentInParent<PlayerHealth>();
        if (player == null || player.GetCurrentHealth() <= 0 || !Application.CanStreamedLevelBeLoaded(destinationScene)) return;
        loading = true;
        SceneManager.LoadSceneAsync(destinationScene);
    }
}
