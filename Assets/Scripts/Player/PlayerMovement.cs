using Unity.Netcode;
using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerMovement : NetworkBehaviour
{
    [Header("Movement")]
    public float sprintMultiplier = 2f;
    public float jumpHeight = 1.5f;

    [Header("Gravity")]
    public float gravity = -9.81f;
    public float fallMultiplier = 2.5f;

    [Header("Ground")]
    public Transform groundCheck;
    public float groundDistance = 0.25f;
    public LayerMask groundMask;
    public float groundedGraceTime = 0.1f;

    [Header("Crouch")]
    public float crouchHeight = 1.3f;
    public float crouchSpeed = 1f;

    private PlayerContext ctx;
    private CharacterController controller;

    private Vector3 velocity;
    private Vector2 moveInput;
    //private NetworkVariable<float> netAnimationSpeed = new NetworkVariable<float>(
        //0,
        //NetworkVariableReadPermission.Everyone,
        //NetworkVariableWritePermission.Owner
    //);

    bool jumpPressed;
    bool sprintHeld;
    bool isCrouching;
    public bool isGrounded;
    float groundedTimer;

    public bool IsMovementLocked { get; set; }
    public Vector3 MoveDirection { get; private set; }
    public float AnimationSpeed { get; private set; }

    public bool IsGrounded => groundedTimer > 0f;
    public float VerticalVelocity => velocity.y;
    public bool IsCrouching => isCrouching;

    float standHeight;

    //==================================================================================//
    void Awake()
    {
        controller = GetComponent<CharacterController>();
        standHeight = controller.height;
        ctx = GetComponent<PlayerContext>();

    }

    //==================================================================================//
    void Update()
    {
        if (!IsOwner)
            return;


        CheckGround();
        Jump();
        Gravity();
        Move();
        Crouch();
    }

    //==================================================================================//
    void Move()
    {

        //Vector3 camForward = Camera.main.transform.forward;
        //Vector3 camRight = Camera.main.transform.right;
        Transform cam = ctx.mainCamera.transform;

        Vector3 camForward = cam.forward;
        Vector3 camRight = cam.right;

        camForward.y = 0;
        camRight.y = 0;

        camForward.Normalize();
        camRight.Normalize();

        MoveDirection = camForward * moveInput.y + camRight * moveInput.x;

        if (IsMovementLocked)
            return;

        float moveSpeed = ctx.stats.GetStat(StatType.MoveSpeed);

        float speed = isCrouching ? crouchSpeed : sprintHeld ? moveSpeed * sprintMultiplier : moveSpeed;

        AnimationSpeed = moveInput.magnitude * (sprintHeld ? 1f : 0.5f);

        controller.Move(MoveDirection * speed * Time.deltaTime);
    }

    //==================================================================================//
    void CheckGround()
    {
        Vector3 checkPosition = transform.position + controller.center - Vector3.up * (controller.height * 0.5f);

        isGrounded = Physics.CheckSphere(checkPosition,groundDistance,groundMask);

        groundedTimer = isGrounded ? groundedGraceTime : groundedTimer - Time.deltaTime;

        if (groundedTimer > 0f && velocity.y < 0) velocity.y = -2f;
    }

    //==================================================================================//
    void Jump()
    {
        if (!jumpPressed || !isGrounded)
            return;

        jumpPressed = false;

        velocity.y = Mathf.Sqrt(jumpHeight * -2f * gravity);
    }

    //==================================================================================//
    void Gravity()
    {
        if (velocity.y < 0)
            velocity.y += gravity * fallMultiplier * Time.deltaTime;
        else
            velocity.y += gravity * Time.deltaTime;

        controller.Move(velocity * Time.deltaTime);
    }

    //==================================================================================//
    void Crouch()
    {
        float targetHeight = isCrouching ? crouchHeight : standHeight;
    }

    //==================================================================================//
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

    //==================================================================================//

    void OnDrawGizmosSelected()
    {
        CharacterController cc = GetComponent<CharacterController>();

        Vector3 checkPosition = transform.position + cc.center - Vector3.up * (cc.height * 0.5f);

        Gizmos.color = isGrounded ? Color.green : Color.red;

        Gizmos.DrawWireSphere(checkPosition,groundDistance);
    }
}
