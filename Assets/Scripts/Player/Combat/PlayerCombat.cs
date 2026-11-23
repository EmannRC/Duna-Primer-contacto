using System.Collections;
using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerCombat : MonoBehaviour
{
    [Header("Shooting")]
    [SerializeField] private bool requireAimToShoot = true;

    private PlayerContext ctx;

    private bool shootInputHeld;
    private bool isAttacking;
    private bool projectileFired;

    private void Awake()
    {
        ctx = GetComponentInParent<PlayerContext>();
    }

    //========================================================//
    // INPUT
    //========================================================//

    public void SetShootInput(bool isPressed)
    {
        shootInputHeld = isPressed;
    }

    private void Update()
    {
        if (ctx == null)
            return;

        if (ctx.health.IsDead.Value)
        {
            shootInputHeld = false;
            return;
        }

        if (!shootInputHeld)
            return;

        TryShoot();
    }

    //========================================================//
    // TRY SHOOT
    //========================================================//

    private void TryShoot()
    {
        if (ctx == null)
            return;

        if (ctx.equipment.weapon == null)
            return;

        if (isAttacking)
            return;

        if (requireAimToShoot &&
            (ctx.targeting == null || !ctx.targeting.isAiming))
            return;

        float attackSpeed =
            ctx.stats.GetStat(StatType.AttackSpeed);

        if (attackSpeed <= 0f)
            return;

        isAttacking = true;
        projectileFired = false;

        // La velocidad de la animación controla
        // directamente la cadencia de disparo.
        ctx.playerAnimation.PlayShootAnimation(attackSpeed);
    }

    //========================================================//
    // ANIMATION EVENT - FIRE
    //========================================================//

    public void FireProjectile()
    {
        if (!isAttacking || projectileFired)
            return;

        if (ctx == null ||
            ctx.shooter == null ||
            ctx.equipment.weapon == null)
            return;

        projectileFired = true;

        ctx.shooter.Shoot();
    }

    //========================================================//
    // ANIMATION EVENT - END ATTACK
    //========================================================//

    public void EndAttack()
    {
        if (!isAttacking)
            return;

        isAttacking = false;

        if (ctx == null)
            return;

    }
}

