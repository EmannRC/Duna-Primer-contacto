using Unity.Netcode;
using UnityEngine;

public class FlyingEnemyMovement : EnemyMovementBase
{
    [Header("Movement")]
    [SerializeField] private float moveSpeed = 4f;
    [SerializeField] private float stoppingDistance = 2f;

    [Header("Height")]
    [SerializeField] private float hoverHeight = 3f;
    [SerializeField] private float heightAdjustSpeed = 5f;
    [SerializeField] private float groundCheckDistance = 20f;
    [SerializeField] private LayerMask groundMask;

    [Header("Formation")]
    [SerializeField] private float formationRadius = 3f;


    //=======================================================//
    // MOVEMENT LOCK
    //=======================================================//

    public override void SetMovementLocked(
        bool locked)
    {
        movementLocked = locked;
    }


    //=======================================================//
    // MOVE
    //=======================================================//

    protected override void Move(
        Transform target)
    {
        Vector3 position =
            transform.position;

        Vector3 targetPosition =
            GetFormationTargetPosition(target);

        Vector3 direction =
            targetPosition - position;

        direction.y = 0f;

        float distance =
            direction.magnitude;

        if (distance > stoppingDistance)
        {
            position +=
                direction.normalized *
                moveSpeed *
                Time.deltaTime;
        }

        MaintainHeight(
            ref position
        );

        transform.position =
            position;
    }


    //=======================================================//
    // STOP MOVEMENT
    //=======================================================//

    protected override void StopMovement()
    {
        // El enemigo volador no usa
        // NavMeshAgent, por lo que no
        // necesita ResetPath().
    }


    //=======================================================//
    // MAINTAIN HEIGHT
    //=======================================================//

    private void MaintainHeight(
        ref Vector3 position)
    {
        Vector3 rayOrigin =
            position +
            Vector3.up * 10f;

        if (!Physics.Raycast(
            rayOrigin,
            Vector3.down,
            out RaycastHit hit,
            groundCheckDistance,
            groundMask,
            QueryTriggerInteraction.Ignore))
        {
            return;
        }

        float desiredY =
            hit.point.y +
            hoverHeight;

        position.y =
            Mathf.MoveTowards(
                position.y,
                desiredY,
                heightAdjustSpeed *
                Time.deltaTime
            );
    }


    //=======================================================//
    // FORMATION
    //=======================================================//

    private Vector3 GetFormationTargetPosition(
        Transform target)
    {
        if (ctx.formation == null)
            return target.position;

        int count =
            EnemyFormation.ActiveEnemies.Count;

        if (count <= 1)
        {
            Vector3 direction =
                transform.position -
                target.position;

            direction.y = 0f;

            if (direction.sqrMagnitude < 0.01f)
                direction = -target.forward;

            return target.position +
                   direction.normalized *
                   formationRadius;
        }

        float angle =
            (360f / count) *
            ctx.formation.SlotIndex;

        Vector3 offset = new Vector3(
            Mathf.Cos(angle * Mathf.Deg2Rad),
            0f,
            Mathf.Sin(angle * Mathf.Deg2Rad)
        );

        offset *= formationRadius;

        return target.position +
               offset;
    }


    //=======================================================//
    // GIZMOS
    //=======================================================//

    private void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.cyan;

        Gizmos.DrawWireSphere(
            transform.position,
            formationRadius
        );
    }
}

