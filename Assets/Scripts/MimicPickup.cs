using UnityEngine;

public class MimicPickup : MonoBehaviour
{
    private void OnTriggerEnter(Collider other)
    {
        // 只有 Player 可以捡
        if (!other.CompareTag("Player"))
            return;

        PlayerMimic playerMimic =
            other.GetComponent<PlayerMimic>();

        if (playerMimic == null)
            return;

        // 解锁战士形态
        playerMimic.UnlockWarrior();

        Debug.Log("Picked up Warrior Mimic Drop!");

        // 掉落物消失
        Destroy(gameObject);
    }
}