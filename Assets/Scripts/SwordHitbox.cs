using UnityEngine;

public class SwordHitbox : MonoBehaviour
{
    [Header("Damage")]
    public int slimeDamage = 1;
    public int warriorDamage = 2;

    [Header("Run Bonus")]
    public int damageBonus = 0;

    [Header("References")]
    public PlayerMimic playerMimic;

    private bool canDamage = false;

    public void EnableDamage()
    {
        canDamage = true;
    }

    public void DisableDamage()
    {
        canDamage = false;
    }

    private void OnTriggerEnter(Collider other)
    {
        if (!canDamage)
            return;

        EnemyHealth enemyHealth =
            other.GetComponentInParent<EnemyHealth>();

        if (enemyHealth == null)
            return;

        int finalDamage = slimeDamage;

        if (playerMimic != null &&
            playerMimic.IsWarrior())
        {
            finalDamage = warriorDamage;
        }

        // 加上献祭Buff
        finalDamage += damageBonus;

        Vector3 knockbackDirection =
            enemyHealth.transform.position -
            transform.root.position;

        knockbackDirection.y = 0f;

        enemyHealth.TakeDamage(
            finalDamage,
            knockbackDirection
        );

        Debug.Log(
            "Sword Damage: " +
            finalDamage
        );

        // 一挥只命中一次
        canDamage = false;
    }
}