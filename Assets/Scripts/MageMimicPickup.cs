using UnityEngine;

public class MageMimicPickup : MonoBehaviour
{
    [Header("Optional Effect")]
    public GameObject pickupEffectPrefab;

    private bool pickedUp = false;

    private void OnTriggerEnter(Collider other)
    {
        if (pickedUp)
            return;

        if (!other.CompareTag("Player"))
            return;

        PlayerMimic playerMimic =
            other.GetComponent<PlayerMimic>();

        if (playerMimic == null)
        {
            playerMimic =
                other.GetComponentInParent<PlayerMimic>();
        }

        if (playerMimic == null)
        {
            Debug.LogWarning(
                "MageMimicPickup: PlayerMimic not found!"
            );

            return;
        }

        pickedUp = true;

        // 解锁法师形态
        playerMimic.UnlockMage();

        Debug.Log(
            "Player picked up Mage Mimic Orb!"
        );

        // 可选拾取特效
        if (pickupEffectPrefab != null)
        {
            GameObject effect =
                Instantiate(
                    pickupEffectPrefab,
                    transform.position,
                    Quaternion.identity
                );

            Destroy(effect, 2f);
        }

        Destroy(gameObject);
    }
}