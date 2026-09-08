using UnityEngine;
using UnityEngine.InputSystem;

public class ReadableObject : MonoBehaviour
{
    [Header("Text")]
    public string title = "Tutorial";

    [TextArea(3, 10)]
    public string content;

    [Header("References")]
    public ReadableUI readableUI;

    private bool playerNearby = false;

    private bool waitForERelease = false;

    void Update()
    {
        if (!playerNearby)
            return;

        if (readableUI == null)
            return;

        if (Keyboard.current == null)
            return;

        // 打开之后，必须先松开一次 E
        if (waitForERelease)
        {
            if (!Keyboard.current.eKey.isPressed)
            {
                waitForERelease = false;
            }

            return;
        }

        if (Keyboard.current.eKey.wasPressedThisFrame)
        {
            if (readableUI.IsReading())
            {
                readableUI.CloseReadPanel();
            }
            else
            {
                readableUI.OpenReadPanel(
                    title,
                    content
                );

                waitForERelease = true;
            }
        }
    }

    private void OnTriggerEnter(Collider other)
    {
        if (!other.CompareTag("Player"))
            return;

        playerNearby = true;

        if (readableUI != null)
            readableUI.ShowHint();
    }

    private void OnTriggerExit(Collider other)
    {
        if (!other.CompareTag("Player"))
            return;

        playerNearby = false;

        if (readableUI != null)
            readableUI.HideHint();
    }
}