using UnityEngine;

public class SwordHitbox : MonoBehaviour
{
    [Header("Damage")]
    public int slimeDamage = 1;
    public int warriorDamage = 5;

    [Header("Run Bonus")]
    public int damageBonus = 0;

    [Header("References")]
    public PlayerMimic playerMimic;

    private bool canDamage = false;

    private BoxCollider weaponCollider;
    private Vector3 originalSize;
    private Vector3 originalCenter;

    public void SetWarriorRange(bool warrior)
    {
        if (weaponCollider == null)
        {
            weaponCollider = GetComponent<BoxCollider>();
            if (weaponCollider == null) return;
            originalSize = weaponCollider.size;
            originalCenter = weaponCollider.center;
        }
        int axis = originalSize.x >= originalSize.y && originalSize.x >= originalSize.z ? 0 :
            originalSize.y >= originalSize.z ? 1 : 2;
        var size = originalSize;
        var center = originalCenter;
        if (warrior)
        {
            size[axis] *= 1.4f;
            center[axis] += (size[axis] - originalSize[axis]) * 0.5f *
                (originalCenter[axis] < 0f ? -1f : 1f);
        }
        weaponCollider.size = size;
        weaponCollider.center = center;
    }
    public void EnableDamage()
    {
        canDamage = true;
    }

    public void DisableDamage()
    {
        canDamage = false;
    }

    private void OnTriggerStay(Collider other) { OnTriggerEnter(other); }

    private void OnTriggerEnter(Collider other)
    {
        if (!canDamage)
            return;

        var barrel = other.GetComponentInParent<ExplosiveBarrel>();
        if (barrel != null)
        {
            canDamage = false;
            barrel.Explode(GetComponentInParent<PlayerHealth>());
            return;
        }

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
            knockbackDirection,
            playerMimic
        );

        Debug.Log(
            "Sword Damage: " +
            finalDamage
        );

        // 一挥只命中一次
        canDamage = false;
    }
}