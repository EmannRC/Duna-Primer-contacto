using System;
using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;

public class EnemyMovement : NetworkBehaviour
{
    [SerializeField] private float moveSpeed = 5f;
    [SerializeField] private float orbitDistance = 3f;
    [SerializeField] private float rotationSpeed = 8f;

    private EnemyContext ctx;

    //==============================================================================================================//
    private void Awake()
    {
        ctx = GetComponent<EnemyContext>();
    }

    //==============================================================================================================//
    private void Update()
    {
        if (!IsServer)
            return;

        Move();
        RotateTowardsTarget();
    }

    //==============================================================================================================//
    private void Move()
    {
        if (ctx.targeting.CurrentTarget == null)
            return;

        Vector3 desiredPosition = GetOrbitPosition();

        Vector3 direction = desiredPosition - transform.position;

        if (direction.sqrMagnitude < 0.04f)
            return;

        transform.position += direction.normalized * moveSpeed * Time.deltaTime;
    }
    //==============================================================================================================//
    private Vector3 GetOrbitPosition()
    {
        int enemyCount =
            EnemyFormation.ActiveEnemies.Count;

        if (enemyCount == 0) return ctx.targeting.CurrentTarget.position;

        float angleStep = 360f / enemyCount;

        float angle = ctx.formation.SlotIndex * angleStep;

        Vector3 offset = Quaternion.Euler(0f, angle, 0f) * Vector3.forward * orbitDistance;

        return ctx.targeting.CurrentTarget.position + offset;
    }

    //==============================================================================================================//
    private void RotateTowardsTarget()
    {
        if (ctx.targeting.CurrentTarget == null)
            return;

        Vector3 dir = ctx.targeting.CurrentTarget.position - transform.position;

        dir.y = 0f;

        if (dir.sqrMagnitude < 0.01f)
            return;

        Quaternion targetRotation = Quaternion.LookRotation(dir);

        transform.rotation = Quaternion.Slerp(transform.rotation, targetRotation, rotationSpeed * Time.deltaTime);
    }

    //==============================================================================================================//
    private void OnDrawGizmosSelected()
    {
        if (ctx == null ||
            ctx.targeting == null ||
            ctx.targeting.CurrentTarget == null)
            return;

        Vector3 targetPos =
            ctx.targeting.CurrentTarget.position;

        Vector3 orbitPos =
            GetOrbitPosition();

        // Radio de órbita
        Gizmos.color = Color.cyan;
        Gizmos.DrawWireSphere(targetPos, orbitDistance);

        // Posición asignada
        Gizmos.color = Color.green;
        Gizmos.DrawWireSphere(orbitPos, 0.3f);

        // Trayectoria hacia el slot
        Gizmos.color = Color.yellow;
        Gizmos.DrawLine(transform.position, orbitPos);

        // Línea al objetivo
        Gizmos.color = Color.red;
        Gizmos.DrawLine(transform.position, targetPos);
    }
}
