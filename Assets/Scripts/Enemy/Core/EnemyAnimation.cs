using Unity.Netcode;
using UnityEngine;

public class EnemyAnimation : NetworkBehaviour
{
    private EnemyContext ctx;

    private static readonly int VelXHash =
        Animator.StringToHash("VelX");

    private static readonly int VelYHash =
        Animator.StringToHash("VelY");

    private static readonly int AttackHash =
        Animator.StringToHash("Attack");

    private static readonly int SpecialAttackHash =
        Animator.StringToHash("JumpAttack");

    private static readonly int DeadHash =
        Animator.StringToHash("Death");


    private void Awake()
    {
        ctx = GetComponent<EnemyContext>();
    }


    private void Update()
    {
        if (!IsServer)
            return;

        UpdateMovementAnimation();
    }


    private void UpdateMovementAnimation()
    {
        if (ctx.animator == null || ctx.movement == null)
            return;

        Vector3 velocity = ctx.groundMovement.Velocity;

        Vector3 localVelocity =
            transform.InverseTransformDirection(velocity);

        ctx.animator.SetFloat(
            VelXHash,
            localVelocity.x
        );

        ctx.animator.SetFloat(
            VelYHash,
            localVelocity.z
        );
    }


    public void NotifyAttack()
    {
        if (!IsServer)
            return;

        ctx.animator.SetTrigger(AttackHash);
    }


    public void NotifySpecialAttack()
    {
        if (!IsServer)
            return;

        ctx.animator.SetTrigger(SpecialAttackHash);
    }


    public void PlayDeathAnimation()
    {
        if (!IsServer)
            return;

        ctx.animator.SetBool(DeadHash, true);
    }
}
