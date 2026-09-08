using UnityEngine;

public class WeaponPickup : MonoBehaviour
{
    [Header("Weapon")]
    public string weaponDisplayName = "New Weapon";

    public GameObject weaponPrefab;

    [Header("References")]
    public WeaponChoiceUI choiceUI;

    private bool hasTriggered = false;

    private void OnTriggerEnter(
        Collider other)
    {
        if (hasTriggered)
            return;

        if (!other.CompareTag("Player"))
            return;

        hasTriggered = true;

        if (choiceUI != null)
        {
            choiceUI.OpenChoice(this);
        }
    }

    public void RemovePickup()
    {
        Destroy(gameObject);
    }

    public void CancelPickup()
    {
        hasTriggered = false;
    }
}