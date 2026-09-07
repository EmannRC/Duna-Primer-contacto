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
    [SerializeField] private float heightAdjustSpeed = 5f;
    [SerializeField] private float groundCheckDistance = 20f;
    [SerializeField] private LayerMask groundMask;

    [Header("Formation")]
    [SerializeField] private float formationRadius = 3f;

    private EnemyContext ctx;
    private Transform currentTarget;

    //=======================================================//
    // AWAKE
    //=======================================================//

    private void Awake()
    {
        ctx = GetComponent<EnemyContext>();
    }

    //=======================================================//
    // UPDATE
    //=======================================================//

    private void Update()
    {
        // El movimiento lo controla únicamente el servidor.
        if (!IsServer)
            return;

        if (ctx == null)
            return;

        if (ctx.targeting == null)
            return;

        Transform target = ctx.targeting.CurrentTarget;

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

        // Posición que debería ocupar dentro de la formación.
        Vector3 targetPosition = GetFormationTargetPosition(target);

        //===================================================//
        // MOVIMIENTO HORIZONTAL
        //===================================================//

        Vector3 direction = targetPosition - position;

        // No queremos que el movimiento horizontal modifique
        // la altura.
        direction.y = 0f;

        float distance = direction.magnitude;

        if (distance > stoppingDistance)
        {
            position +=
                direction.normalized *
                moveSpeed *
                Time.deltaTime;
        }

        //===================================================//
        // ALTURA SOBRE EL SUELO
        //===================================================//

        MaintainHeight(ref position);

        transform.position = position;
    }

    //=======================================================//
    // MAINTAIN HEIGHT
    //=======================================================//

    private void MaintainHeight(ref Vector3 position)
    {
        // Lanzamos el raycast desde arriba del enemigo.
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

        // Altura que queremos mantener respecto al suelo.
        float desiredY =
            hit.point.y + hoverHeight;

        // Movemos suavemente hacia la altura deseada.
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

        // No queremos que incline el cuerpo hacia arriba/abajo.
        direction.y = 0f;

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
    // FORMATION
    //=======================================================//

    private Vector3 GetFormationTargetPosition(Transform target)
    {
        // Si no tenemos formación, simplemente usamos
        // la posición del objetivo.
        if (ctx.formation == null)
            return target.position;

        int count =
            EnemyFormation.ActiveEnemies.Count;

        // Si solamente hay un enemigo, lo colocamos delante
        // del jugador en función de su posición actual.
        if (count <= 1)
        {
            Vector3 direction =
                transform.position - target.position;

            direction.y = 0f;

            if (direction.sqrMagnitude < 0.01f)
            {
                direction = -target.forward;
            }

            return target.position +
                   direction.normalized *
                   formationRadius;
        }

        //===================================================//
        // CALCULAR SLOT
        //===================================================//

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

    //=======================================================//
    // GIZMOS
    //=======================================================//

    private void OnDrawGizmosSelected()
    {
        //===================================================//
        // STOPPING DISTANCE
        //===================================================//

        Gizmos.color = Color.red;

        Gizmos.DrawWireSphere(
            transform.position,
            stoppingDistance
        );

        if (!Application.isPlaying)
            return;

        //===================================================//
        // FORMATION TARGET
        //===================================================//

        if (ctx != null &&
            ctx.targeting != null &&
            ctx.targeting.CurrentTarget != null)
        {
            Gizmos.color = Color.yellow;

            Vector3 formationPosition =
                GetFormationTargetPosition(
                    ctx.targeting.CurrentTarget
                );

            Gizmos.DrawSphere(
                formationPosition,
                0.2f
            );

            // Línea hacia el slot de formación.
            Gizmos.DrawLine(
                transform.position,
                formationPosition
            );
        }

        //===================================================//
        // GROUND RAYCAST
        //===================================================//

        Vector3 rayOrigin =
            transform.position +
            Vector3.up * 10f;

        Gizmos.color = Color.cyan;

        Gizmos.DrawLine(
            rayOrigin,
            rayOrigin +
            Vector3.down *
            groundCheckDistance
        );

        //===================================================//
        // HOVER POINT
        //===================================================//

        if (Physics.Raycast(
            rayOrigin,
            Vector3.down,
            out RaycastHit hit,
            groundCheckDistance,
            groundMask,
            QueryTriggerInteraction.Ignore))
        {
            Vector3 hoverPoint =
                hit.point +
                Vector3.up *
                hoverHeight;

            // Punto exacto donde debería estar el enemigo.
            Gizmos.DrawSphere(
                hoverPoint,
                0.15f
            );

            // Línea desde el enemigo hasta la altura objetivo.
            Gizmos.DrawLine(
                transform.position,
                hoverPoint
            );

            // Línea desde el suelo hasta la altura objetivo.
            Gizmos.DrawLine(
                hit.point,
                hoverPoint
            );
        }
    }
}

