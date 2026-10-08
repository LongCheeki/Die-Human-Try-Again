using UnityEngine;
using System.Collections;

public class MageBossWall : MonoBehaviour
{
    [Header("Wall Timing")]
    public float stationaryTime = 2f;
    public float moveDuration = 4f;

    [Header("Movement")]
    public float moveSpeed = 3f;

    private Transform player;

    private bool isMoving = false;

    private Vector3 moveDirection;

    public void Setup(Transform target)
    {
        player = target;

        StartCoroutine(
            WallSequence()
        );
    }

    IEnumerator WallSequence()
    {
        // =========================
        // WALL STAYS STILL
        // =========================

        yield return new WaitForSeconds(
            stationaryTime
        );

        if (player != null)
        {
            // 锁定这一瞬间玩家的位置
            moveDirection =
                player.position -
                transform.position;

            moveDirection.y = 0f;

            moveDirection.Normalize();
        }

        isMoving = true;

        // =========================
        // MOVING PERIOD
        // =========================

        yield return new WaitForSeconds(
            moveDuration
        );

        Destroy(gameObject);
    }

    void Update()
    {
        if (!isMoving)
            return;

        transform.position +=
            moveDirection *
            moveSpeed *
            Time.deltaTime;
    }
}