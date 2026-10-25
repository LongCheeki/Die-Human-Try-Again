using TMPro;
using UnityEngine;

public sealed class PlayerFormBar : MonoBehaviour
{
    [System.Serializable]
    public sealed class Slot
    {
        public UnityEngine.UI.Image frame;
        public UnityEngine.UI.Image portrait;
        public UnityEngine.UI.Image keycap;
        public TMP_Text keyLabel;
    }

    public PlayerMimic playerMimic;
    public Slot[] slots;
    public Color selectedColor = new Color32(150, 207, 240, 255);
    public Color frameColor = new Color32(54, 55, 57, 255);
    public Color keycapColor = new Color32(226, 213, 179, 255);
    private int displayedState = -1;

    private void OnEnable()
    {
        displayedState = -1;
        Refresh();
    }

    private void LateUpdate() { Refresh(); }

    public void Refresh()
    {
        if (playerMimic == null || slots == null || slots.Length != 3) return;
        int unlocked = (playerMimic.warriorUnlocked ? 1 : 0) |
            (playerMimic.archerUnlocked ? 2 : 0) |
            (playerMimic.mageUnlocked ? 4 : 0);
        int selected = playerMimic.IsWarrior() ? 1 : playerMimic.IsArcher() ? 2 :
            playerMimic.IsMage() ? 3 : 0;
        int state = unlocked | (selected << 3);
        if (state == displayedState) return;
        displayedState = state;
        for (int i = 0; i < slots.Length; i++)
        {
            bool available = (unlocked & (1 << i)) != 0;
            bool active = available && selected == i + 1;
            var slot = slots[i];
            slot.portrait.enabled = available;
            slot.frame.color = active ? selectedColor : frameColor;
            slot.keycap.color = active ? selectedColor : keycapColor;
            if (!available)
            {
                var color = slot.keycap.color;
                color.a = 0.45f;
                slot.keycap.color = color;
            }
            slot.keyLabel.color = available ? new Color32(25, 28, 30, 255) :
                new Color32(95, 98, 101, 255);
        }
    }
}
