using UnityEngine;

public class ArcherMimicPickup : MonoBehaviour
{
    private void OnTriggerEnter(Collider other)
    {
        if (!other.CompareTag("Player"))
            return;

        PlayerMimic playerMimic =
            other.GetComponent<PlayerMimic>();

        if (playerMimic == null)
            return;

        playerMimic.UnlockArcher();

        Debug.Log(
            "Picked up Archer Mimic Drop!"
        );

        Destroy(gameObject);
    }
}