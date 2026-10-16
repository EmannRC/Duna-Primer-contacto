using Unity.Netcode;
using UnityEngine;
using System.Collections;

public class EnemyMeleeCombat : NetworkBehaviour
{
    [Header("Combat")]
    [SerializeField] private int damage = 10;
    [SerializeField] private float attackRange = 1.5f;
    [SerializeField] private float attackCooldown = 1f;
    [SerializeField, Min(0.1f)] private float attackFailsafeDuration = 1.1f;

    private EnemyContext ctx;

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
                target.position
            );

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

        // BLOQUEAR MOVIMIENTO
        ctx.movement.SetMovementLocked(true);

        if (ctx.enemyAnimation != null)
        {
            ctx.enemyAnimation.NotifyAttack();
            // Fallback for clips without an AnimationEvent.
            StartCoroutine(FinishAttackAfterAnimation());
            return;
        }

        // Si no existe animación,
        // terminamos inmediatamente.
        FinishAttack();
    }


    //========================================================//
    private IEnumerator FinishAttackAfterAnimation()
    {
        yield return new WaitForSeconds(attackFailsafeDuration);

        if (!isAttacking)
            yield break;

        Transform target = ctx.targeting != null ? ctx.targeting.CurrentTarget : null;
        if (IsValidTarget(target))
            DealDamage(target);

        FinishAttack();
    }
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

        FinishAttack();
    }


    //========================================================//
    // FINISH ATTACK
    //========================================================//

    private void FinishAttack()
    {
        isAttacking = false;

        // VOLVER A PERMITIR MOVIMIENTO
        ctx.movement.SetMovementLocked(false);
    }


    //========================================================//
    // TARGET
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
    // DAMAGE
    //========================================================//

    private void DealDamage(Transform target)
    {
        if (!IsServer)
            return;

        if (!IsValidTarget(target))
            return;

        float distance =
            Vector3.Distance(
                transform.position,
                target.position
            );

        if (distance > attackRange)
            return;

        PlayerHealth health =
            target.GetComponent<PlayerHealth>();

        if (health != null)
        {
            health.TakeDamage(damage);
        }
    }


    //========================================================//
    // CANCEL
    //========================================================//

    public void CancelAttack()
    {
        isAttacking = false;

        ctx.movement.SetMovementLocked(false);
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
