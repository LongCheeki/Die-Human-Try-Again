using UnityEngine;
using UnityEngine.InputSystem;

public sealed class SecretRoomEntrance : MonoBehaviour
{
    public CastleCombatRoom prerequisite;
    public GameObject wall;
    public GameObject hint;
    public float interactionDistance = 2.5f;
    private Transform player;
    private bool opened;
    private void Start() { var health = FindFirstObjectByType<PlayerHealth>(); if (health != null) player = health.transform; }
    private void Update()
    {
        if (opened || player == null) return;
        var near = Vector3.Distance(player.position, transform.position) <= interactionDistance;
        if (hint != null) hint.SetActive(near && prerequisite.IsCleared);
        if (Time.timeScale == 0f || !near || !prerequisite.IsCleared || Keyboard.current == null || !Keyboard.current.eKey.wasPressedThisFrame) return;
        opened = true;
        wall.SetActive(false);
        if (hint != null) hint.SetActive(false);
    }
}
