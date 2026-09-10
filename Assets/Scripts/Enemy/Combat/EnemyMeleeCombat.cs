using Unity.Netcode;
using UnityEngine;
using System.Collections;

public class EnemyMeleeCombat : NetworkBehaviour
{
    [Header("Combat")]
    [SerializeField] private int damage = 10;
    [SerializeField] private float attackRange = 1.5f;
    [SerializeField] private float attackCooldown = 1f;

    private EnemyContext ctx;
    //private EnforcerSpecialAttack specialAttack;

    private bool isAttacking;
    private float nextAttackTime;

    //========================================================//
    // AWAKE
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

        if (isAttacking)
            return;

        if (Time.time < nextAttackTime)
            return;

        Transform target =
            ctx.targeting.CurrentTarget;

        if (!IsValidTarget(target))
            return;

        float distance =
            Vector3.Distance(
                transform.position,
                target.position);

        if (distance > attackRange)
            return;

        StartAttack();
    }


    //========================================================//
    // START ATTACK
    //========================================================//

    private void StartAttack()
    {
        isAttacking = true;

        nextAttackTime =
            Time.time + attackCooldown;

        // Si tiene animación,
        // esperamos al Animation Event.
        if (ctx.enemyAnimation != null)
        {
            ctx.enemyAnimation.NotifyAttack();
            return;
        }

        // Si no tiene animación,
        // hacemos el daño inmediatamente.
        DealDamage(ctx.targeting.CurrentTarget);

        isAttacking = false;
    }


    //========================================================//
    // ANIMATION EVENT
    //========================================================//

    public void AnimationEventDealDamage()
    {
        if (!IsServer)
            return;

        if (!isAttacking)
            return;

        Transform target =
            ctx.targeting.CurrentTarget;

        if (IsValidTarget(target))
        {
            DealDamage(target);
        }

        isAttacking = false;
    }


    //========================================================//
    // TARGET
    //========================================================//

    private bool IsValidTarget(
        Transform target)
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
    // DAMAGE
    //========================================================//

    private void DealDamage(
        Transform target)
    {
        if (!IsServer)
            return;

        if (!IsValidTarget(target))
            return;

        float distance =
            Vector3.Distance(
                transform.position,
                target.position);

        if (distance > attackRange)
            return;

        PlayerHealth health =
            target.GetComponent<PlayerHealth>();

        health.TakeDamage(damage);
    }


    //========================================================//
    // CANCEL ATTACK
    //========================================================//

    public void CancelAttack()
    {
        isAttacking = false;
    }


    //========================================================//
    // GIZMOS
    //========================================================//

    private void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.red;

        Gizmos.DrawWireSphere(
            transform.position,
            attackRange);
    }
}
