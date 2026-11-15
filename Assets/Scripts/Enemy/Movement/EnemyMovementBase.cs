using Unity.Netcode;
using UnityEngine;

public abstract class EnemyMovementBase : NetworkBehaviour
{
    [Header("Movement")]
    [SerializeField] protected float stoppingDistance = 2f;

    [Header("Rotation")]
    [SerializeField] protected float rotationSpeed = 8f;

    [Header("Repel")]
    [SerializeField] private float repelResistance = 1f;

    protected EnemyContext ctx;
    protected bool movementLocked;

    protected Vector3 destination;
    protected bool hasDestination;

    private Vector3 repelDirection;
    private float repelSpeed;
    private float repelRemaining;

    public bool IsBeingRepelled => repelRemaining > 0f;

    //=======================================================//
    // AWAKE
    //=======================================================//

    protected virtual void Awake()
    {
        ctx = GetComponent<EnemyContext>();
    }

    //=======================================================//
    // UPDATE
    //=======================================================//

    protected virtual void Update()
    {
        if (!IsServer)
            return;

        if (IsBeingRepelled)
        {
            float delta = Mathf.Min(
                Time.deltaTime,
                repelRemaining
            );

            MoveDuringRepel(
                repelDirection * repelSpeed * delta
            );

            repelRemaining -= delta;

            if (repelRemaining <= 0f)
                OnRepelFinished();

            return;
        }
    }

    //=======================================================//
    // MOVEMENT
    //=======================================================//

    protected abstract void Move(Transform target);

    protected abstract void MoveTowardsDestination();

    protected virtual bool IsMovementPaused => false;

    //=======================================================//
    // DESTINATION
    //=======================================================//

    public virtual void MoveToPosition(Vector3 position)
    {
        if (!IsServer ||
            movementLocked ||
            IsMovementPaused ||
            IsBeingRepelled)
            return;

        destination = position;
        hasDestination = true;
    }

    public virtual bool HasReachedDestination
    {
        get
        {
            if (!hasDestination)
                return false;

            Vector3 difference =
                destination - transform.position;

            difference.y = 0f;

            return difference.magnitude <= stoppingDistance + 0.2f;
        }
    }

    //=======================================================//
    // ROTATION
    //=======================================================//

    protected void RotateTowards(Vector3 direction)
    {
        if (direction.sqrMagnitude < 0.001f)
            return;

        Quaternion targetRotation =
            Quaternion.LookRotation(direction);

        transform.rotation = Quaternion.Slerp(
            transform.rotation,
            targetRotation,
            rotationSpeed * Time.deltaTime
        );
    }

    //=======================================================//
    // STOP MOVEMENT
    //=======================================================//

    protected virtual void StopMovement()
    {
        hasDestination = false;
    }

    public void Stop()
    {
        if (!IsServer)
            return;

        StopMovement();
    }

    //=======================================================//
    // MOVEMENT LOCK
    //=======================================================//

    public abstract void SetMovementLocked(bool locked);

    //=======================================================//
    // MOVEMENT COMMANDS
    //=======================================================//

    public void MoveTo(Transform target)
    {
        if (!IsServer ||
            movementLocked ||
            IsMovementPaused ||
            IsBeingRepelled ||
            target == null)
            return;

        Move(target);
    }

    //=======================================================//
    // REPEL
    //=======================================================//

    public void ApplyRepel(
        Vector3 sourcePosition,
        float force,
        float duration)
    {
        if (!IsServer ||
            duration <= 0f ||
            force <= 0f ||
            movementLocked)
            return;

        repelDirection =
            transform.position - sourcePosition;

        repelDirection.y = 0f;

        if (repelDirection.sqrMagnitude < 0.001f)
            repelDirection = transform.forward;

        repelDirection.Normalize();

        repelSpeed =
            force / Mathf.Max(0.1f, duration);

        repelSpeed /=
            Mathf.Max(0.1f, repelResistance);

        repelRemaining = duration;

        StopMovement();
    }

    protected virtual void MoveDuringRepel(
        Vector3 displacement)
    {
        transform.position += displacement;
    }

    protected virtual void OnRepelFinished()
    {
    }
}

