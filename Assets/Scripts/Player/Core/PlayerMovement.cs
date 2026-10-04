using Unity.Netcode;
using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerMovement : NetworkBehaviour
{

    [Header("Movement")]
    [SerializeField] private float sprintMultiplier = 2f;
    [SerializeField] private float acceleration = 12f;
    [SerializeField] private float deceleration = 16f;

    [Header("Jump")]
    [SerializeField] private float jumpHeight = 1.5f;

    [Header("Gravity")]
    [SerializeField] private float gravity = -9.81f;
    [SerializeField] private float fallMultiplier = 2.5f;

    [Header("Ground")]
    [SerializeField] private float groundDistance = 0.25f;
    [SerializeField] private LayerMask groundMask;
    [SerializeField] private float groundedGraceTime = 0.1f;


    [Header("Crouch")]
    [SerializeField] private float crouchHeight = 1.3f;
    [SerializeField] private float crouchSpeed = 1f;

    [Header("Aim Movement")]
    [SerializeField] private float aimMovementMultiplier = 0.6f;

    [Header("Dash")]
    [SerializeField] private float dashDistance = 4.5f;
    [SerializeField] private float dashDuration = 0.25f;
    [SerializeField] private float dashCooldown = 1f;

    private bool dashPressed;
    private bool isDashing;
    private float dashTimer;
    private float dashCooldownTimer;
    private Vector3 dashDirection;

    public bool IsDashing => isDashing;
    public float DashCooldownRemaining => dashCooldownTimer;

    private bool isAiming;

    public bool IsAiming => isAiming;

    private bool isAttacking;


    private PlayerContext ctx;

    private Transform cameraTransform;

    private Vector2 moveInput;
    private Vector3 velocity;
    private Vector3 moveDirection;

    private float currentSpeed;
    private float groundedTimer;
    private float standHeight;

    private bool jumpPressed;
    private bool sprintHeld;
    private bool isCrouching;
    private bool wasGrounded;


    public bool IsMovementLocked { get; private set; }

    public Vector3 MoveDirection => moveDirection;

    public float AnimationSpeed { get; private set; }

    public bool IsGrounded => groundedTimer > 0f;

    public float VerticalVelocity => velocity.y;

    public bool IsCrouching => isCrouching;


    //========================================================//
    // INITIALIZATION
    //========================================================//

    private void Awake()
    {
        ctx = GetComponent<PlayerContext>();

        standHeight = ctx.controller.height;
    }


    //========================================================//
    // UPDATE
    //========================================================//

    private void Update()
    {
        if (!IsOwner)
            return;

        UpdateGrounded();

        UpdateDash();

        if (isDashing)
            return;

        UpdateJump();
        UpdateGravity();
        UpdateMovement();
    }


    //========================================================//
    // MOVEMENT
    //========================================================//

    private void UpdateMovement()
    {
        if (cameraTransform == null)
            return;

        CalculateMoveDirection();
        UpdateSpeed();

        Vector3 horizontalVelocity =
            moveDirection * currentSpeed;

        Vector3 finalVelocity =
            horizontalVelocity + velocity;

        ctx.controller.Move(
            finalVelocity * Time.deltaTime
        );
    }


    private void CalculateMoveDirection()
    {
        Vector3 forward = cameraTransform.forward;
        Vector3 right = cameraTransform.right;

        forward.y = 0f;
        right.y = 0f;

        forward.Normalize();
        right.Normalize();

        moveDirection =
            forward * moveInput.y +
            right * moveInput.x;

        if (moveDirection.sqrMagnitude > 1f)
            moveDirection.Normalize();
    }


    private void UpdateSpeed()
    {
        float moveSpeed =
            ctx.stats.GetStat(StatType.MoveSpeed);

        bool isMoving =
            moveInput.sqrMagnitude > 0.01f;

        float targetSpeed = 0f;

        if (!IsMovementLocked && isMoving)
        {
            if (isCrouching)
            {
                targetSpeed = crouchSpeed;
            }
            else if (sprintHeld && !isAiming)
            {
                // No puede correr mientras apunta.
                targetSpeed = moveSpeed * sprintMultiplier;
            }
            else
            {
                targetSpeed = moveSpeed;
            }

            // Reducir velocidad mientras apunta.
            if (isAiming)
            {
                targetSpeed *= aimMovementMultiplier;
            }
        }

        float accelerationRate =
            isMoving
                ? acceleration
                : deceleration;

        currentSpeed = Mathf.MoveTowards(
            currentSpeed,
            targetSpeed,
            accelerationRate * Time.deltaTime
        );

        AnimationSpeed =
            moveSpeed > 0f
                ? currentSpeed / (moveSpeed * sprintMultiplier)
                : 0f;
    }



    // GROUND
    private void UpdateGrounded()
    {
        Vector3 checkPosition =
            transform.position +
            ctx.controller.center -
            Vector3.up * (ctx.controller.height * 0.5f);

        bool grounded =
            Physics.CheckSphere(
                checkPosition,
                groundDistance,
                groundMask
            );

        groundedTimer =
            grounded
                ? groundedGraceTime
                : groundedTimer - Time.deltaTime;


        // LANDING
        if (grounded && !wasGrounded && velocity.y < -1f)
        {
            currentSpeed *= 0.85f;
        }


        // VERTICAL
        if (IsGrounded && velocity.y < 0f)
            velocity.y = -2f;


        wasGrounded = grounded;
    }


    //========================================================//
    // JUMP
    //========================================================//

    private void UpdateJump()
    {
        if (!jumpPressed || !IsGrounded)
            return;

        jumpPressed = false;

        velocity.y =
            Mathf.Sqrt(
                jumpHeight * -2f * gravity
            );
    }


    //========================================================//
    // GRAVITY
    //========================================================//

    private void UpdateGravity()
    {
        float gravityMultiplier =
            velocity.y < 0f
                ? fallMultiplier
                : 1f;

        velocity.y +=
            gravity *
            gravityMultiplier *
            Time.deltaTime;
    }


    //========================================================//
    // PUBLIC API
    //========================================================//

    public void SetCameraTransform(Transform camera)
    {
        cameraTransform = camera;
    }


    public void SetMoveInput(Vector2 input)
    {
        moveInput = input;
    }


    public void SetJump(bool value)
    {
        if (value)
            jumpPressed = true;
    }


    public void SetSprint(bool value)
    {
        sprintHeld = value;
    }


    public void SetCrouch(bool value)
    {
        isCrouching = value;
    }


    public void SetMovementLocked(bool locked)
    {
        IsMovementLocked = locked;

        if (locked)
        {
            moveInput = Vector2.zero;
            currentSpeed = 0f;
        }
    }

    public void SetAiming(bool aiming)
    {
        isAiming = aiming;
    }

    public void SetDash(bool value)
    {
        if (value)
            dashPressed = true;
    }


    //========================================================//
    // DEBUG
    //========================================================//

    private void OnDrawGizmosSelected()
    {
        CharacterController cc =
            GetComponent<CharacterController>();

        if (cc == null)
            return;

        Vector3 checkPosition =
            transform.position +
            cc.center -
            Vector3.up * (cc.height * 0.5f);

        Gizmos.color =
            IsGrounded
                ? Color.green
                : Color.red;

        Gizmos.DrawWireSphere(
            checkPosition,
            groundDistance
        );
    }

    //========================================================//
    // RESPAWN
    //========================================================//

    public void ResetForRespawn()
    {
        moveInput = Vector2.zero;
        moveDirection = Vector3.zero;

        velocity = Vector3.zero;

        currentSpeed = 0f;
        AnimationSpeed = 0f;

        groundedTimer = 0f;

        jumpPressed = false;
        sprintHeld = false;
        isCrouching = false;

        wasGrounded = false;

        IsMovementLocked = false;
    }
    //========================================================//
    // DASH
    //========================================================//

    private void UpdateDash()
    {
        if (dashCooldownTimer > 0f)
        {
            dashCooldownTimer -= Time.deltaTime;
        }

        // Si ya está haciendo dash
        if (isDashing)
        {
            dashTimer += Time.deltaTime;

            float dashSpeed = dashDistance / dashDuration;

            // Suaviza ligeramente la velocidad del dash.
            float dashProgress = dashTimer / dashDuration;

            float speedMultiplier =
                1f - Mathf.Clamp01(dashProgress) * 0.25f;

            ctx.controller.Move(
                dashDirection *
                dashSpeed *
                speedMultiplier *
                Time.deltaTime
            );

            if (dashTimer >= dashDuration)
            {
                EndDash();
            }

            return;
        }

        if (!dashPressed)
            return;

        dashPressed = false;

        if (!CanDash())
            return;

        StartDash();
    }


    private bool CanDash()
    {
        if (isDashing)
            return false;

        if (dashCooldownTimer > 0f)
            return false;

        if (IsMovementLocked)
            return false;

        if (!IsGrounded)
            return false;

        if (isCrouching)
            return false;

        return true;
    }


    private void StartDash()
    {
        isDashing = true;

        dashTimer = 0f;
        dashCooldownTimer = dashCooldown;

        sprintHeld = false;
        currentSpeed = 0f;

        dashDirection = CalculateDashDirection();

        // Aseguramos que sea horizontal.
        dashDirection.y = 0f;

        if (dashDirection.sqrMagnitude <= 0.001f)
        {
            dashDirection = transform.forward;
        }

        dashDirection.Normalize();

        OnDashStarted();
    }


    private void EndDash()
    {
        isDashing = false;

        dashTimer = 0f;
        currentSpeed = 0f;

        OnDashEnded();
    }


    private Vector3 CalculateDashDirection()
    {
        Vector3 direction = moveDirection;

        // Si el jugador no está dando ninguna dirección,
        // dash hacia donde mira la cámara.
        if (direction.sqrMagnitude <= 0.01f)
        {
            if (cameraTransform != null)
            {
                direction = cameraTransform.forward;
                direction.y = 0f;
                direction.Normalize();
            }
            else
            {
                direction = transform.forward;
            }
        }

        return direction.normalized;
    }
    private void OnDashStarted()
    {
       
        // - Animación de dash
        // - Trail
        // - VFX
        // - Sonido
    }


    private void OnDashEnded()
    {
        
        // - Final de animación
        // - VFX de aterrizaje
    }
}
