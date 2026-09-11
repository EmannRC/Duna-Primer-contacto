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

    private static readonly int LaserAttackHash =
        Animator.StringToHash("LaserAttack");

    private static readonly int DeathHash =
        Animator.StringToHash("Death");


    private void Awake()
    {
        ctx = GetComponentInParent<EnemyContext>();
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


    public void NotifyLaserAttack()
    {
        if (!IsServer)
            return;

        ctx.animator.SetTrigger(LaserAttackHash);
    }


    public void PlayDeathAnimation()
    {
        if (!IsServer)
            return;

        if (ctx.animator == null)
            return;

        ctx.animator.SetTrigger(
            DeathHash
        );
    }

    public void AnimationEventDealDamage()
    {
        ctx.meleeCombat.AnimationEventDealDamage();
    }

    public void AnimationEventFireLaser()
    {
        if (ctx.laserAttack == null)
            return;

        ctx.laserAttack.AnimationEventFireLaser();
    }
}
