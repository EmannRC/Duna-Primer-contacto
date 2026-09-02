using Unity.Netcode;
using UnityEngine;

public class EnemyMeleeCombat : NetworkBehaviour
{
    [SerializeField] private int damage = 10;
    [SerializeField] private float attackCooldown = 1f;
    [SerializeField] private float attackRange = 1.5f;

    private float nextAttackTime;
    private EnemyContext ctx;

    private void Awake()
    {
        ctx = GetComponent<EnemyContext>();
    }

    private void Update()
    {
        if (!IsServer)
            return;

        Transform target = ctx.targeting.CurrentTarget;

        if (target == null || Time.time < nextAttackTime)
            return;

        if (Vector3.Distance(transform.position, target.position) > attackRange)
            return;

        if (target.TryGetComponent<PlayerHealth>(out var health))
        {
            health.TakeDamage(damage);
            nextAttackTime = Time.time + attackCooldown;
        }
    }

    private void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(transform.position, attackRange);
    }
}
