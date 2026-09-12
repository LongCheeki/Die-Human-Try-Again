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
    public bool IsOpen { get; private set; }

    private void Awake() { SetOpen(initiallyOpen, true); }

    public void SetOpen(bool open, bool instant = false)
    {
        IsOpen = open;
        if (bars != null && hideWhenOpen) bars.gameObject.SetActive(!open);
        if (blocker != null) blocker.enabled = !open;
        if (statusLight != null) statusLight.color = open ? new Color(0.2f, 1f, 0.65f) : new Color(1f, 0.2f, 0.08f);
        if (bars != null && hideWhenOpen) bars.localPosition = Vector3.zero;
        else if (instant && bars != null) bars.localPosition = Vector3.up * (open ? liftHeight : 0f);
    }

    private void Update()
    {
        if (bars != null && !hideWhenOpen)
            bars.localPosition = Vector3.MoveTowards(bars.localPosition,
                Vector3.up * (IsOpen ? liftHeight : 0f), liftSpeed * Time.deltaTime);
    }
}
