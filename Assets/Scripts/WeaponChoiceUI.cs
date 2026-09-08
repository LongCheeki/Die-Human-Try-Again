using UnityEngine;
using TMPro;

public class WeaponChoiceUI : MonoBehaviour
{
    [Header("UI")]
    public GameObject choicePanel;
    public TMP_Text weaponNameText;

    [Header("References")]
    public WeaponManager weaponManager;
    public PlayerMovement playerMovement;
    public PlayerCombat playerCombat;

    private WeaponPickup currentPickup;

    void Start()
    {
        if (choicePanel != null)
        {
            choicePanel.SetActive(false);
        }
    }

    // =========================
    // OPEN UI
    // =========================

    public void OpenChoice(
        WeaponPickup pickup)
    {
        currentPickup = pickup;

        if (choicePanel != null)
        {
            choicePanel.SetActive(true);
        }

        // ÏÔÊ¾ÎäÆ÷Ãû×Ö
        if (weaponNameText != null &&
            currentPickup != null)
        {
            weaponNameText.text =
                currentPickup.weaponDisplayName;
        }

        // ½ûÖ¹ÒÆ¶¯
        if (playerMovement != null)
        {
            playerMovement.enabled = false;
        }

        // ½ûÖ¹¹¥»÷
        if (playerCombat != null)
        {
            playerCombat.enabled = false;
        }

        Cursor.visible = true;
        Cursor.lockState =
            CursorLockMode.None;
    }

    // =========================
    // USE
    // =========================

    public void ChooseUse()
    {
        if (currentPickup == null)
            return;

        if (weaponManager != null)
        {
            weaponManager.EquipWeapon(
                currentPickup.weaponPrefab
            );
        }

        currentPickup.RemovePickup();

        CloseChoice();
    }

    // =========================
    // SACRIFICE
    // =========================

    public void ChooseSacrifice()
    {
        if (currentPickup == null)
            return;

        if (weaponManager != null)
        {
            weaponManager.SacrificeWeapon();
        }

        currentPickup.RemovePickup();

        CloseChoice();
    }

    // =========================
    // CLOSE
    // =========================

    void CloseChoice()
    {
        currentPickup = null;

        if (choicePanel != null)
        {
            choicePanel.SetActive(false);
        }

        if (playerMovement != null)
        {
            playerMovement.enabled = true;
        }

        if (playerCombat != null)
        {
            playerCombat.enabled = true;
        }
    }
}