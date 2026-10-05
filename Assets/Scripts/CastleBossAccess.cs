using UnityEngine;

public sealed class CastleBossAccess : MonoBehaviour
{
    public CastleCombatRoom[] rooms;
    public RoomGate[] gates;
    public CastleCombatRoom[] gatePrerequisites;
    private void Start() { foreach (var gate in gates) gate.SetOpen(false, true); }
    private void Update()
    {
        for (var i = 0; i < gates.Length; i++)
        {
            var gate = gates[i];
            if (gate == null || gate.IsOpen) continue;
            if (gatePrerequisites != null && i < gatePrerequisites.Length && gatePrerequisites[i] != null)
            {
                if (!gatePrerequisites[i].IsCleared) continue;
            }
            else
            {
                var allCleared = true;
                foreach (var room in rooms) if (room == null || !room.IsCleared) allCleared = false;
                if (!allCleared) continue;
            }
            gate.SetOpen(true);
        }
    }
}
