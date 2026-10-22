
using Unity.Netcode;
using UnityEngine;
using UnityEngine.AI;


[RequireComponent(typeof(NavMeshAgent))]
public class GroundEnemyMovement : EnemyMovementBase
{
    [Header("Movement")]
    [SerializeField] private float moveSpeed = 3.5f;
    [SerializeField] private float stoppingDistance = 2f;
    [SerializeField] private float rotatSpeed = 720f;

    private NavMeshAgent agent;

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

        agent = GetComponent<NavMeshAgent>();

        agent.speed = moveSpeed;
        agent.stoppingDistance = stoppingDistance;
        agent.angularSpeed = rotationSpeed;

        // La rotación la manejamos nosotros.
        agent.updateRotation = false;
    }


    //=======================================================//
    // MOVEMENT LOCK
    //=======================================================//

    public override void SetMovementLocked(bool locked)
    {
        movementLocked = locked;

        if (agent == null)
            return;

        if (!agent.isActiveAndEnabled ||
            !agent.isOnNavMesh)
        {
            return;
        }

        agent.isStopped = locked;

        if (locked)
        {
            agent.ResetPath();
        }
    }


    //=======================================================//
    // MOVE
    //=======================================================//

    protected override void Move(Transform target)
    {
        if (movementLocked)
            return;

        if (agent == null ||
            !agent.isActiveAndEnabled ||
            !agent.isOnNavMesh)
        {
            return;
        }

        agent.isStopped = false;

        agent.SetDestination(target.position);
    }


    //=======================================================//
    // STOP
    //=======================================================//

    protected override void StopMovement()
    {
        if (agent == null)
            return;

        if (!agent.isActiveAndEnabled ||
            !agent.isOnNavMesh)
        {
            return;
        }

        agent.isStopped = true;
        agent.ResetPath();
    }

    //=======================================================//
    // REPEAL
    //=======================================================//

    protected override void MoveDuringRepel(Vector3 displacement)
    {
        if (agent != null &&
            agent.isActiveAndEnabled &&
            agent.isOnNavMesh)
        {
            agent.isStopped = true;
            agent.ResetPath();
            agent.Move(displacement);
        }
        else
        {
            transform.position += displacement;
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