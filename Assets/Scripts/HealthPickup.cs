using UnityEngine;

public class HealthPickup : MonoBehaviour
{
    [Header("Healing")]
    public float healAmount = 2f;

    [Header("Optional Effect")]
    public GameObject pickupEffect;

    private void OnTriggerEnter(Collider other)
    {
        if (!other.CompareTag("Player"))
            return;

        PlayerHealth playerHealth =
            other.GetComponent<PlayerHealth>();

        if (playerHealth == null)
            return;

        // 满血时不吃掉回血道具
        if (playerHealth.GetCurrentHealth() >=
            playerHealth.GetMaxHealth())
        {
            return;
        }

        playerHealth.Heal(healAmount);

        if (pickupEffect != null)
        {
            GameObject effect = Instantiate(
                pickupEffect,
                transform.position,
                Quaternion.identity
            );

            Destroy(effect, 2f);
        }

        Destroy(gameObject);
    }
}