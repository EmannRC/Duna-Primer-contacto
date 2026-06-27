using Unity.Netcode;
using UnityEngine;

public class FlyingEnemyMovement : NetworkBehaviour
{
    [Header("Movement")]
    [SerializeField] private float moveSpeed = 4f;
    [SerializeField] private float stoppingDistance = 2f;
    [SerializeField] private float rotationSpeed = 8f;

    [Header("Height")]
    [SerializeField] private float hoverHeight = 3f;
    [SerializeField] private float heightLerpSpeed = 5f;
    [SerializeField] private float groundCheckDistance = 20f;
    [SerializeField] private LayerMask groundMask;

    [Header("Formation")]
    [SerializeField] private float formationRadius = 3f;



    private EnemyContext ctx;
    private EnemyFormation formation;

    //========================================================//

    private void Awake()
    {
        ctx = GetComponent<EnemyContext>();
        formation = GetComponent<EnemyFormation>();
    }

    private void Update()
    {
        if (!IsServer)
            return;

        Transform target = ctx.targeting.CurrentTarget;

        if (target == null)
            return;

        Move();
        MaintainHeight();
        LookAtTarget(target);
    }

    //========================================================//

    private void Move()
    {
        Vector3 targetPosition = GetFormationTargetPosition();

        Vector3 direction =
            targetPosition - transform.position;

        direction.y = 0f;

        float distance = direction.magnitude;

        if (distance <= stoppingDistance)
            return;

        transform.position +=
            direction.normalized *
            moveSpeed *
            Time.deltaTime;
    }

    private void MaintainHeight()
    {
        Vector3 rayOrigin = transform.position + Vector3.up * 5f;

        if (Physics.Raycast(
            rayOrigin,
            Vector3.down,
            out RaycastHit hit,
            groundCheckDistance,
            groundMask))
        {
            float desiredY = hit.point.y + hoverHeight;

            Vector3 position = transform.position;

            position.y = Mathf.Lerp(
                position.y,
                desiredY,
                heightLerpSpeed * Time.deltaTime);

            transform.position = position;
        }
    }

    private void LookAtTarget(Transform target)
    {
        Vector3 direction =
            target.position - transform.position;

        direction.y = 0f;

        if (direction.sqrMagnitude < 0.01f)
            return;

        Quaternion targetRotation =
            Quaternion.LookRotation(direction);

        transform.rotation = Quaternion.Slerp(
            transform.rotation,
            targetRotation,
            rotationSpeed * Time.deltaTime);
    }

    //========================================================//

    private Vector3 GetFormationTargetPosition()
    {
        Transform target = ctx.targeting.CurrentTarget;

        if (target == null)
            return transform.position;

        if (formation == null)
            return target.position;

        int count = EnemyFormation.ActiveEnemies.Count;

        if (count <= 1)
            return target.position;

        float angle =
            (360f / count) * formation.SlotIndex;

        Vector3 offset = new Vector3(
            Mathf.Cos(angle * Mathf.Deg2Rad),
            0f,
            Mathf.Sin(angle * Mathf.Deg2Rad)
        ) * formationRadius;

        return target.position + offset;
    }

    //========================================================//

    private void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(
            transform.position,
            stoppingDistance);

        if (Application.isPlaying &&
            ctx != null &&
            ctx.targeting.CurrentTarget != null)
        {
            Gizmos.color = Color.yellow;

            Gizmos.DrawSphere(
                GetFormationTargetPosition(),
                0.2f);
        }
    }
}
