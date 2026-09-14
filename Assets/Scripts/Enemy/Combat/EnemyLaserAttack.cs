using Unity.Netcode;
using UnityEngine;

public class EnemyLaserAttack : NetworkBehaviour
{
    [Header("Laser")]
    [SerializeField] private int damage = 15;
    [SerializeField] private float minLaserRange = 3f;
    [SerializeField] private float maxLaserRange = 10f;
    [SerializeField] private float attackCooldown = 3f;
    [SerializeField] private LayerMask damageLayers;

    [Header("Aim")]
    [SerializeField] private float turnSpeed = 90f;

    [Header("References")]
    [SerializeField] private Transform firePoint;
    [SerializeField] private LineRenderer lineRenderer;

    [Header("Visual")]
    [SerializeField] private float laserDuration = 0.15f;

    private EnemyContext ctx;

    private float nextAttackTime;

    private bool isAttacking;
    private Transform attackTarget;


    //========================================================//
    // INITIALIZE
    //========================================================//

    private void Awake()
    {
        ctx = GetComponentInParent<EnemyContext>();

        if (lineRenderer != null)
            lineRenderer.enabled = false;
    }


    //========================================================//
    // UPDATE
    //========================================================//

    private void Update()
    {
        if (!IsServer)
            return;

        // Mientras prepara el láser,
        // gira hacia el jugador.
        if (isAttacking)
        {
            AimAtTarget();
            return;
        }

        if (Time.time < nextAttackTime)
            return;

        Transform target = ctx.targeting.CurrentTarget;

        if (!IsValidTarget(target))
            return;

        float distance =
            Vector3.Distance(
                transform.position,
                target.position
            );

        if (distance < minLaserRange ||
            distance > maxLaserRange)
        {
            return;
        }

        StartLaserAttack(target);
    }


    //========================================================//
    // AIM
    //========================================================//

    private void AimAtTarget()
    {
        if (!IsValidTarget(attackTarget))
            return;

        Vector3 direction =
            attackTarget.position - transform.position;

        direction.y = 0f;

        if (direction.sqrMagnitude < 0.001f)
            return;

        Quaternion targetRotation =
            Quaternion.LookRotation(direction);

        transform.rotation =
            Quaternion.RotateTowards(
                transform.rotation,
                targetRotation,
                turnSpeed * Time.deltaTime
            );
    }


    //========================================================//
    // START ATTACK
    //========================================================//

    private void StartLaserAttack(Transform target)
    {
        isAttacking = true;
        attackTarget = target;

        nextAttackTime =
            Time.time + attackCooldown;

        if (ctx.enemyAnimation != null)
        {
            ctx.enemyAnimation.NotifyLaserAttack();
        }
    }


    //========================================================//
    // ANIMATION EVENT
    //========================================================//

    public void AnimationEventFireLaser()
    {
        if (!IsServer)
            return;

        if (!isAttacking)
            return;

        if (firePoint == null || attackTarget == null)
        {
            CancelAttack();
            return;
        }

        // Dirección desde el FirePoint
        // hacia la posición ACTUAL del jugador.
        Vector3 direction =
            attackTarget.position -
            firePoint.position;

        if (direction.sqrMagnitude < 0.001f)
        {
            CancelAttack();
            return;
        }

        direction.Normalize();

        FireLaser(direction);
    }


    //========================================================//
    // FIRE LASER
    //========================================================//

    private void FireLaser(Vector3 direction)
    {
        Vector3 start =
            firePoint.position;

        Vector3 end =
            start +
            direction *
            maxLaserRange;

        // Comprueba si el láser golpea algo.
        if (Physics.Raycast(
                start,
                direction,
                out RaycastHit hit,
                maxLaserRange,
                damageLayers))
        {
            end = hit.point;

            PlayerHealth health =
                hit.collider.GetComponentInParent<PlayerHealth>();

            if (health != null &&
                !health.IsDead.Value)
            {
                health.TakeDamage(damage);
            }
        }

        ShowLaserClientRpc(start, end);

        CancelAttack();
    }


    //========================================================//
    // VISUAL
    //========================================================//

    [Rpc(SendTo.ClientsAndHost)]
    private void ShowLaserClientRpc(
        Vector3 start,
        Vector3 end)
    {
        if (lineRenderer == null)
            return;

        lineRenderer.SetPosition(0, start);
        lineRenderer.SetPosition(1, end);

        lineRenderer.enabled = true;

        CancelInvoke(nameof(HideLaser));

        Invoke(
            nameof(HideLaser),
            laserDuration
        );
    }


    private void HideLaser()
    {
        if (lineRenderer != null)
            lineRenderer.enabled = false;
    }


    //========================================================//
    // CANCEL
    //========================================================//

    private void CancelAttack()
    {
        isAttacking = false;
        attackTarget = null;
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

        return !health.IsDead.Value;
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
