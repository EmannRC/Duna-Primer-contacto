using Unity.Netcode;
using UnityEngine;

public class EnemyLaserAttack : NetworkBehaviour
{
    [Header("Laser")]
    [SerializeField] private int damage = 15;
    [SerializeField] private float minLaserRange = 3f;
    [SerializeField] private float maxLaserRange = 10f;
    [SerializeField] private float attackCooldown = 3f;

    [Header("References")]
    [SerializeField] private Transform firePoint;

    private EnemyContext ctx;
    private float nextAttackTime;


    //========================================================//
    // INITIALIZE
    //========================================================//

    private void Awake()
    {
        ctx = GetComponent<EnemyContext>();
    }


    //========================================================//
    // UPDATE
    //========================================================//

    private void Update()
    {
        if (!IsServer)
            return;

        if (Time.time < nextAttackTime)
            return;

        if (ctx == null || ctx.targeting == null)
            return;

        Transform target = ctx.targeting.CurrentTarget;

        if (!IsValidTarget(target))
            return;

        float distance =
            Vector3.Distance(
                transform.position,
                target.position
            );

        // El jugador debe estar dentro del rango del láser.
        if (distance < minLaserRange ||
            distance > maxLaserRange)
            return;

        StartLaserAttack(target);
    }


    //========================================================//
    // ATTACK
    //========================================================//

    private void StartLaserAttack(Transform target)
    {
        if (firePoint == null)
            return;

        nextAttackTime =
            Time.time + attackCooldown;

        Vector3 targetPosition = target.position;

        Vector3 direction =
            targetPosition - firePoint.position;

        if (direction.sqrMagnitude <= 0.001f)
            return;

        direction.Normalize();

        // Daño real.
        DealDamage(target);

        // Efecto visual para todos.
        ShowLaserClientRpc(
            firePoint.position,
            targetPosition
        );
    }


    //========================================================//
    // DAMAGE
    //========================================================//

    private void DealDamage(Transform target)
    {
        if (!IsValidTarget(target))
            return;

        PlayerHealth health =
            target.GetComponent<PlayerHealth>();

        if (health == null)
            return;

        health.TakeDamage(damage);
    }


    //========================================================//
    // VISUAL
    //========================================================//

    [Rpc(SendTo.ClientsAndHost)]
    private void ShowLaserClientRpc(
        Vector3 start,
        Vector3 end)
    {
        // Acá después ponemos el efecto visual.
        // Por ejemplo:
        // - LineRenderer
        // - partículas
        // - impacto en el jugador
    }


    //========================================================//
    // VALIDATION
    //========================================================//

    private bool IsValidTarget(Transform target)
    {
        if (target == null)
            return false;

        if (!target.TryGetComponent<PlayerHealth>(
                out PlayerHealth health))
        {
            return false;
        }

        if (health.IsDead.Value)
            return false;

        return true;
    }


    //========================================================//
    // GIZMOS
    //========================================================//

    private void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.yellow;

        Gizmos.DrawWireSphere(
            transform.position,
            maxLaserRange
        );

        Gizmos.color = Color.red;

        Gizmos.DrawWireSphere(
            transform.position,
            minLaserRange
        );
    }
}
