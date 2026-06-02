using UnityEngine;


public class PlayerAnimation : MonoBehaviour
{
    private Animator animator;

    private PlayerContext ctx;

    private int lastShootCounter;

    private bool deathPlayed;

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
        UpdateShoot();
        UpdateDeath();
    }

    //============================================//
    void UpdateLocomotion()
    {

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
    void UpdateShoot()
    {
        int currentShootCounter =
            ctx.animationSync.ShootCounter.Value;

        if (currentShootCounter == lastShootCounter)
            return;

        lastShootCounter = currentShootCounter;

        animator.SetTrigger("Shoot");
    }

    void UpdateDeath()
    {
        if (deathPlayed)
            return;

        if (ctx.health.IsDead.Value)
        {
            deathPlayed = true;
            animator.SetTrigger("Death");
        }
    }
}
