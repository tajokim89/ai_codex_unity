using UnityEngine;

[DisallowMultipleComponent]
public sealed class AutoShooter : MonoBehaviour
{
    private const float DirectionChangeThreshold = 0.999f;

    [SerializeField, Min(0.01f)] private float fireInterval = 0.3f;
    [SerializeField, Min(0f)] private float minTurnCooldown = 0.1f;
    [SerializeField] private ProjectileController projectilePrefab;
    [SerializeField] private Transform spawnPoint;
    [SerializeField] private Transform baseRotationReference;
    [SerializeField] private PlayerMovement2D playerMovement;

    private float nextFireTime;
    private float turnLockedUntil;
    private Vector2 lastAimDirection = Vector2.zero;

    private void Awake()
    {
        if (playerMovement == null)
        {
            playerMovement = GetComponent<PlayerMovement2D>();
        }

        if (spawnPoint == null)
        {
            spawnPoint = transform;
        }

        if (baseRotationReference == null)
        {
            baseRotationReference = transform;
        }
    }

    private void Update()
    {
        if (projectilePrefab == null)
        {
            return;
        }

        Vector2 aimDirection = ResolveAimDirection();
        if (aimDirection == Vector2.zero)
        {
            return;
        }

        if (HasDirectionChanged(aimDirection))
        {
            turnLockedUntil = Time.time + Mathf.Max(0f, minTurnCooldown);
            lastAimDirection = aimDirection;
        }

        if (Time.time < nextFireTime || Time.time < turnLockedUntil)
        {
            return;
        }

        Fire(aimDirection);
        lastAimDirection = aimDirection;
        nextFireTime = Time.time + Mathf.Max(0.01f, fireInterval);
    }

    private Vector2 ResolveAimDirection()
    {
        if (playerMovement == null)
        {
            return lastAimDirection;
        }

        Vector2 moveInput = playerMovement.CurrentMoveInput;
        if (moveInput != Vector2.zero)
        {
            return moveInput.normalized;
        }

        return playerMovement.LastMoveDirection;
    }

    private bool HasDirectionChanged(Vector2 nextDirection)
    {
        return lastAimDirection != Vector2.zero
            && Vector2.Dot(lastAimDirection.normalized, nextDirection.normalized) < DirectionChangeThreshold;
    }

    private void Fire(Vector2 direction)
    {
        Transform origin = spawnPoint != null ? spawnPoint : transform;
        Quaternion baseRotation = baseRotationReference != null ? baseRotationReference.rotation : Quaternion.identity;

        ProjectileController projectile = Instantiate(projectilePrefab, origin.position, Quaternion.identity);
        projectile.Launch(direction, baseRotation);
    }
}
