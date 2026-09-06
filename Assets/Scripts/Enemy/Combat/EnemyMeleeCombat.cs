using Unity.Netcode;
using UnityEngine;
using System.Collections;

public class EnemyMeleeCombat : NetworkBehaviour
{
    [Header("Combat")]
    [SerializeField] private int damage = 10;
    [SerializeField] private float attackRange = 1.5f;

    [Header("Animation")]
    [SerializeField] private Animator animator;
    [SerializeField] private string attackStateName = "Attack";
    [SerializeField] private float animationWaitTime = 0.5f;

    private EnemyContext ctx;
    private EnforcerSpecialAttack specialAttack;

    private bool isAttacking;

    private void Awake()
    {
        ctx = GetComponent<EnemyContext>();

        specialAttack =
            GetComponent<EnforcerSpecialAttack>();
    }

    private void Update()
    {
        if (!IsServer || isAttacking)
            return;

        // No atacar normalmente durante el especial
        if (specialAttack != null &&
            specialAttack.IsPerformingSpecialAttack)
        {
            return;
        }

        Transform target =
            ctx.targeting.CurrentTarget;

        if (target == null)
            return;

        if (Vector3.Distance(
                transform.position,
                target.position)
            > attackRange)
        {
            return;
        }

        StartCoroutine(AttackRoutine(target));
    }

    private IEnumerator AttackRoutine(
        Transform target)
    {
        isAttacking = true;

        // Animación
        if (ctx.animationSync != null &&
            ctx.animator != null)
        {
            ctx.animationSync.NotifyAttack();

            yield return null;

            // Esperar hasta el momento del golpe
            float timeout = 3f;
            float timer = 0f;

            while (timer < timeout)
            {
                // Si empieza el especial,
                // cancelamos el ataque normal
                if (specialAttack != null &&
                    specialAttack.IsPerformingSpecialAttack)
                {
                    isAttacking = false;
                    yield break;
                }

                AnimatorStateInfo stateInfo =
                    ctx.animator
                        .GetCurrentAnimatorStateInfo(0);

                if (stateInfo.IsName(attackStateName) &&
                    stateInfo.normalizedTime >=
                    animationWaitTime)
                {
                    break;
                }

                timer += Time.deltaTime;

                yield return null;
            }
        }

        DealDamage(target);

        isAttacking = false;
    }

    private void DealDamage(Transform target)
    {
        if (target == null)
            return;

        // Si empezó el especial no hacemos daño melee
        if (specialAttack != null &&
            specialAttack.IsPerformingSpecialAttack)
        {
            return;
        }

        if (Vector3.Distance(
                transform.position,
                target.position)
            > attackRange)
        {
            return;
        }

        if (target.TryGetComponent(
                out PlayerHealth health))
        {
            health.TakeDamage(damage);
        }
    }

    private void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.red;

        Gizmos.DrawWireSphere(
            transform.position,
            attackRange);
    }
}
