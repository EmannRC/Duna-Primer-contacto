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
                targetSpeed = crouchSpeed;
            else if (sprintHeld)
                targetSpeed = moveSpeed * sprintMultiplier;
            else
                targetSpeed = moveSpeed;
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
}
