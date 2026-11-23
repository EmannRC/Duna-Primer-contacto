using Duna.Player;
using Unity.Netcode;
using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerMovement : NetworkBehaviour
{
    [Header("Movement & Rotation")]
    [SerializeField] private float sprintMultiplier = 2f;
    [SerializeField] private float acceleration = 12f;
    [SerializeField] private float deceleration = 16f;
    [SerializeField] private float aimMovementMultiplier = 0.6f;
    [SerializeField] private float rotationSpeed = 10f;

    [Header("Jump & Gravity")]
    [SerializeField] private float jumpHeight = 1.5f;
    [SerializeField] private float jumpBufferTime = 0.12f;
    [SerializeField] private float gravity = -9.81f;
    [SerializeField] private float fallMultiplier = 2.5f;

    [Header("Ground Check")]
    [SerializeField] private float groundDistance = 0.25f;
    [SerializeField] private LayerMask groundMask;
    [SerializeField] private float groundedGraceTime = 0.1f;

    [Header("Dash")]
    [SerializeField, Min(0.1f)] private float dashDistance = 4.5f;
    [SerializeField, Min(0.01f)] private float dashDuration = 0.25f;
    [SerializeField] private float dashFallMultiplier = 5f;
    [SerializeField] private float dashFallDuration = 0.35f;

    private PlayerContext ctx;
    private Transform cameraTransform;

    private Vector2 moveInput;
    private Vector3 velocity;
    private Vector3 moveDirection;
    private Vector3 lastValidMoveDirection;

    private float currentSpeed;
    private float groundedTimer;
    private float jumpBufferTimer;
    private bool sprintHeld;
    private bool wasGrounded;

    // Dash state
    private bool isDashing;
    private float dashTimer;
    private Vector3 dashDirection;
    private bool isDashFalling;
    private float dashFallTimer;

    // Rotation state
    private Vector3 attackDirection;
    private bool attackRotation;

    public bool IsDashing => isDashing;
    public bool IsAiming { get; private set; }
    public bool IsMovementLocked { get; private set; }
    public Vector3 MoveDirection => moveDirection;
    public float AnimationSpeed { get; private set; }
    public float AnimationPlaybackSpeed { get; private set; }
    public bool IsGrounded => groundedTimer > 0f;
    public float VerticalVelocity => velocity.y;

    private bool CanControlMovement =>
        !IsMovementLocked &&
        ctx != null &&
        ctx.state != null &&
        ctx.state.CanMove;

    private bool CanRotate =>
        ctx != null &&
        ctx.state != null &&
        ctx.state.CurrentState == PlayerActionState.Normal;

    private void Awake()
    {
        ctx = GetComponent<PlayerContext>();

        if (ctx == null || ctx.controller == null || ctx.state == null)
        {
            Debug.LogError("PlayerMovement: faltan referencias críticas en PlayerContext.", this);
            enabled = false;
        }
    }

    private void Update()
    {
        if (!IsOwner)
            return;

        UpdateGrounded();

        if (!CanControlMovement)
        {
            StopMovementInput();
            if (isDashing) EndDash();

            UpdateGravity();
            UpdateMovement();
            UpdateRotation();
            return;
        }

        UpdateDash();
        if (isDashing)
            return;

        UpdateJump();
        UpdateGravity();
        UpdateMovement();
        UpdateRotation();
    }

    private void StopMovementInput()
    {
        moveInput = Vector2.zero;
        sprintHeld = false;
        jumpBufferTimer = 0f;
        currentSpeed = 0f;
        moveDirection = Vector3.zero;
    }

    //========================================================//
    // MOVEMENT & SPEED
    //========================================================//

    private void UpdateMovement()
    {
        if (cameraTransform != null && CanControlMovement)
            CalculateMoveDirection();
        else
            moveDirection = Vector3.zero;

        UpdateSpeed();

        Vector3 dir = moveDirection.sqrMagnitude > 0.001f ? moveDirection : lastValidMoveDirection;
        Vector3 finalVelocity = (dir * currentSpeed) + velocity;

        ctx.controller.Move(finalVelocity * Time.deltaTime);
    }

    private void CalculateMoveDirection()
    {
        Vector3 forward = cameraTransform.forward;
        Vector3 right = cameraTransform.right;
        forward.y = 0f;
        right.y = 0f;
        forward.Normalize();
        right.Normalize();

        moveDirection = forward * moveInput.y + right * moveInput.x;

        if (moveDirection.sqrMagnitude > 1f)
            moveDirection.Normalize();

        if (moveDirection.sqrMagnitude > 0.001f)
            lastValidMoveDirection = moveDirection;
    }

    private void UpdateSpeed()
    {
        float moveSpeed = ctx.stats != null ? ctx.stats.GetStat(StatType.MoveSpeed) : 5f;
        bool isMoving = moveInput.sqrMagnitude > 0.01f;
        float targetSpeed = 0f;

        if (CanControlMovement && isMoving)
        {
            targetSpeed = (sprintHeld && !IsAiming)
                ? moveSpeed * sprintMultiplier
                : moveSpeed;

            if (IsAiming)
                targetSpeed *= aimMovementMultiplier;
        }

        float rate = isMoving ? acceleration : deceleration;
        currentSpeed = Mathf.MoveTowards(currentSpeed, targetSpeed, rate * Time.deltaTime);

        if (currentSpeed <= 0.01f && !isMoving)
            lastValidMoveDirection = Vector3.zero;

        AnimationSpeed = moveSpeed > 0f ? currentSpeed / (moveSpeed * sprintMultiplier) : 0f;
        AnimationPlaybackSpeed = moveSpeed > 0f ? currentSpeed / moveSpeed : 0f;
    }

    //========================================================//
    // ROTATION
    //========================================================//

    private void UpdateRotation()
    {
        if (!CanRotate)
            return;

        if (attackRotation)
        {
            FaceDirection(attackDirection);
            return;
        }

        bool aiming = IsAiming || (ctx.targeting != null && ctx.targeting.isAiming);
        if (aiming)
        {
            Transform cam = cameraTransform != null ? cameraTransform : (ctx.mainCamera != null ? ctx.mainCamera.transform : null);
            if (cam != null)
                FaceDirection(cam.forward);
            return;
        }

        if (moveDirection.sqrMagnitude > 0.01f)
        {
            FaceDirection(moveDirection);
        }
    }

    private void FaceDirection(Vector3 dir)
    {
        dir.y = 0f;
        if (dir.sqrMagnitude < 0.01f)
            return;

        Quaternion targetRotation = Quaternion.LookRotation(dir);
        transform.rotation = Quaternion.Slerp(transform.rotation, targetRotation, Time.deltaTime * rotationSpeed);
    }

    public void LookAt(Vector3 direction)
    {
        if (!CanRotate)
            return;

        attackDirection = direction;
        attackRotation = true;
    }

    public void StopAttackRotation()
    {
        attackRotation = false;
    }

    //========================================================//
    // GROUND, JUMP & GRAVITY
    //========================================================//

    private void UpdateGrounded()
    {
        Vector3 checkPosition = transform.position + ctx.controller.center - Vector3.up * (ctx.controller.height * 0.5f);
        bool grounded = Physics.CheckSphere(checkPosition, groundDistance, groundMask);

        groundedTimer = grounded ? groundedGraceTime : groundedTimer - Time.deltaTime;

        if (grounded && !wasGrounded && velocity.y < -1f)
            currentSpeed *= 0.85f;

        if (IsGrounded && velocity.y < 0f)
            velocity.y = -2f;

        wasGrounded = grounded;
    }

    private void UpdateJump()
    {
        if (jumpBufferTimer > 0f)
            jumpBufferTimer -= Time.deltaTime;

        if (jumpBufferTimer <= 0f || !IsGrounded)
            return;

        jumpBufferTimer = 0f;
        groundedTimer = 0f;
        velocity.y = Mathf.Sqrt(jumpHeight * -2f * gravity);
    }

    private void UpdateGravity()
    {
        float multiplier = isDashFalling ? dashFallMultiplier : (velocity.y < 0f ? fallMultiplier : 1f);
        velocity.y += gravity * multiplier * Time.deltaTime;

        if (isDashFalling)
        {
            dashFallTimer -= Time.deltaTime;
            if (dashFallTimer <= 0f || IsGrounded)
            {
                isDashFalling = false;
                dashFallTimer = 0f;
            }
        }
    }

    //========================================================//
    // DASH & SKILLS
    //========================================================//

    private void UpdateDash()
    {
        if (!isDashing)
            return;

        float previousTimer = dashTimer;
        dashTimer = Mathf.Min(dashTimer + Time.deltaTime, dashDuration);

        float dashFrameTime = dashTimer - previousTimer;
        float dashSpeed = dashDistance / dashDuration;

        ctx.controller.Move(dashDirection * dashSpeed * dashFrameTime);

        if (dashTimer >= dashDuration)
            EndDash();
    }

    public void ActivateDashFromSkill()
    {
        if (!IsOwner || !CanControlMovement || isDashing)
            return;

        isDashing = true;
        dashTimer = 0f;
        velocity.y = 0f;

        Vector3 dir = moveDirection.sqrMagnitude > 0.01f
            ? moveDirection
            : (cameraTransform != null ? cameraTransform.forward : transform.forward);

        dir.y = 0f;
        dashDirection = dir.sqrMagnitude > 0.001f ? dir.normalized : transform.forward;
    }

    private void EndDash()
    {
        isDashing = false;
        dashTimer = 0f;

        if (!IsGrounded)
        {
            isDashFalling = true;
            dashFallTimer = dashFallDuration;
        }
    }

    public bool ApplyImpulseFromSkill(float height)
    {
        if (!IsOwner || !CanControlMovement || !IsGrounded || IsDashing)
            return false;

        jumpBufferTimer = 0f;
        groundedTimer = 0f;
        velocity.y = Mathf.Sqrt(height * -2f * gravity);
        return true;
    }

    //========================================================//
    // PUBLIC API & RESPAWN
    //========================================================//

    public void SetCameraTransform(Transform camera) => cameraTransform = camera;
    public void SetMoveInput(Vector2 input) => moveInput = CanControlMovement ? input : Vector2.zero;
    public void SetSprint(bool value) => sprintHeld = CanControlMovement && value;
    public void SetAiming(bool aiming) => IsAiming = aiming;

    public void SetJump(bool value)
    {
        if (value && CanControlMovement)
            jumpBufferTimer = jumpBufferTime;
    }

    public void SetMovementLocked(bool locked)
    {
        IsMovementLocked = locked;
        if (locked)
        {
            StopMovementInput();
            if (isDashing) EndDash();
        }
    }

    public void ResetForRespawn()
    {
        StopMovementInput();
        lastValidMoveDirection = Vector3.zero;
        velocity = Vector3.zero;
        AnimationSpeed = 0f;
        AnimationPlaybackSpeed = 0f;
        groundedTimer = 0f;
        wasGrounded = false;
        IsMovementLocked = false;
        isDashing = false;
        dashTimer = 0f;
        dashDirection = Vector3.zero;
        isDashFalling = false;
        dashFallTimer = 0f;
        attackRotation = false;
    }

    private void OnDrawGizmosSelected()
    {
        if (!TryGetComponent(out CharacterController cc))
            return;

        Vector3 checkPosition = transform.position + cc.center - Vector3.up * (cc.height * 0.5f);
        Gizmos.color = IsGrounded ? Color.green : Color.red;
        Gizmos.DrawWireSphere(checkPosition, groundDistance);
    }
}
