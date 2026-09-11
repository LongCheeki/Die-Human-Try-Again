using UnityEngine;

public class HealthPickup : MonoBehaviour
{
    [Header("Healing")]
    public float healAmount = 2f;

    [Header("Optional Effect")]
    public GameObject pickupEffect;
    private bool consumed;

    private void OnTriggerEnter(Collider other)
    {
        if (consumed || Time.timeScale == 0f || !other.CompareTag("Player"))
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

        float before = playerHealth.GetCurrentHealth();
        if (before <= 0f || healAmount <= 0f) return;
        playerHealth.Heal(healAmount);
        if (playerHealth.GetCurrentHealth() <= before) return;
        consumed = true;
        CharacterSfx.Get(playerHealth.gameObject).HealPickup();

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