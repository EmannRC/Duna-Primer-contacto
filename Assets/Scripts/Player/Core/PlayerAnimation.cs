using System.Globalization;
using Unity.Netcode;
using UnityEngine;


public class PlayerAnimation : NetworkBehaviour
{
    private PlayerContext ctx;

    private bool deathPlayed;


    // Hashes para evitar strings constantemente
    private static readonly int VelXHash =
        Animator.StringToHash("VelX");

    private static readonly int VelYHash =
        Animator.StringToHash("VelY");

    private static readonly int SpeedHash =
        Animator.StringToHash("Speed");

    private static readonly int IsMovingHash =
        Animator.StringToHash("IsMoving");

    private static readonly int IsGroundedHash =
        Animator.StringToHash("IsGrounded");

    private static readonly int VelocityYHash =
        Animator.StringToHash("VelocityY");

    private static readonly int IsJumpingHash =
        Animator.StringToHash("IsJumping");

    private static readonly int IsFallingHash =
        Animator.StringToHash("IsFalling");

    private static readonly int IsCrouchingHash =
        Animator.StringToHash("IsCrouching");

    private static readonly int ShootHash =
        Animator.StringToHash("Shoot");

    private static readonly int AttackAnimationSpeedHash =
    Animator.StringToHash("AttackAnimationSpeed");

    private static readonly int DeathHash =
        Animator.StringToHash("Death");


    //========================================================//

    void Awake()
    {
        
        ctx = GetComponentInParent<PlayerContext>();
    }

    //========================================================//

    void Update()
    {
        if (!IsOwner)
            return;

        UpdateLocomotion();
        UpdateAirState();
        UpdateCrouch();
    }

    //========================================================//

    private void UpdateLocomotion()
    {
        Vector3 move = ctx.movement.MoveDirection;

        Vector3 localMove =
            ctx.transform.InverseTransformDirection(move);

        float velX = localMove.x;
        float velY = localMove.z;

        ctx.animator.SetFloat(
            VelXHash,
            velX,
            0.15f,
            Time.deltaTime);

        ctx.animator.SetFloat(
            VelYHash,
            velY,
            0.15f,
            Time.deltaTime);


        float speed =
            ctx.movement.AnimationSpeed;

        ctx.animator.SetFloat(
            SpeedHash,
            speed,
            0.1f,
            Time.deltaTime);


        bool isMoving =
            move.sqrMagnitude > 0.01f;

        ctx.animator.SetBool(
            IsMovingHash,
            isMoving);
    }

    //========================================================//

    void UpdateAirState()
    {
        bool grounded =
            ctx.movement.IsGrounded;

        float vertical =
            ctx.movement.VerticalVelocity;

        ctx.animator.SetBool(
            IsGroundedHash,
            grounded);

        ctx.animator.SetFloat(
            VelocityYHash,
            vertical);

        ctx.animator.SetBool(
            IsJumpingHash,
            !grounded && vertical > 0);

        ctx.animator.SetBool(
            IsFallingHash,
            !grounded && vertical < 0);
    }

    //========================================================//

    void UpdateCrouch()
    {
        ctx.animator.SetBool(
            IsCrouchingHash,
            ctx.movement.IsCrouching);
    }

    //========================================================//
    // EVENTS
    //========================================================//

    public void PlayShootAnimation()
    {
        if (!IsOwner)
            return;

        float attackSpeed =
            ctx.stats.GetStat(StatType.AttackSpeed);

        if (attackSpeed <= 0f)
            attackSpeed = 0.1f;

        ctx.animator.SetFloat(
            AttackAnimationSpeedHash,
            attackSpeed
        );

        ctx.animator.SetTrigger(ShootHash);
    }

    public void PlayDeathAnimation()
    {
        if (deathPlayed)
            return;

        deathPlayed = true;

        ctx.animator.SetTrigger(DeathHash);
    }

    public void ResetDeathAnimation()
    {
        if (!IsOwner)
            return;

        deathPlayed = false;

        ctx.animator.ResetTrigger(DeathHash);
    }
}
