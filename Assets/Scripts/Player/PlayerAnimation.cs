using UnityEngine;


public class PlayerAnimation : MonoBehaviour
{
    private Animator animator;

    private PlayerContext ctx;

    //============================================//
    void Awake()
    {
        animator = GetComponent<Animator>();

        ctx = GetComponentInParent<PlayerContext>();
    }

    void Update()
    {
        UpdateLocomotion();
        UpdateAirState();
        UpdateCrouch();
    }

    //============================================//
    void UpdateLocomotion()
    {
        //Vector3 move = ctx.movement.MoveDirection;

        //Vector3 localMove = ctx.transform.InverseTransformDirection(move);
        float velX = ctx.animationSync.VelX.Value;
        float velY = ctx.animationSync.VelY.Value;

        animator.SetFloat(
            "VelX",
            velX,
            0.15f,
            Time.deltaTime);

        animator.SetFloat(
            "VelY",
            velY,
            0.15f,
            Time.deltaTime);

        float speed =
            ctx.animationSync.Speed.Value;

        animator.SetFloat(
            "Speed",
            speed,
            0.1f,
            Time.deltaTime);

        animator.SetBool(
            "IsMoving",
            speed > 0.1f);
    }

    //============================================//
    void UpdateAirState()
    {
        bool grounded =
            ctx.animationSync.Grounded.Value;

        float vertical =
            ctx.animationSync.Vertical.Value;

        animator.SetBool(
            "IsGrounded",
            grounded);

        animator.SetFloat(
            "VelocityY",
            vertical);

        bool isJumping =
            !grounded &&
            vertical > 0;

        bool isFalling =
            !grounded &&
            vertical < 0;

        animator.SetBool(
            "IsJumping",
            isJumping);

        animator.SetBool(
            "IsFalling",
            isFalling);
    }

    //============================================//
    void UpdateCrouch()
    {
        animator.SetBool(
            "IsCrouching",
            ctx.animationSync.Crouching.Value);
    }

    //============================================//
    public void PlayShoot()
    {
        animator.SetTrigger("Shoot");
    }

    public void PlayDeath()
    {
        animator.SetTrigger("Death");
    }
}
