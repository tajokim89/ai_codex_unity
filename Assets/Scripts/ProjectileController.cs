using UnityEngine;

public interface IDamageable
{
    void TakeDamage(float damage);
}

[DisallowMultipleComponent]
[RequireComponent(typeof(Collider2D))]
[RequireComponent(typeof(Rigidbody2D))]
public sealed class ProjectileController : MonoBehaviour
{
    private const string EnemyLayerName = "Enemy";

    [SerializeField, Min(0f)] private float damage = 1f;
    [SerializeField, Min(0.01f)] private float speed = 8f;
    [SerializeField, Min(0.01f)] private float lifetime = 2f;
    [SerializeField, Min(0.01f)] private float scale = 1f;
    [SerializeField] private SpriteRenderer spriteRenderer;

    private Rigidbody2D body;
    private Vector2 moveDirection = Vector2.right;
    private float despawnAt;
    private Quaternion visualBaseRotation = Quaternion.identity;
    private bool isLaunched;

    private void Awake()
    {
        body = GetComponent<Rigidbody2D>();

        if (spriteRenderer == null)
        {
            spriteRenderer = GetComponentInChildren<SpriteRenderer>();
        }

        body.gravityScale = 0f;
        body.freezeRotation = true;
        body.interpolation = RigidbodyInterpolation2D.Interpolate;
    }

    private void Update()
    {
        if (!isLaunched)
        {
            return;
        }

        if (Time.time >= despawnAt)
        {
            Destroy(gameObject);
        }
    }

    private void FixedUpdate()
    {
        if (!isLaunched)
        {
            return;
        }

        body.linearVelocity = moveDirection * speed;
    }

    public void Launch(Vector2 direction, Quaternion baseRotation)
    {
        if (direction.sqrMagnitude <= Mathf.Epsilon)
        {
            direction = Vector2.right;
        }

        moveDirection = direction.normalized;
        visualBaseRotation = baseRotation;
        despawnAt = Time.time + Mathf.Max(0.01f, lifetime);
        isLaunched = true;

        transform.localScale = Vector3.one * Mathf.Max(0.01f, scale);
        ApplyVisualDirection();
        body.linearVelocity = moveDirection * speed;
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        HandleImpact(other.gameObject);
    }

    private void OnCollisionEnter2D(Collision2D collision)
    {
        HandleImpact(collision.gameObject);
    }

    private void HandleImpact(GameObject other)
    {
        if (!isLaunched || !IsEnemy(other.layer))
        {
            return;
        }

        if (other.TryGetComponent<IDamageable>(out IDamageable damageable))
        {
            damageable.TakeDamage(damage);
        }
        else
        {
            other.SendMessage("TakeDamage", damage, SendMessageOptions.DontRequireReceiver);
        }

        Destroy(gameObject);
    }

    private void ApplyVisualDirection()
    {
        Vector2 visualDirection = moveDirection;
        bool shouldFlipX = visualDirection.x < 0f;

        if (shouldFlipX)
        {
            visualDirection.x = -visualDirection.x;
        }

        float angle = Mathf.Atan2(visualDirection.y, visualDirection.x) * Mathf.Rad2Deg;
        transform.rotation = visualBaseRotation * Quaternion.Euler(0f, 0f, angle);

        if (spriteRenderer != null)
        {
            spriteRenderer.flipX = shouldFlipX;
        }
    }

    private static bool IsEnemy(int layer)
    {
        int enemyLayer = LayerMask.NameToLayer(EnemyLayerName);
        return enemyLayer >= 0 && layer == enemyLayer;
    }
}
