using UnityEngine;

[DisallowMultipleComponent]
public sealed class FolloWCamera : MonoBehaviour
{
    private const string PlayerTag = "Player";

    [SerializeField, Min(0.01f)] private float smoothTime = 0.15f;

    private Transform target;
    private MapBoundaryController mapBoundary;
    private Vector3 velocity;
    private float fixedZ;

    private void Awake()
    {
        fixedZ = transform.position.z;
        TryFindTarget();
    }

    private void LateUpdate()
    {
        if (target == null && !TryFindTarget())
        {
            return;
        }

        Vector3 targetPosition = target.position;
        targetPosition.z = fixedZ;

        if (TryGetMapBoundary(out MapBoundaryController boundary))
        {
            Vector2 clampedTarget = boundary.ClampPoint(
                new Vector2(targetPosition.x, targetPosition.y),
                Vector2.zero);
            targetPosition.x = clampedTarget.x;
            targetPosition.y = clampedTarget.y;
        }

        transform.position = Vector3.SmoothDamp(
            transform.position,
            targetPosition,
            ref velocity,
            smoothTime);

        if (TryGetMapBoundary(out boundary))
        {
            Vector2 clampedPosition = boundary.ClampPoint(
                new Vector2(transform.position.x, transform.position.y),
                Vector2.zero);
            transform.position = new Vector3(clampedPosition.x, clampedPosition.y, fixedZ);
        }
    }

    private bool TryFindTarget()
    {
        GameObject player = GameObject.FindGameObjectWithTag(PlayerTag);
        if (player == null)
        {
            return false;
        }

        target = player.transform;
        return true;
    }

    private bool TryGetMapBoundary(out MapBoundaryController boundary)
    {
        if (mapBoundary == null)
        {
            mapBoundary = MapBoundaryController.Instance;

            if (mapBoundary == null)
            {
                mapBoundary = FindAnyObjectByType<MapBoundaryController>();
            }
        }

        boundary = mapBoundary;
        return boundary != null;
    }
}
