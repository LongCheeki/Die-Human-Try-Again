using UnityEngine;

public class TrapProjectile : MonoBehaviour
{
    [Header("Projectile")]
    public float speed = 4f;
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

        // 撞到普通场景物体后消失
        if (!other.isTrigger)
        {
            Destroy(gameObject);
        }
    }
}