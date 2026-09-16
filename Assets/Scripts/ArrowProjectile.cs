using UnityEngine;

public class ArrowProjectile : MonoBehaviour
{
    [Header("Arrow")]
    public float speed = 8f;
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

        if (!other.isTrigger)
        {
            Destroy(gameObject);
        }
    }
}