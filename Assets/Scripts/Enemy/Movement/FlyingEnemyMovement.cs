using Unity.Netcode;
using UnityEngine;

public class FlyingEnemyMovement : EnemyMovementBase
{
    [Header("Movement")]
    [SerializeField] private float moveSpeed = 4f;

    [Header("Hover")]
    [SerializeField] private float hoverHeight = 3f;
    [SerializeField] private float heightAdjustSpeed = 5f;
    [SerializeField] private float groundCheckDistance = 20f;
    [SerializeField] private LayerMask groundMask;

    //=======================================================//
    // UPDATE
    //=======================================================//

    protected override void Update()
    {
        bool wasBeingRepelled = IsBeingRepelled;

        base.Update();

        if (!IsServer || wasBeingRepelled || IsBeingRepelled)
            return;

        if (movementLocked || IsMovementPaused)
            return;

        MaintainHoverHeight();

        MoveTowardsDestination();
    }

    //=======================================================//
    // MOVEMENT LOCK
    //=======================================================//

    public override void SetMovementLocked(bool locked)
    {
        movementLocked = locked;
    }

    //=======================================================//
    // MOVE
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
        if (!hasDestination || HasReachedDestination)
            return;

        Vector3 direction = destination - transform.position;

        // Movimiento horizontal.
        direction.y = 0f;

        if (direction.sqrMagnitude < 0.001f)
            return;

        direction.Normalize();

        transform.position +=
            direction * moveSpeed * Time.deltaTime;

        RotateTowards(direction);
    }

    //=======================================================//
    // HOVER HEIGHT
    //=======================================================//

    private void MaintainHoverHeight()
    {
        Vector3 origin = transform.position + Vector3.up * 0.5f;

        if (!Physics.Raycast(
            origin,
            Vector3.down,
            out RaycastHit hit,
            groundCheckDistance,
            groundMask,
            QueryTriggerInteraction.Ignore))
        {
            return;
        }

        float targetHeight = hit.point.y + hoverHeight;

        float newHeight = Mathf.Lerp(
            transform.position.y,
            targetHeight,
            heightAdjustSpeed * Time.deltaTime
        );

        Vector3 position = transform.position;
        position.y = newHeight;

        transform.position = position;
    }

    //=======================================================//
    // STOP MOVEMENT
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
        transform.position += displacement;
    }

    //=======================================================//
    // GIZMOS
    //=======================================================//

    private void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.green;

        Gizmos.DrawRay(
            transform.position + Vector3.up * 0.5f,
            Vector3.down * groundCheckDistance
        );

        Gizmos.color = Color.yellow;

        Gizmos.DrawWireSphere(
            transform.position,
            stoppingDistance
        );
    }
}

