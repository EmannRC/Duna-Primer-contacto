
using Unity.Netcode;
using UnityEngine;
using UnityEngine.AI;


[RequireComponent(typeof(EnemySteering))]
public class GroundEnemyMovement : EnemyMovementBase
{
    [Header("Movement")]
    [SerializeField] private float moveSpeed = 3.5f;

    [Header("Gravity")]
    [SerializeField] private float gravity = -20f;

    private CharacterController controller;
    private EnemySteering steering;

    private float verticalVelocity;

    public Vector3 Velocity =>
        controller != null
            ? controller.velocity
            : Vector3.zero;

    //=======================================================//
    // AWAKE
    //=======================================================//

    protected override void Awake()
    {
        base.Awake();

        controller = GetComponent<CharacterController>();
        steering = GetComponent<EnemySteering>();
    }

    //=======================================================//
    // UPDATE
    //=======================================================//

    protected override void Update()
    {
        base.Update();

        if (!IsServer ||
            controller == null ||
            !controller.enabled)
            return;

        if (IsBeingRepelled ||
            movementLocked ||
            IsMovementPaused)
            return;

        // Gravity
        if (controller.isGrounded && verticalVelocity < 0f)
            verticalVelocity = -2f;

        verticalVelocity += gravity * Time.deltaTime;

        Vector3 horizontalMovement = Vector3.zero;

        if (hasDestination && !HasReachedDestination)
        {
            Vector3 direction = destination - transform.position;
            direction.y = 0f;

            direction.Normalize();

            Vector3 finalDirection =
                steering.CalculateDirection(direction);

            horizontalMovement = finalDirection * moveSpeed;

            RotateTowards(finalDirection);
        }

        Vector3 movement = horizontalMovement;
        movement.y = verticalVelocity;

        controller.Move(movement * Time.deltaTime);
    }

    //=======================================================//
    // MOVEMENT
    //=======================================================//

    protected override void Move(Transform target)
    {
        if (target == null)
            return;

        destination = target.position;
        hasDestination = true;
    }

    protected override void MoveTowardsDestination()
    {
        // El movimiento se procesa en Update mediante CharacterController.
    }

    //=======================================================//
    // MOVEMENT LOCK
    //=======================================================//

    public override void SetMovementLocked(bool locked)
    {
        movementLocked = locked;
    }

    //=======================================================//
    // STOP
    //=======================================================//

    protected override void StopMovement()
    {
        base.StopMovement();
    }

    //=======================================================//
    // REPEL
    //=======================================================//

    protected override void MoveDuringRepel(Vector3 displacement)
    {
        if (controller != null && controller.enabled)
            controller.Move(displacement);
    }

    //=======================================================//
    // GIZMOS
    //=======================================================//

    private void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.green;

        Gizmos.DrawWireSphere(
            transform.position,
            stoppingDistance
        );
    }
}