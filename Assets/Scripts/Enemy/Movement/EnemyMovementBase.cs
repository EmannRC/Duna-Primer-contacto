using Unity.Netcode;
using UnityEngine;

public abstract class EnemyMovementBase : NetworkBehaviour
{
    [Header("Rotation")]
    [SerializeField] protected float rotationSpeed = 8f;

    protected EnemyContext ctx;
    protected bool movementLocked;

    [Header("Repel")]
    [SerializeField] private float repelResistance = 1f;

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
                repelRemaining);

            MoveDuringRepel(repelDirection * repelSpeed * delta);
            repelRemaining -= delta;
            return;
        }

        if (movementLocked)
            return;

        if (IsMovementPaused)
            return;

        if (!TryGetValidTarget(out Transform target))
        {
            StopMovement();
            return;
        }

        Move(target);
        LookAtTarget(target);
    }


    //=======================================================//
    // MOVEMENT
    //=======================================================//

    protected abstract void Move(Transform target);


    //=======================================================//
    // STOP MOVEMENT
    //=======================================================//

    protected virtual void StopMovement()
    {
    }


    //=======================================================//
    // MOVEMENT PAUSED
    //=======================================================//

    protected virtual bool IsMovementPaused => false;


    //=======================================================//
    // TARGET
    //=======================================================//

    protected bool TryGetValidTarget(
        out Transform target)
    {
        target = null;

        if (ctx == null ||
            ctx.targeting == null)
        {
            return false;
        }

        target =
            ctx.targeting.CurrentTarget;

        if (target == null)
            return false;

        if (target.TryGetComponent(
                out PlayerHealth health) &&
            health.IsDead.Value)
        {
            target = null;
            return false;
        }

        return true;
    }


    //=======================================================//
    // LOOK AT TARGET
    //=======================================================//

    protected void LookAtTarget(
        Transform target)
    {
        if (target == null)
            return;

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
    // MOVEMENT LOCK
    //=======================================================//

    public abstract void SetMovementLocked(
        bool locked);

    //=======================================================//
    // REPEAL
    //=======================================================//
    public void ApplyRepel(
    Vector3 sourcePosition,
    float force,
    float duration)
    {
        if (!IsServer || duration <= 0f || force <= 0f)
            return;

        repelDirection = transform.position - sourcePosition;
        repelDirection.y = 0f;

        if (repelDirection.sqrMagnitude < 0.001f)
            repelDirection = transform.forward;

        repelDirection.Normalize();

        repelSpeed = force / Mathf.Max(0.1f, duration);
        repelSpeed /= Mathf.Max(0.1f, repelResistance);

        repelRemaining = duration;
    }

    protected virtual void MoveDuringRepel(Vector3 displacement)
    {
        transform.position += displacement;
    }
}

