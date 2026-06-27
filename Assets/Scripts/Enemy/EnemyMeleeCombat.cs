using Unity.Netcode;
using UnityEngine;

public class EnemyMeleeCombat : NetworkBehaviour
{
    [SerializeField] private int damage = 10;
    [SerializeField] private float attackRate = 1f;
    [SerializeField] private float attackRange = 1.5f;

    private float nextAttackTime;

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

        if (Time.time < nextAttackTime)
            return;

        nextAttackTime =
            Time.time + (1f / attackRate);

        Attack(target);
    }

    private void Attack(Transform target)
    {
        PlayerHealth health =
            target.GetComponentInParent<PlayerHealth>();

        if (health == null)
            return;

        health.TakeDamage(damage);
    }
    private void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.red;

        Gizmos.DrawWireSphere(
            transform.position,
            attackRange);
    }
}
