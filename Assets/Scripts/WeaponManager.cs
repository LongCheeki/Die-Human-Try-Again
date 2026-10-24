using UnityEngine;

public class WeaponManager : MonoBehaviour
{
    [Header("Weapon")]
    public Transform weaponPivot;
    public GameObject startingWeapon;

    [Header("Combat")]
    public PlayerCombat playerCombat;
    public PlayerMimic playerMimic;

    [Header("Sacrifice Upgrade")]
    public int sacrificeDamageBonus = 0;

    private GameObject currentWeapon;

    void Start()
    {
        if (startingWeapon != null)
        {
            EquipWeapon(startingWeapon);
        }
    }

    public void EquipWeapon(GameObject weaponPrefab)
    {
        // 删除旧武器
        var nextWeapon = weaponPrefab.transform.IsChildOf(weaponPivot)
            ? weaponPrefab : Instantiate(weaponPrefab, weaponPivot);
        foreach (Transform child in weaponPivot)
        {
            if (child.gameObject == nextWeapon || child.GetComponentInChildren<SwordHitbox>(true) == null) continue;
            child.gameObject.SetActive(false);
            Destroy(child.gameObject);
        }
        currentWeapon = nextWeapon;
        currentWeapon.SetActive(true);

        currentWeapon.transform.localPosition =
            Vector3.zero;

        currentWeapon.transform.localRotation =
            Quaternion.identity;

        // 找到新武器的 Hitbox
        SwordHitbox newHitbox =
            currentWeapon.GetComponentInChildren<SwordHitbox>();

        if (newHitbox != null)
        {
            newHitbox.playerMimic =
                playerMimic;

            newHitbox.damageBonus =
                sacrificeDamageBonus;
        }

        // 告诉 PlayerCombat 新武器是谁
        if (playerCombat != null)
        {
            playerCombat.SetWeapon(
                currentWeapon.transform,
                newHitbox
            );
        }

        if (playerMimic != null) playerMimic.RefreshWeaponVisibility();

        Debug.Log(
            "Equipped: " +
            weaponPrefab.name
        );
    }

    public void SacrificeWeapon()
    {
        sacrificeDamageBonus += 1;

        // 当前装备也更新Buff
        if (currentWeapon != null)
        {
            SwordHitbox hitbox =
                currentWeapon.GetComponentInChildren<SwordHitbox>();

            if (hitbox != null)
            {
                hitbox.damageBonus =
                    sacrificeDamageBonus;
            }
        }

        Debug.Log(
            "Weapon sacrificed! Damage Bonus: +" +
            sacrificeDamageBonus
        );
    }

    public GameObject GetCurrentWeapon()
    {
        return currentWeapon;
    }

    public void ResetRunUpgrades()
    {
        sacrificeDamageBonus = 0;

        if (currentWeapon != null)
        {
            SwordHitbox hitbox =
                currentWeapon.GetComponentInChildren<SwordHitbox>();

            if (hitbox != null)
            {
                hitbox.damageBonus = 0;
            }
        }
    }
}