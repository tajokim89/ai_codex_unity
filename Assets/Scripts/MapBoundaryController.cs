using UnityEngine;

[DisallowMultipleComponent]
[RequireComponent(typeof(BoxCollider2D))]
public sealed class MapBoundaryController : MonoBehaviour
{
    public static MapBoundaryController Instance { get; private set; }

    private BoxCollider2D boundaryCollider;

    public Bounds WorldBounds => boundaryCollider.bounds;

    private void Reset()
    {
        CacheCollider();
        boundaryCollider.isTrigger = true;
    }

    private void Awake()
    {
        CacheCollider();
        Instance = this;
    }

    private void OnEnable()
    {
        CacheCollider();
        Instance = this;
    }

    private void OnDisable()
    {
        if (Instance == this)
        {
            Instance = null;
        }
    }

    public Vector2 ClampPoint(Vector2 point, Vector2 padding)
    {
        Bounds bounds = WorldBounds;
        float minX = bounds.min.x + padding.x;
        float maxX = bounds.max.x - padding.x;
        float minY = bounds.min.y + padding.y;
        float maxY = bounds.max.y - padding.y;

        if (minX > maxX)
        {
            float centerX = bounds.center.x;
            minX = centerX;
            maxX = centerX;
        }

        if (minY > maxY)
        {
            float centerY = bounds.center.y;
            minY = centerY;
            maxY = centerY;
        }

        return new Vector2(
            Mathf.Clamp(point.x, minX, maxX),
            Mathf.Clamp(point.y, minY, maxY));
    }

    private void CacheCollider()
    {
        if (boundaryCollider == null)
        {
            boundaryCollider = GetComponent<BoxCollider2D>();
        }
    }
}
