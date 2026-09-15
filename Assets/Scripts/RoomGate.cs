using UnityEngine;

public sealed class RoomGate : MonoBehaviour
{
    public Transform bars;
    public Collider blocker;
    public Light statusLight;
    public bool initiallyOpen;
    public bool hideWhenOpen;
    public float liftHeight = 4.2f;
    public float liftSpeed = 7f;
    public Renderer[] tallVisuals;
    public GameObject closedMarker;
    public bool IsOpen { get; private set; }
    private BattleRoom[] rooms;
    private BossEncounter bossEncounter;

    private void Awake()
    {
        rooms = FindObjectsByType<BattleRoom>(FindObjectsSortMode.None);
        bossEncounter = FindFirstObjectByType<BossEncounter>();
        SetOpen(initiallyOpen, true);
    }

    public void SetOpen(bool open, bool instant = false)
    {
        IsOpen = open;
        if (closedMarker != null)
        {
            closedMarker.SetActive(!open);
            SetTallVisuals(open && !IsCombat());
        }
        if (bars != null && hideWhenOpen) bars.gameObject.SetActive(!open);
        if (blocker != null) blocker.enabled = !open;
        if (statusLight != null) statusLight.color = open ? new Color(0.2f, 1f, 0.65f) : new Color(1f, 0.2f, 0.08f);
        if (bars != null && hideWhenOpen) bars.localPosition = Vector3.zero;
        else if (instant && bars != null) bars.localPosition = Vector3.up * (open ? liftHeight : 0f);
    }

    private void Update()
    {
        if (closedMarker != null) SetTallVisuals(IsOpen && !IsCombat());
        if (bars != null && !hideWhenOpen)
            bars.localPosition = Vector3.MoveTowards(bars.localPosition,
                Vector3.up * (IsOpen ? liftHeight : 0f), liftSpeed * Time.deltaTime);
    }

    private bool IsCombat()
    {
        if (bossEncounter != null && bossEncounter.IsFighting) return true;
        if (rooms != null)
            foreach (var room in rooms)
                if (room != null && room.State == BattleRoom.RoomState.Fighting) return true;
        return false;
    }

    private void SetTallVisuals(bool visible)
    {
        if (tallVisuals == null) return;
        foreach (var visual in tallVisuals)
            if (visual != null && visual.enabled != visible) visual.enabled = visible;
    }
}
