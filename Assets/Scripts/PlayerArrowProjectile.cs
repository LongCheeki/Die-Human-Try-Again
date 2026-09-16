using UnityEngine;

public class PlayerArrowProjectile : MonoBehaviour
{
    [Header("Arrow")]
    public float speed = 12f;
    public int damage = 1;
    public float lifeTime = 6f;

    private Vector3 moveDirection;

    void Start()
    {
        Destroy(gameObject, lifeTime);
    }

    public void SetDirection(Vector3 direction)
    {
        moveDirection = direction.normalized;

        if (moveDirection != Vector3.zero)
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
        // 不打玩家自己
        if (other.CompareTag("Player"))
            return;

        EnemyHealth enemyHealth =
            other.GetComponentInParent<EnemyHealth>();

        if (enemyHealth != null)
        {
            Vector3 knockbackDirection =
                enemyHealth.transform.position -
                transform.position;

            knockbackDirection.y = 0f;

            enemyHealth.TakeDamage(
                damage,
                knockbackDirection
            );

            Destroy(gameObject);
            return;
        }

        // 撞到墙、地面等实体
        if (!other.isTrigger)
        {
            Destroy(gameObject);
        }
    }
}