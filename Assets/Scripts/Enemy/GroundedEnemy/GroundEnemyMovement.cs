
using Unity.Netcode;
using UnityEngine;
using UnityEngine.AI;


[RequireComponent(typeof(NavMeshAgent))]
public class GroundEnemyMovement : EnemyMovementBase
{
    [Header("Movement")]
    [SerializeField] private float moveSpeed = 3.5f;
    [SerializeField] private float stoppingDistance = 2f;

    private NavMeshAgent agent;
    private EnforcerSpecialAttack specialAttack;

    public Vector3 Velocity =>
        agent != null
            ? agent.velocity
            : Vector3.zero;


    //=======================================================//
    // AWAKE
    //=======================================================//

    protected override void Awake()
    {
        base.Awake();

        agent =
            GetComponent<NavMeshAgent>();

        specialAttack =
            GetComponent<EnforcerSpecialAttack>();
    }


    //=======================================================//
    // MOVEMENT PAUSED
    //=======================================================//

    protected override bool IsMovementPaused =>
        specialAttack != null &&
        specialAttack.IsPerformingSpecialAttack;


    //=======================================================//
    // MOVEMENT LOCK
    //=======================================================//

    public override void SetMovementLocked(
        bool locked)
    {
        movementLocked = locked;

        if (!movementLocked)
            return;

        StopMovement();
    }


    //=======================================================//
    // MOVE
    //=======================================================//

    protected override void Move(
        Transform target)
    {
        if (agent == null ||
            !agent.isActiveAndEnabled ||
            !agent.isOnNavMesh)
        {
            return;
        }

        agent.speed =
            moveSpeed;

        agent.stoppingDistance =
            stoppingDistance;

        agent.angularSpeed =
            720f;

        agent.updateRotation =
            false;

        agent.SetDestination(
            target.position
        );
    }


    //=======================================================//
    // STOP MOVEMENT
    //=======================================================//

    protected override void StopMovement()
    {
        if (agent == null)
            return;

        if (agent.isActiveAndEnabled &&
            agent.isOnNavMesh)
        {
            agent.ResetPath();
        }
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