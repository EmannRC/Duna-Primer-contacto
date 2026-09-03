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
    private bool isAttacking;

    private void Awake()
    {
        ctx = GetComponent<EnemyContext>();
    }

    private void Update()
    {
        if (!IsServer || isAttacking)
            return;

        Transform target = ctx.targeting.CurrentTarget;

        if (target == null)
            return;

        if (Vector3.Distance(transform.position, target.position) > attackRange)
            return;

        StartCoroutine(AttackRoutine(target));
    }

    private IEnumerator AttackRoutine(Transform target)
    {
        isAttacking = true;

        // Inicia la animación sincronizada
        ctx.animationSync.NotifyAttack();

        // Esperamos un frame para que el Animator
        // entre en el estado Attack
        yield return null;

        // Esperamos hasta que la animación termine
        while (true)
        {
            AnimatorStateInfo stateInfo = ctx.animator.GetCurrentAnimatorStateInfo(0);

            if (stateInfo.IsName(attackStateName) && stateInfo.normalizedTime >= animationWaitTime)
            {
                break;
            }

            yield return null;
        }

        // Cuando la snimacion termine, ataca.
        DealDamage(target);

        isAttacking = false;
    }

    private void DealDamage(Transform target)
    {
        if (target == null)
            return;

        if (Vector3.Distance(transform.position, target.position) > attackRange)
            return;

        if (target.TryGetComponent(out PlayerHealth health))
        {
            health.TakeDamage(damage);
        }
    }

    private void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(transform.position, attackRange);
    }
}
