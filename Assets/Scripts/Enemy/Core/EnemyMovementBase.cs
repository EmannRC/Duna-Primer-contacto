using Unity.Netcode;
using UnityEngine;

public abstract class EnemyMovementBase : NetworkBehaviour
{
    [Header("Rotation")]
    [SerializeField] protected float rotationSpeed = 8f;

    protected EnemyContext ctx;
    protected bool movementLocked;


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
}

