using UnityEngine;

public sealed class BattleRoom : MonoBehaviour
{
    public enum RoomState { Waiting, Fighting, Cleared }
    public RoomGate entrance;
    public RoomGate exit;
    public BattleRoom previousRoom;
    public EnemyHealth[] enemies;
    public GameObject enemyTemplate;
    public Transform checkpoint;
    public GameObject reward;
    public RoomState State { get; private set; }

    private Vector3[] spawnPositions;
    private Quaternion[] spawnRotations;
    private int[] spawnHealth;
    private PlayerHealth player;
    private BoxCollider area;

    private void Awake()
    {
        area = GetComponent<BoxCollider>();
        spawnPositions = new Vector3[enemies.Length];
        spawnRotations = new Quaternion[enemies.Length];
        spawnHealth = new int[enemies.Length];
        for (int i = 0; i < enemies.Length; i++)
        {
            spawnPositions[i] = enemies[i].transform.position;
            spawnRotations[i] = enemies[i].transform.rotation;
            spawnHealth[i] = enemies[i].maxHealth;
            enemies[i].gameObject.SetActive(false);
        }
        if (reward != null) reward.SetActive(false);
    }

    private void Start()
    {
        entrance.SetOpen(true, true);
        exit.SetOpen(exit == entrance, true);
    }

    private void OnTriggerEnter(Collider other) { TryEnter(other); }
    private void OnTriggerStay(Collider other) { TryEnter(other); }

    private void TryEnter(Collider other)
    {
        if (State != RoomState.Waiting || Time.timeScale == 0f) return;
        var candidate = other.GetComponentInParent<PlayerHealth>();
        if (candidate == null || candidate.GetCurrentHealth() <= 0f) return;
        if (previousRoom != null && previousRoom.State != RoomState.Cleared) return;
        if (!area.bounds.Contains(candidate.transform.position + Vector3.up * 0.5f)) return;
        player = candidate;
        if (player.respawnPoint != null && checkpoint != null)
            player.respawnPoint.SetPositionAndRotation(checkpoint.position, checkpoint.rotation);
        State = RoomState.Fighting;
        entrance.SetOpen(false);
        exit.SetOpen(false);
        foreach (var enemy in enemies)
        {
            if (enemy == null) continue;
            var ai = enemy.GetComponent<EnemyAI>();
            ai.player = player.transform;
            ai.detectionRange = 40f;
            enemy.gameObject.SetActive(true);
        }
    }

    private void Update()
    {
        if (State != RoomState.Fighting) return;
        if (player == null || player.GetCurrentHealth() <= 0f)
        {
            ResetEncounter();
            return;
        }
        foreach (var enemy in enemies) if (enemy != null) return;
        State = RoomState.Cleared;
        entrance.SetOpen(true);
        exit.SetOpen(true);
        if (reward != null) reward.SetActive(true);
    }

    private void ResetEncounter()
    {
        for (int i = 0; i < enemies.Length; i++)
        {
            if (enemies[i] != null)
            {
                enemies[i].gameObject.SetActive(false);
                Destroy(enemies[i].gameObject);
            }
            var replacement = Instantiate(enemyTemplate, spawnPositions[i], spawnRotations[i], transform);
            replacement.name = "Guard " + (i + 1);
            replacement.SetActive(false);
            enemies[i] = replacement.GetComponent<EnemyHealth>();
            enemies[i].maxHealth = spawnHealth[i];
        }
        State = RoomState.Waiting;
        entrance.SetOpen(true);
        exit.SetOpen(exit == entrance);
    }
}
