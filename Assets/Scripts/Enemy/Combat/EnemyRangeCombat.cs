using System;
using Unity.Netcode;
using UnityEngine;

public class EnemyRangeCombat : NetworkBehaviour
{
    [Header("Attack Settings")]
    [SerializeField] private float fireRate = 1f;
    [SerializeField] private float attackRange = 8f;

    [Header("References")]
    [SerializeField] private NetworkObject projectilePrefab;
    [SerializeField] private Transform leftCannon;
    [SerializeField] private Transform rightCannon;

    private float nextShotTime;
    private bool useLeftCannon = true;

    private EnemyContext ctx;

    //================================================//

    private void Awake()
    {
        ctx = GetComponent<EnemyContext>();
    }

    private void Update()
    {
        if (!IsServer)
            return;

        if (ctx.health != null && ctx.health.IsDead.Value)
            return;

        TryAttack();
    }

    //================================================//

    private void TryAttack()
    {
        if (ctx == null || ctx.targeting == null)
            return;

        Transform target = ctx.targeting.CurrentTarget;

        if (target == null)
            return;

        float distance = Vector3.Distance(
            transform.position,
            target.position
        );

        if (distance > attackRange)
            return;

        if (Time.time < nextShotTime)
            return;

        nextShotTime = Time.time + (1f / fireRate);

        Transform cannon = GetNextCannon();

        if (cannon == null)
            return;

        Shoot(cannon, target);
    }

    //================================================//

    private Transform GetNextCannon()
    {
        Transform cannon = useLeftCannon
            ? leftCannon
            : rightCannon;

        // Alterna entre los dos cañones.
        useLeftCannon = !useLeftCannon;

        // Si uno no está asignado, utiliza el otro.
        if (cannon == null)
            cannon = leftCannon != null ? leftCannon : rightCannon;

        return cannon;
    }

    private void Shoot(Transform cannon, Transform target)
    {
        if (cannon == null || target == null || projectilePrefab == null)
            return;

        Vector3 direction =
            (target.position - cannon.position).normalized;

        NetworkObject projectileObject = Instantiate(
            projectilePrefab,
            cannon.position,
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

    private void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.red;

        Gizmos.DrawWireSphere(transform.position, attackRange);
    }
}
