using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(BoxCollider))]
public sealed class BossEncounter : MonoBehaviour
{
    public RoomGate gate;
    public BattleRoom previousRoom;
    public EnemyHealth boss;
    public GameObject bossTemplate;
    public GameObject minionTemplate;
    public BossWarningCircle circlePrefab;
    public Transform checkpoint;
    public LevelVictory victory;
    public float circleInterval = 5f;
    public bool IsFighting { get; private set; }
    public bool IsComplete { get; private set; }
    public int SummonedCount { get; private set; }
    private readonly List<GameObject> minions = new List<GameObject>();
    private readonly List<GameObject> circles = new List<GameObject>();
    private PlayerHealth player;
    private Vector3 bossSpawn;
    private Quaternion bossRotation;
    private float nextCircle;
    private int nextThreshold;
    private BoxCollider area;

    private void Awake()
    {
        area = GetComponent<BoxCollider>();
        bossSpawn = boss.transform.position;
        bossRotation = boss.transform.rotation;
        boss.gameObject.SetActive(false);
    }

    private void Start() { gate.SetOpen(true, true); }
    private void OnTriggerEnter(Collider other) { TryStart(other); }
    private void OnTriggerStay(Collider other) { TryStart(other); }

    private void TryStart(Collider other)
    {
        if (IsFighting || IsComplete || Time.timeScale == 0) return;
        if (previousRoom != null && previousRoom.State != BattleRoom.RoomState.Cleared) return;
        var candidate = other.GetComponentInParent<PlayerHealth>();
        if (candidate == null || candidate.GetCurrentHealth() <= 0 || !area.bounds.Contains(candidate.transform.position)) return;
        player = candidate;
        player.respawnPoint.SetPositionAndRotation(checkpoint.position, checkpoint.rotation);
        IsFighting = true;
        SummonedCount = 0;
        nextThreshold = 1;
        nextCircle = Time.time + circleInterval;
        boss.HealthChanged += OnBossDamage;
        boss.Died += OnBossDeath;
        boss.GetComponent<EnemyAI>().player = player.transform;
        boss.gameObject.SetActive(true);
        gate.SetOpen(false);
    }

    private void Update()
    {
        if (!IsFighting) return;
        if (player == null || player.GetCurrentHealth() <= 0) { ResetEncounter(); return; }
        if (Time.time >= nextCircle)
        {
            nextCircle = Time.time + circleInterval;
            var point = player.transform.position;
            point.y = -0.045f;
            var circle = Instantiate(circlePrefab, point, Quaternion.identity, transform);
            circle.Initialize(player, this);
            circles.Add(circle.gameObject);
        }
    }

    private void OnBossDamage(float health)
    {
        if (!IsFighting || health <= 0) return;
        while (nextThreshold <= 4 && health <= boss.maxHealth * (1f - nextThreshold / 5f) + 0.0001f)
        {
            Summon(nextThreshold);
            nextThreshold++;
        }
    }

    private void Summon(int wave)
    {
        Vector3[] offsets = { new Vector3(-4,0.76f,2), new Vector3(4,0.76f,2), new Vector3(-4,0.76f,-2), new Vector3(4,0.76f,-2) };
        var minion = Instantiate(minionTemplate, transform.position + offsets[wave-1], Quaternion.identity, transform);
        minion.name = "Boss reinforcement " + wave;
        minion.GetComponent<EnemyAI>().player = player.transform;
        minion.GetComponent<EnemyAI>().detectionRange = 40;
        minions.Add(minion);
        minion.SetActive(true);
        SummonedCount++;
    }

    private void OnBossDeath()
    {
        if (!IsFighting) return;
        IsFighting = false;
        IsComplete = true;
        Cleanup();
        gate.SetOpen(true, true);
        victory.Complete();
    }

    private void ResetEncounter()
    {
        IsFighting = false;
        Cleanup();
        boss.HealthChanged -= OnBossDamage;
        boss.Died -= OnBossDeath;
        boss.gameObject.SetActive(false);
        Destroy(boss.gameObject);
        var replacement = Instantiate(bossTemplate, bossSpawn, bossRotation, transform);
        boss = replacement.GetComponent<EnemyHealth>();
        replacement.SetActive(false);
        SummonedCount = 0;
        gate.SetOpen(true, true);
    }

    private void Cleanup()
    {
        foreach (var g in minions) if (g != null) { g.SetActive(false); Destroy(g); }
        foreach (var g in circles) if (g != null) { g.SetActive(false); Destroy(g); }
        minions.Clear();
        circles.Clear();
    }
}
