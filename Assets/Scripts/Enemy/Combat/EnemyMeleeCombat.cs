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
    private EnforcerSpecialAttack specialAttack;

    private bool isAttacking;
    private float nextAttackTime;

    //========================================================//
    // AWAKE
    //========================================================//

    private void Awake()
    {
        ctx =
            GetComponent<EnemyContext>();

        specialAttack =
            GetComponent<EnforcerSpecialAttack>();
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

        // No atacar durante el ataque especial.
        if (specialAttack != null &&
            specialAttack.IsPerformingSpecialAttack)
        {
            return;
        }

        Transform target =
            ctx.targeting.CurrentTarget;

        if (!IsValidTarget(target))
            return;

        if (Vector3.Distance(
                transform.position,
                target.position)
            > attackRange)
        {
            return;
        }

        StartAttack(target);
    }


    //========================================================//
    // START ATTACK
    //========================================================//

    private void StartAttack(
        Transform target)
    {
        isAttacking = true;

        nextAttackTime =
            Time.time + attackCooldown;

        bool hasAnimation =
            ctx.enemyAnimation != null &&
            ctx.animator != null;

        //====================================================//
        // ENEMIGO CON ANIMACIÓN
        //====================================================//

        if (hasAnimation)
        {
            ctx.enemyAnimation.NotifyAttack();

            return;
        }

        //====================================================//
        // ENEMIGO SIN ANIMACIÓN
        //====================================================//

        DealDamage(target);

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

        // Si empezó el ataque especial,
        // cancelamos el ataque normal.
        if (specialAttack != null &&
            specialAttack.IsPerformingSpecialAttack)
        {
            isAttacking = false;
            return;
        }

        Transform target =
            ctx.targeting.CurrentTarget;

        if (!IsValidTarget(target))
        {
            isAttacking = false;
            return;
        }

        DealDamage(target);

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

        if (!target.TryGetComponent(
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

        if (Vector3.Distance(
                transform.position,
                target.position)
            > attackRange)
        {
            return;
        }

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
            attackRange
        );
    }
}
