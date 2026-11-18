using Unity.Netcode;
using UnityEngine;

public class EnemyLaserAttack : NetworkBehaviour
{
    [Header("Combat")]
    [SerializeField] private float minLaserRange = 3f;
    [SerializeField] private float maxLaserRange = 10f;
    [SerializeField] private float attackCooldown = 3f;

    [Header("Aim")]
    [SerializeField] private float turnSpeed = 90f;

    [Header("References")]
    [SerializeField] private Transform firePoint;
    [SerializeField] private NetworkObject projectilePrefab;

    private EnemyContext ctx;

    private float nextAttackTime;

    private bool isAttacking;
    private Transform attackTarget;

    //================================================//
    // AWAKE
    //================================================//

    private void Awake()
    {
        ctx = GetComponentInParent<EnemyContext>();
    }

    //================================================//
    // UPDATE
    //================================================//

    private void Update()
    {
        if (!IsServer)
            return;

        if (isAttacking)
        {
            if (!IsValidTarget(attackTarget))
            {
                CancelAttack();
                return;
            }

            AimAtTarget();
            return;
        }

        if (Time.time < nextAttackTime)
            return;

        Transform target = ctx.targeting.CurrentTarget;

        if (!IsValidTarget(target))
            return;

        float distance = Vector3.Distance(
            transform.position,
            target.position
        );

        if (distance < minLaserRange ||
            distance > maxLaserRange)
            return;

        StartLaserAttack(target);
    }

    //================================================//
    // AIM
    //================================================//

    private void AimAtTarget()
    {
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

    //================================================//
    // START ATTACK
    //================================================//

    private void StartLaserAttack(Transform target)
    {
        isAttacking = true;
        attackTarget = target;

        nextAttackTime = Time.time + attackCooldown;

        ctx.movement.SetMovementLocked(true);

        if (ctx.enemyAnimation != null)
        {
            ctx.enemyAnimation.NotifyLaserAttack();
        }
        else
        {
            CancelAttack();
        }
    }

    //================================================//
    // ANIMATION EVENT - FIRE
    //================================================//

    public void AnimationEventFireLaser()
    {
        if (!IsServer || !isAttacking)
            return;

        if (!IsValidTarget(attackTarget) ||
            firePoint == null ||
            projectilePrefab == null)
        {
            CancelAttack();
            return;
        }

        Vector3 direction =
            attackTarget.position - firePoint.position;

        if (direction.sqrMagnitude < 0.001f)
        {
            CancelAttack();
            return;
        }

        SpawnProjectile(direction.normalized);

        CancelAttack();
    }

    //================================================//
    // SPAWN PROJECTILE
    //================================================//

    private void SpawnProjectile(Vector3 direction)
    {
        NetworkObject projectileObject = Instantiate(
            projectilePrefab,
            firePoint.position,
            Quaternion.LookRotation(direction)
        );

        Projectile projectile =
            projectileObject.GetComponent<Projectile>();

        if (projectile == null)
        {
            Destroy(projectileObject.gameObject);
            return;
        }

        projectile.Initialize(direction);

        projectileObject.Spawn();
    }

    //================================================//
    // CANCEL
    //================================================//

    private void CancelAttack()
    {
        isAttacking = false;
        attackTarget = null;

        ctx.movement.SetMovementLocked(false);
    }

    //================================================//
    // VALIDATION
    //================================================//

    private bool IsValidTarget(Transform target)
    {
        if (target == null)
            return false;

        if (!target.TryGetComponent<PlayerHealth>(
            out PlayerHealth health))
            return false;

        return !health.IsDead.Value;
    }

    //================================================//
    // GIZMOS
    //================================================//

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
