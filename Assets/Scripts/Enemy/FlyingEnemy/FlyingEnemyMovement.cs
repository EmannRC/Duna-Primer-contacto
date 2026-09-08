using Unity.Netcode;
using UnityEngine;

public class FlyingEnemyMovement : EnemyMovementBase
{
    [Header("Movement")]
    [SerializeField] private float moveSpeed = 4f;
    [SerializeField] private float stoppingDistance = 2f;
    [SerializeField] private float rotationSpeed = 8f;

    [Header("Height")]
    [SerializeField] private float hoverHeight = 3f;
    [SerializeField] private float heightAdjustSpeed = 5f;
    [SerializeField] private float groundCheckDistance = 20f;
    [SerializeField] private LayerMask groundMask;

    [Header("Formation")]
    [SerializeField] private float formationRadius = 3f;

    private EnemyContext ctx;
    private bool movementLocked;


    //=======================================================//
    // AWAKE
    //=======================================================//

    private void Awake()
    {
        ctx = GetComponent<EnemyContext>();
    }


    //=======================================================//
    // MOVEMENT LOCK
    //=======================================================//

    public override void SetMovementLocked(bool locked)
    {
        movementLocked = locked;
    }


    //=======================================================//
    // UPDATE
    //=======================================================//

    private void Update()
    {
        if (!IsServer)
            return;

        if (movementLocked)
            return;

        if (ctx == null)
            return;

        if (ctx.targeting == null)
            return;

        Transform target =
            ctx.targeting.CurrentTarget;

        if (target == null)
            return;

        Move(target);
        LookAtTarget(target);
    }


    //=======================================================//
    // MOVE
    //=======================================================//

    private void Move(Transform target)
    {
        Vector3 position = transform.position;

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

        MaintainHeight(ref position);

        transform.position = position;
    }


    //=======================================================//
    // MAINTAIN HEIGHT
    //=======================================================//

    private void MaintainHeight(ref Vector3 position)
    {
        Vector3 rayOrigin =
            position + Vector3.up * 10f;

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
            hit.point.y + hoverHeight;

        position.y = Mathf.MoveTowards(
            position.y,
            desiredY,
            heightAdjustSpeed * Time.deltaTime
        );
    }


    //=======================================================//
    // LOOK AT TARGET
    //=======================================================//

    private void LookAtTarget(Transform target)
    {
        Vector3 direction =
            target.position - transform.position;

        direction.y = 0f;

        if (direction.sqrMagnitude < 0.001f)
            return;

        Quaternion targetRotation =
            Quaternion.LookRotation(direction);

        transform.rotation =
            Quaternion.Slerp(
                transform.rotation,
                targetRotation,
                rotationSpeed * Time.deltaTime
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
                transform.position - target.position;

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

        return target.position + offset;
    }
}

