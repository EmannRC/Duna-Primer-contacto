using Unity.Netcode;
using UnityEngine;
using UnityEngine.AI;
using System.Collections;

public class EnforcerSpecialAttack : NetworkBehaviour
{
    [Header("Special Attack")]
    [SerializeField] private float cooldown = 8f;
    [SerializeField] private float minTriggerDistance = 3f;
    [SerializeField] private float maxTriggerDistance = 10f;

    [Header("Animation")]
    [SerializeField] private string jumpTrigger = "JumpAttack";

    [Tooltip("Tiempo total aproximado de la animación")]
    [SerializeField] private float attackDuration = 1.5f;

    [Header("Landing")]
    [SerializeField] private float navMeshSearchRadius = 3f;

    [Header("Impact")]
    [SerializeField] private float damageRadius = 4f;
    [SerializeField] private int damage = 30;
    [SerializeField] private LayerMask damageLayers;

    private EnemyContext ctx;
    private NavMeshAgent agent;

    private float lastAttackTime = -999f;

    private bool isPerformingSpecialAttack;
    private bool impactPerformed;

    private Vector3 landingPosition;

    public bool IsPerformingSpecialAttack =>
        isPerformingSpecialAttack;

    private void Awake()
    {
        ctx = GetComponent<EnemyContext>();
        agent = GetComponent<NavMeshAgent>();
    }

    private void Update()
    {
        if (!IsServer)
            return;

        if (isPerformingSpecialAttack)
            return;

        Transform target = ctx.targeting.CurrentTarget;

        if (target == null)
            return;

        if (Time.time < lastAttackTime + cooldown)
            return;

        float distance = Vector3.Distance(
            transform.position,
            target.position
        );

        if (distance < minTriggerDistance ||
            distance > maxTriggerDistance)
        {
            return;
        }

        StartCoroutine(SpecialAttackRoutine(target));
    }

    private IEnumerator SpecialAttackRoutine(Transform target)
    {
        isPerformingSpecialAttack = true;
        impactPerformed = false;

        lastAttackTime = Time.time;

        // Guardamos la posición del jugador
        // al momento de iniciar el salto
        landingPosition = target.position;

        // Detener NavMeshAgent
        if (agent != null &&
            agent.isOnNavMesh)
        {
            agent.isStopped = true;
            agent.ResetPath();
        }

        // Mirar hacia donde vamos a saltar
        LookAtPosition(landingPosition);

        // Reproducir animación
        if (ctx.animationSync != null)
        {
            ctx.animationSync.NotifySpecialAttack();
        }

        // Esperamos que termine la animación
        yield return new WaitForSeconds(attackDuration);

        // Seguridad:
        // Si por alguna razón el Animation Event
        // no llamó a Impact(), lo ejecutamos.
        if (!impactPerformed)
        {
            Impact();
        }

        // Pequeña pausa después del impacto
        yield return new WaitForSeconds(0.1f);

        // Reactivar movimiento
        if (agent != null &&
            agent.isOnNavMesh)
        {
            agent.isStopped = false;
        }

        isPerformingSpecialAttack = false;
    }

    private void LookAtPosition(Vector3 position)
    {
        Vector3 direction =
            position - transform.position;

        direction.y = 0f;

        if (direction.sqrMagnitude < 0.01f)
            return;

        transform.rotation =
            Quaternion.LookRotation(direction);
    }

    // ESTA FUNCIÓN LA LLAMA EL ANIMATION EVENT
    public void Impact()
    {
        if (!IsServer)
            return;

        if (!isPerformingSpecialAttack)
            return;

        if (impactPerformed)
            return;

        impactPerformed = true;

        MoveToLandingPosition();

        DealRadialDamage();
    }

    private void MoveToLandingPosition()
    {
        if (agent == null)
            return;

        if (NavMesh.SamplePosition(
            landingPosition,
            out NavMeshHit hit,
            navMeshSearchRadius,
            NavMesh.AllAreas))
        {
            agent.Warp(hit.position);
        }
    }

    private void DealRadialDamage()
    {
        Collider[] hits = Physics.OverlapSphere(
            transform.position,
            damageRadius,
            damageLayers
        );

        foreach (Collider hit in hits)
        {
            PlayerHealth health =
                hit.GetComponentInParent<PlayerHealth>();

            if (health != null)
            {
                health.TakeDamage(damage);
            }
        }
    }

    private void OnDrawGizmosSelected()
    {
        // Distancia mínima
        Gizmos.color = Color.cyan;
        Gizmos.DrawWireSphere(
            transform.position,
            minTriggerDistance
        );

        // Distancia máxima
        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(
            transform.position,
            maxTriggerDistance
        );

        // Daño radial
        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(
            transform.position,
            damageRadius
        );
    }
}
