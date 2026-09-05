
using Unity.Netcode;
using UnityEngine;
using UnityEngine.AI;


[RequireComponent(typeof(NavMeshAgent))]
public class GroundEnemyMovement : NetworkBehaviour
{
    [Header("Movement")]
    [SerializeField] private float moveSpeed = 3.5f;
    [SerializeField] private float stoppingDistance = 2f;

    private EnemyContext ctx;
    private NavMeshAgent agent;

    public Vector3 Velocity => agent.velocity;

    private void Awake()
    {
        ctx = GetComponent<EnemyContext>();
        agent = GetComponent<NavMeshAgent>();
    }

    private void Update()
    {
        if (!IsServer)
            return;

        agent.speed = moveSpeed;
        agent.stoppingDistance = stoppingDistance;
        agent.angularSpeed = 720f;

        // La rotación la controlamos nosotros.
        agent.updateRotation = false;

        Transform target = ctx.targeting.CurrentTarget;

        if (target == null || !agent.isOnNavMesh)
        {
            if (agent.isActiveAndEnabled && agent.isOnNavMesh)
                agent.ResetPath();

            return;
        }

        agent.SetDestination(target.position);

        LookAtTarget(target);
    }

    private void LookAtTarget(Transform target)
    {
        Vector3 direction = target.position - transform.position;
        direction.y = 0f;

        if (direction.sqrMagnitude < 0.01f)
            return;

        Quaternion targetRotation =
            Quaternion.LookRotation(direction);

        transform.rotation = Quaternion.Slerp(
            transform.rotation,
            targetRotation,
            8f * Time.deltaTime);
    }

    private void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.green;

        Gizmos.DrawWireSphere(
            transform.position,
            stoppingDistance);
    }
}