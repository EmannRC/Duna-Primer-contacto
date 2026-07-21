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

        TryAttack();
    }

    //================================================//

    private void TryAttack()
    {
        Transform target = ctx.targeting.CurrentTarget;

        if (target == null)
            return;

        float distance = Vector3.Distance(
            transform.position,
            target.position);

        if (distance > attackRange)
            return;

        if (Time.time < nextShotTime)
            return;

        nextShotTime =
            Time.time + (1f / fireRate);

        Shoot(leftCannon);

        if (rightCannon != null)
            Shoot(rightCannon);
    }

    private void Shoot(Transform cannon)
    {
        if (cannon == null)
            return;

        NetworkObject projectileObject =
            Instantiate(
                projectilePrefab,
                cannon.position,
                cannon.rotation);

        Projectile projectile =
            projectileObject.GetComponent<Projectile>();

        Vector3 direction =
            (ctx.targeting.CurrentTarget.position
            - cannon.position).normalized;

        projectile.Initialize(direction);

        projectileObject.Spawn();
    }
}
