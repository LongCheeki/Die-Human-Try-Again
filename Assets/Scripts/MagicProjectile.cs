using UnityEngine;

public class MagicProjectile : MonoBehaviour
{
    [Header("Projectile")]
    public float speed = 9f;
    public float damage = 1f;
    public float lifeTime = 6f;

    private Vector3 moveDirection;

    void Start()
    {
        Destroy(gameObject, lifeTime);
    }

    public void SetDirection(Vector3 direction)
    {
        moveDirection = direction.normalized;

        if (moveDirection.sqrMagnitude > 0.001f)
        {
            transform.rotation =
                Quaternion.LookRotation(moveDirection);
        }
    }

    void Update()
    {
        transform.position +=
            moveDirection *
            speed *
            Time.deltaTime;
    }

    private void OnTriggerEnter(Collider other)
    {
        // 不攻击其他敌人
        if (other.GetComponentInParent<EnemyHealth>() != null)
            return;

        // 击中玩家
        if (other.CompareTag("Player"))
        {
            PlayerHealth playerHealth =
                other.GetComponent<PlayerHealth>();

            if (playerHealth != null)
            {
                Vector3 knockbackDirection =
                    other.transform.position -
                    transform.position;

                knockbackDirection.y = 0f;

                playerHealth.TakeDamage(
                    damage,
                    knockbackDirection
                );
            }

            Destroy(gameObject);
            return;
        }

        // 撞墙/地面后销毁
        if (!other.isTrigger)
        {
            Destroy(gameObject);
        }
    }
}