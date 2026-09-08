using UnityEngine;
using TMPro;

public class ReadableUI : MonoBehaviour
{
    [Header("UI")]
    public GameObject readPanel;
    public GameObject interactHint;

    public TMP_Text titleText;
    public TMP_Text contentText;

    private bool isReading = false;

    void Start()
    {
        if (readPanel != null)
            readPanel.SetActive(false);

        if (interactHint != null)
            interactHint.SetActive(false);
    }

    public void ShowHint()
    {
        if (isReading)
            return;

        if (interactHint != null)
            interactHint.SetActive(true);
    }

    public void HideHint()
    {
        if (interactHint != null)
            interactHint.SetActive(false);
    }

    public void OpenReadPanel(string title, string content)
    {
        if (isReading)
            return;

        isReading = true;

        HideHint();

        if (titleText != null)
            titleText.text = title;

        if (contentText != null)
            contentText.text = content;

        if (readPanel != null)
            readPanel.SetActive(true);

        Time.timeScale = 0f;

        Cursor.visible = true;
        Cursor.lockState = CursorLockMode.None;
    }

    public void CloseReadPanel()
    {
        if (!isReading)
            return;

        isReading = false;

        if (readPanel != null)
            readPanel.SetActive(false);

        Time.timeScale = 1f;
    }

    public bool IsReading()
    {
        return isReading;
    }
}