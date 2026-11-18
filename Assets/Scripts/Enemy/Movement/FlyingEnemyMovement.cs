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

    [Header("Death Fall")]
    [SerializeField] private float gravity = -20f;
    [SerializeField] private float groundOffset = 0.5f;

    private bool isFalling;
    private bool hasLanded;

    private float verticalVelocity;

    //=======================================================//
    // MOVEMENT
    //=======================================================//

    protected override void UpdateMovement()
    {
        // Prioridad: caída por muerte.
        if (isFalling)
        {
            UpdateFall();
            return;
        }

        if (movementLocked || IsMovementPaused)
            return;

        MaintainHoverHeight();
        MoveTowardsDestination();
    }

    protected override void MoveTowardsDestination()
    {
        if (!hasDestination || HasReachedDestination)
            return;

        Vector3 direction =
            destination - transform.position;

        direction.y = 0f;

        if (direction.sqrMagnitude < 0.001f)
            return;

        direction.Normalize();

        transform.position +=
            direction * moveSpeed * Time.deltaTime;

        RotateTowards(direction);
    }

    //=======================================================//
    // DEATH
    //=======================================================//

    public override void OnDeath()
    {
        isFalling = true;
        hasLanded = false;

        verticalVelocity = 0f;
    }

    private void UpdateFall()
    {
        if (hasLanded)
            return;

        // Aplicar gravedad.
        verticalVelocity += gravity * Time.deltaTime;

        Vector3 position = transform.position;

        position.y += verticalVelocity * Time.deltaTime;

        // Comprobar suelo.
        Vector3 origin =
            transform.position + Vector3.up * 0.5f;

        if (Physics.Raycast(
            origin,
            Vector3.down,
            out RaycastHit hit,
            groundCheckDistance,
            groundMask,
            QueryTriggerInteraction.Ignore))
        {
            float targetHeight =
                hit.point.y + groundOffset;

            // Comprobar si llegó al suelo.
            if (position.y <= targetHeight)
            {
                position.y = targetHeight;

                verticalVelocity = 0f;

                hasLanded = true;
                isFalling = false;
            }
        }

        transform.position = position;
    }

    //=======================================================//
    // HOVER HEIGHT
    //=======================================================//

    private void MaintainHoverHeight()
    {
        Vector3 origin =
            transform.position + Vector3.up * 0.5f;

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

        float targetHeight =
            hit.point.y + hoverHeight;

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

