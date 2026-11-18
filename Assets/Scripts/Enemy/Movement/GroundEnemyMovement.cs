
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

    [Header("Chase Speed")]
    [SerializeField] private bool accelerateWhenChasing = true;
    [SerializeField] private float chaseSpeedMultiplier = 1.5f;

    private float currentMoveSpeed;

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

        currentMoveSpeed = moveSpeed;
    }

    //=======================================================//
    // MOVEMENT
    //=======================================================//

    protected override void UpdateMovement()
    {
        if (controller == null || !controller.enabled)
            return;

        // Gravedad siempre activa.
        if (controller.isGrounded && verticalVelocity < 0f)
            verticalVelocity = -2f;

        verticalVelocity += gravity * Time.deltaTime;

        Vector3 horizontalMovement = Vector3.zero;

        // Movimiento horizontal.
        if (!movementLocked && !IsMovementPaused)
        {
            horizontalMovement = CalculateHorizontalMovement();
        }

        Vector3 movement = horizontalMovement;
        movement.y = verticalVelocity;

        controller.Move(movement * Time.deltaTime);
    }

    private Vector3 CalculateHorizontalMovement()
    {
        if (!hasDestination || HasReachedDestination)
            return Vector3.zero;

        Vector3 direction =
            destination - transform.position;

        direction.y = 0f;

        if (direction.sqrMagnitude < 0.001f)
            return Vector3.zero;

        direction.Normalize();

        Vector3 finalDirection =
            steering.CalculateDirection(direction);

        RotateTowards(finalDirection);

        //return finalDirection * moveSpeed;
        return finalDirection * currentMoveSpeed;
    }

    protected override void MoveTowardsDestination()
    {
        // El movimiento se procesa en UpdateMovement.
    }

    //=======================================================//
    // REPEL
    //=======================================================//

    protected override void MoveDuringRepel(Vector3 displacement)
    {
        if (controller != null && controller.enabled)
            controller.Move(displacement);
    }


    public void SetChasing(bool isChasing)
    {
        if (!accelerateWhenChasing)
        {
            currentMoveSpeed = moveSpeed;
            return;
        }

        currentMoveSpeed = isChasing
            ? moveSpeed * chaseSpeedMultiplier
            : moveSpeed;
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