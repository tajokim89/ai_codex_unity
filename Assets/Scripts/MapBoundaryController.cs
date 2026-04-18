using UnityEngine;

[DisallowMultipleComponent]
[RequireComponent(typeof(BoxCollider2D))]
public sealed class MapBoundaryController : MonoBehaviour
{
    public static MapBoundaryController Instance { get; private set; }

    [SerializeField] private BoxCollider2D boundaryCollider;

    public Bounds WorldBounds => boundaryCollider.bounds;

    private void Reset()
    {
        CacheCollider();
        EnsureTriggerCollider();
    }

    private void OnValidate()
    {
        CacheCollider();
        EnsureTriggerCollider();
    }

    private void Awake()
    {
        CacheCollider();
        EnsureTriggerCollider();
        RegisterInstance();
    }

    private void OnEnable()
    {
        CacheCollider();
        EnsureTriggerCollider();
        RegisterInstance();
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

    private void EnsureTriggerCollider()
    {
        if (boundaryCollider != null)
        {
            boundaryCollider.isTrigger = true;
        }
    }

    private void RegisterInstance()
    {
        if (Instance != null && Instance != this)
        {
            Debug.LogError(
                "Multiple MapBoundaryController instances are active. Keep only one shared boundary controller in the scene.",
                this);
            enabled = false;
            return;
        }

        Instance = this;
    }
}
