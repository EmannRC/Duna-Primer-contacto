
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
        agent.angularSpeed = rotatSpeed;

        // La rotación la manejamos nosotros.
        agent.updateRotation = false;
    }

    protected override void Update()
    {
        base.Update();

        if (!IsServer || agent == null || !agent.isActiveAndEnabled)
            return;

        if (IsBeingRepelled || movementLocked || IsMovementPaused)
            return;

        Vector3 direction = agent.desiredVelocity;
        direction.y = 0f;

        if (direction.sqrMagnitude < 0.01f)
            return;

        Quaternion targetRotation = Quaternion.LookRotation(direction);

        transform.rotation = Quaternion.Slerp(
            transform.rotation,
            targetRotation,
            rotatSpeed * Time.deltaTime
        );
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


    public override void MoveToPosition(Vector3 position)
    {
        if (!IsServer || movementLocked)
            return;

        if (agent == null ||
            !agent.isActiveAndEnabled ||
            !agent.isOnNavMesh)
            return;

        agent.isStopped = false;
        agent.SetDestination(position);
    }

    public override bool HasReachedDestination
    {
        get
        {
            if (agent == null ||
                !agent.isActiveAndEnabled ||
                !agent.isOnNavMesh)
                return false;

            return !agent.pathPending &&
                   agent.hasPath &&
                   agent.remainingDistance <= agent.stoppingDistance + 0.2f;
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