using System;
using Unity.Netcode;
using UnityEngine;

public class EnemyCombat : NetworkBehaviour
{
    [SerializeField] private NetworkObject projectilePrefab;

    [SerializeField] private Transform leftCannon;
    [SerializeField] private Transform rightCannon;

    [SerializeField] private float fireRate = 1f;
    [SerializeField] private float attackRange = 8f;

    private float nextShotTime;

    private EnemyContext ctx;

    //===================================================================================================//
    private void Awake()
    {
        ctx = GetComponent<EnemyContext>();
    }


    //===================================================================================================//
    private void Update()
    {
        if (!IsServer)
            return;

        TryShoot();
    }


    //===================================================================================================//
    private void TryShoot()
    {
        if (ctx.targeting.CurrentTarget == null)
            return;

        float distance = Vector3.Distance(transform.position, ctx.targeting.CurrentTarget.position);

        if (distance > attackRange)
            return;

        if (Time.time < nextShotTime)
            return;

        nextShotTime = Time.time + (1f / fireRate);

        Shoot(leftCannon);
        Shoot(rightCannon);
    }


    //===================================================================================================//
    private void Shoot(Transform cannon)
    {
        NetworkObject projectileObject =
            Instantiate(
                projectilePrefab,
                cannon.position,
                cannon.rotation);

        Projectile projectile =
            projectileObject.GetComponent<Projectile>();

        Vector3 direction =
            (ctx.targeting.CurrentTarget.position -
             cannon.position).normalized;

        projectile.Initialize(direction);

        projectileObject.Spawn();
    }

    //===================================================================================================//
    private void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.red;

        Gizmos.DrawWireSphere(transform.position, attackRange);
    }
}
