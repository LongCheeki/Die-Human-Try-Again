using UnityEngine;

public class PlayerMagicProjectile : MonoBehaviour
{
    [Header("Projectile")]
    public float speed = 10f;
    public int damage = 3;
    public float lifeTime = 6f;

    private Vector3 moveDirection;

    void Start()
    {
        Destroy(gameObject, lifeTime);
    }

    public void SetDirection(Vector3 direction)
    {
        moveDirection =
            direction.normalized;

        if (moveDirection.sqrMagnitude > 0.001f)
        {
            transform.rotation =
                Quaternion.LookRotation(
                    moveDirection
                );
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
        // 不打玩家
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

        // 撞墙/地面
        if (!other.isTrigger)
        {
            Destroy(gameObject);
        }
    }
}