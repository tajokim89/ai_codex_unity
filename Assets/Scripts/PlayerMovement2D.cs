using UnityEngine;
using UnityEngine.InputSystem;

[RequireComponent(typeof(Animator))]
[RequireComponent(typeof(BoxCollider2D))]
[RequireComponent(typeof(Rigidbody2D))]
public sealed class PlayerMovement2D : MonoBehaviour
{
    private const float MoveSpeed = 3.5f;
    private const float AxisTieTolerance = 0.001f;

    private static readonly int MoveXHash = Animator.StringToHash("moveX");
    private static readonly int MoveYHash = Animator.StringToHash("moveY");
    private static readonly int IsMovingHash = Animator.StringToHash("isMoving");
    private static readonly int IdleFrontHash = Animator.StringToHash("Base Layer.Idle_Front");
    private static readonly int IdleBackHash = Animator.StringToHash("Base Layer.Idle_Back");
    private static readonly int IdleLeftHash = Animator.StringToHash("Base Layer.Idle_Left");
    private static readonly int IdleRightHash = Animator.StringToHash("Base Layer.Idle_Right");
    private static readonly int WalkFrontHash = Animator.StringToHash("Base Layer.Walk_Front");
    private static readonly int WalkBackHash = Animator.StringToHash("Base Layer.Walk_Back");
    private static readonly int WalkLeftHash = Animator.StringToHash("Base Layer.Walk_Left");
    private static readonly int WalkRightHash = Animator.StringToHash("Base Layer.Walk_Right");

    private Animator animator;
    private Rigidbody2D body;
    private Vector2 moveInput;
    private Vector2 facing = Vector2.down;
    private int currentStateHash;
    private bool hasMoveXParameter;
    private bool hasMoveYParameter;
    private bool hasIsMovingParameter;

    private void Awake()
    {
        animator = GetComponent<Animator>();
        body = GetComponent<Rigidbody2D>();

        body.gravityScale = 0f;
        body.interpolation = RigidbodyInterpolation2D.Interpolate;
        body.freezeRotation = true;

        CacheAnimatorParameters();
        PlayAnimation(IdleFrontHash);
    }

    private void Update()
    {
        moveInput = ReadMoveInput();

        if (moveInput != Vector2.zero)
        {
            facing = ResolveFacing(moveInput, facing);
        }

        UpdateAnimator(moveInput != Vector2.zero, facing);
    }

    private void FixedUpdate()
    {
        Vector2 normalizedMove = moveInput.sqrMagnitude > 1f ? moveInput.normalized : moveInput;
        Vector2 targetPosition = body.position + (normalizedMove * MoveSpeed * Time.fixedDeltaTime);
        body.MovePosition(targetPosition);
    }

    private void CacheAnimatorParameters()
    {
        foreach (AnimatorControllerParameter parameter in animator.parameters)
        {
            if (parameter.nameHash == MoveXHash)
            {
                hasMoveXParameter = true;
            }
            else if (parameter.nameHash == MoveYHash)
            {
                hasMoveYParameter = true;
            }
            else if (parameter.nameHash == IsMovingHash)
            {
                hasIsMovingParameter = true;
            }
        }
    }

    private static Vector2 ReadMoveInput()
    {
        Keyboard keyboard = Keyboard.current;
        if (keyboard == null)
        {
            return Vector2.zero;
        }

        float horizontal = 0f;
        float vertical = 0f;

        if (keyboard.aKey.isPressed || keyboard.leftArrowKey.isPressed)
        {
            horizontal -= 1f;
        }

        if (keyboard.dKey.isPressed || keyboard.rightArrowKey.isPressed)
        {
            horizontal += 1f;
        }

        if (keyboard.sKey.isPressed || keyboard.downArrowKey.isPressed)
        {
            vertical -= 1f;
        }

        if (keyboard.wKey.isPressed || keyboard.upArrowKey.isPressed)
        {
            vertical += 1f;
        }

        return new Vector2(horizontal, vertical);
    }

    // Preserve the previous dominant axis on perfect diagonals to prevent animation jitter.
    private static Vector2 ResolveFacing(Vector2 input, Vector2 fallbackFacing)
    {
        float absoluteX = Mathf.Abs(input.x);
        float absoluteY = Mathf.Abs(input.y);

        if (Mathf.Abs(absoluteX - absoluteY) <= AxisTieTolerance)
        {
            if (Mathf.Abs(fallbackFacing.x) > Mathf.Abs(fallbackFacing.y) && absoluteX > 0f)
            {
                return new Vector2(Mathf.Sign(input.x), 0f);
            }

            if (absoluteY > 0f)
            {
                return new Vector2(0f, Mathf.Sign(input.y));
            }

            if (absoluteX > 0f)
            {
                return new Vector2(Mathf.Sign(input.x), 0f);
            }
        }

        return absoluteX > absoluteY
            ? new Vector2(Mathf.Sign(input.x), 0f)
            : new Vector2(0f, Mathf.Sign(input.y));
    }

    private void UpdateAnimator(bool isMoving, Vector2 lookDirection)
    {
        if (hasMoveXParameter)
        {
            animator.SetFloat(MoveXHash, lookDirection.x);
        }

        if (hasMoveYParameter)
        {
            animator.SetFloat(MoveYHash, lookDirection.y);
        }

        if (hasIsMovingParameter)
        {
            animator.SetBool(IsMovingHash, isMoving);
        }

        PlayAnimation(GetAnimationStateHash(isMoving, lookDirection));
    }

    private void PlayAnimation(int stateHash)
    {
        if (currentStateHash == stateHash)
        {
            return;
        }

        currentStateHash = stateHash;
        animator.Play(stateHash, 0, 0f);
    }

    private static int GetAnimationStateHash(bool isMoving, Vector2 lookDirection)
    {
        if (lookDirection.x < 0f)
        {
            return isMoving ? WalkLeftHash : IdleLeftHash;
        }

        if (lookDirection.x > 0f)
        {
            return isMoving ? WalkRightHash : IdleRightHash;
        }

        if (lookDirection.y > 0f)
        {
            return isMoving ? WalkBackHash : IdleBackHash;
        }

        return isMoving ? WalkFrontHash : IdleFrontHash;
    }
}
