using Unity.Netcode;
using UnityEngine;

public class EnemyAIController : NetworkBehaviour
{
    public enum AIState
    {
        Patrol,
        Chase
    }

    [Header("References")]
    [SerializeField] private EnemyTargeting targeting;
    [SerializeField] private EnemyMovementBase movement;

    [Header("Patrol")]
    [SerializeField] private Transform[] patrolPoints;
    [SerializeField] private float waitAtPoint = 2f;

    private AIState currentState;

    private int currentPatrolIndex;
    private float waitTimer;

    private bool patrolInitialized;

    //=======================================================//
    // UPDATE
    //=======================================================//

    private void Update()
    {
        if (!IsServer)
            return;

        if (targeting == null || movement == null)
            return;

        Transform target = targeting.CurrentTarget;

        if (target != null)
        {
            if (currentState != AIState.Chase)
                ChangeState(AIState.Chase);

            Chase(target);
        }
        else
        {
            if (currentState != AIState.Patrol)
                ChangeState(AIState.Patrol);

            Patrol();
        }
    }

    //=======================================================//
    // STATE
    //=======================================================//

    private void ChangeState(AIState newState)
    {
        if (currentState == newState)
            return;

        currentState = newState;

        switch (currentState)
        {
            case AIState.Patrol:
                movement.Stop();
                patrolInitialized = false;
                waitTimer = 0f;
                break;

            case AIState.Chase:
                movement.Stop();
                break;
        }
    }

    //=======================================================//
    // PATROL
    //=======================================================//

    private void Patrol()
    {
        if (patrolPoints == null ||
            patrolPoints.Length == 0)
            return;

        if (!patrolInitialized)
        {
            patrolInitialized = true;
            MoveToNextPoint();
            return;
        }

        if (!movement.HasReachedDestination)
            return;

        waitTimer += Time.deltaTime;

        if (waitTimer >= waitAtPoint)
        {
            MoveToNextPoint();
        }
    }

    private void MoveToNextPoint()
    {
        waitTimer = 0f;

        currentPatrolIndex =
            (currentPatrolIndex + 1) % patrolPoints.Length;

        Transform point = patrolPoints[currentPatrolIndex];

        if (point == null)
            return;

        movement.MoveToPosition(point.position);
    }

    //=======================================================//
    // CHASE
    //=======================================================//

    private void Chase(Transform target)
    {
        movement.MoveTo(target);
    }
}
