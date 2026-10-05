using UnityEngine;

public sealed class RoomEnemyBoundary : MonoBehaviour
{
    public Vector3 center;
    public Vector2 halfSize;
    private void LateUpdate()
    {
        var position = transform.position;
        position.x = Mathf.Clamp(position.x, center.x - halfSize.x, center.x + halfSize.x);
        position.z = Mathf.Clamp(position.z, center.z - halfSize.y, center.z + halfSize.y);
        transform.position = position;
    }
}
