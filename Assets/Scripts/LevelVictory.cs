using UnityEngine;

public sealed class LevelVictory : MonoBehaviour
{
    public bool IsComplete { get; private set; }

    public void Complete()
    {
        if (IsComplete) return;
        IsComplete = true;
    }

}
