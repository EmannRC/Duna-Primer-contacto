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

    protected virtual bool IsMovementPaused => false;

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
            UpdateRepel();
            return;
        }

        UpdateMovement();
    }

    private void UpdateRepel()
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
        {
            repelRemaining = 0f;
            OnRepelFinished();
        }
    }

    //=======================================================//
    // MOVEMENT
    //=======================================================//

    protected abstract void UpdateMovement();

    protected abstract void MoveTowardsDestination();

    //=======================================================//
    // DESTINATION
    //=======================================================//

    public virtual void MoveToPosition(Vector3 position)
    {
        if (!CanReceiveMovementCommand())
            return;

        destination = position;
        hasDestination = true;
    }

    public void MoveTo(Transform target)
    {
        if (target == null ||
            !CanReceiveMovementCommand())
            return;

        destination = target.position;
        hasDestination = true;
    }

    private bool CanReceiveMovementCommand()
    {
        return IsServer &&
               !movementLocked &&
               !IsMovementPaused &&
               !IsBeingRepelled;
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

    public virtual void SetMovementLocked(bool locked)
    {
        if (!IsServer)
            return;

        if (movementLocked == locked)
            return;

        movementLocked = locked;

        if (locked)
        {
            StopMovement();
            //OnMovementLocked();
        }
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
            force <= 0f)
            return;

        repelDirection =
            transform.position - sourcePosition;

        repelDirection.y = 0f;

        if (repelDirection.sqrMagnitude < 0.001f)
        {
            // Si ambos pivotes coinciden, el enemigo suele estar mirando
            // hacia el jugador; empujarlo hacia atrás evita un vector nulo.
            repelDirection = -transform.forward;
            repelDirection.y = 0f;
        }

        if (repelDirection.sqrMagnitude < 0.001f)
            repelDirection = Vector3.back;

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

    public virtual void OnDeath()
    {
    }
}

