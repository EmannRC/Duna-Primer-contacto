using System.Collections.Generic;
using UnityEngine;

public class EnemySteering : MonoBehaviour
{
    [Header("Separation")]
    [SerializeField] private float separationRadius = 2.5f;
    [SerializeField] private float separationWeight = 2f;
    [SerializeField] private LayerMask enemyMask;

    [Header("Obstacle Avoidance")]
    [SerializeField] private float obstacleDetectionDistance = 2f;
    [SerializeField] private float obstacleRadius = 0.5f;
    [SerializeField] private float obstacleAvoidanceWeight = 3f;
    [SerializeField] private LayerMask obstacleMask;

    private readonly Collider[] nearbyColliders =
        new Collider[32];

    private readonly HashSet<EnemyMovementBase> processedEnemies =
        new HashSet<EnemyMovementBase>();


    public Vector3 CalculateDirection(Vector3 desiredDirection)
    {
        desiredDirection.y = 0f;

        if (desiredDirection.sqrMagnitude < 0.001f)
            return Vector3.zero;

        desiredDirection.Normalize();

        Vector3 separation =
            CalculateSeparation();

        Vector3 avoidance =
            CalculateObstacleAvoidance(desiredDirection);

        Vector3 finalDirection =
            desiredDirection +
            separation * separationWeight +
            avoidance * obstacleAvoidanceWeight;

        finalDirection.y = 0f;

        return finalDirection.sqrMagnitude > 0.001f
            ? finalDirection.normalized
            : desiredDirection;
    }


    //=======================================================//
    // SEPARATION
    //=======================================================//

    private Vector3 CalculateSeparation()
    {
        Vector3 separation = Vector3.zero;

        int count = Physics.OverlapSphereNonAlloc(
            transform.position,
            separationRadius,
            nearbyColliders,
            enemyMask,
            QueryTriggerInteraction.Ignore
        );

        processedEnemies.Clear();

        int neighbors = 0;

        for (int i = 0; i < count; i++)
        {
            Collider other = nearbyColliders[i];

            if (other == null)
                continue;

            EnemyMovementBase enemy =
                other.GetComponentInParent<EnemyMovementBase>();

            if (enemy == null ||
                enemy.transform == transform ||
                !processedEnemies.Add(enemy))
                continue;

            Vector3 direction =
                transform.position - enemy.transform.position;

            direction.y = 0f;

            float distance = direction.magnitude;

            if (distance < 0.001f)
                continue;

            separation +=
                direction.normalized *
                (1f - distance / separationRadius);

            neighbors++;
        }

        return neighbors > 0
            ? separation / neighbors
            : Vector3.zero;
    }


    //=======================================================//
    // OBSTACLE AVOIDANCE
    //=======================================================//

    private Vector3 CalculateObstacleAvoidance(
        Vector3 desiredDirection)
    {
        Vector3 origin =
            transform.position + Vector3.up * 0.5f;

        if (!Physics.SphereCast(
            origin,
            obstacleRadius,
            desiredDirection,
            out RaycastHit hit,
            obstacleDetectionDistance,
            obstacleMask,
            QueryTriggerInteraction.Ignore))
        {
            return Vector3.zero;
        }

        Vector3 avoidance =
            Vector3.ProjectOnPlane(
                desiredDirection,
                hit.normal
            );

        if (avoidance.sqrMagnitude < 0.001f)
        {
            avoidance =
                Vector3.Cross(hit.normal, Vector3.up);
        }

        return avoidance.normalized;
    }


    //=======================================================//
    // GIZMOS
    //=======================================================//

    private void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.green;

        Gizmos.DrawWireSphere(
            transform.position,
            separationRadius
        );

        Gizmos.color = Color.yellow;

        Gizmos.DrawRay(
            transform.position + Vector3.up * 0.5f,
            transform.forward * obstacleDetectionDistance
        );
    }
}
