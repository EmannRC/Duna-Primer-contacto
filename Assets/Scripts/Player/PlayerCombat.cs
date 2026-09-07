using System.Collections;
using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerCombat : MonoBehaviour
{
    [Header("Shooting")]
    [Tooltip("Mantiene el disparo ligado al modo de apuntado.")]
    [SerializeField] private bool requireAimToShoot = true;

    private float nextShootTime;
    private bool isAttacking;

    private PlayerContext ctx;


    //========================================================//
    // AWAKE
    //========================================================//

    private void Awake()
    {
        ctx = GetComponentInParent<PlayerContext>();
    }


    //========================================================//
    // TRY SHOOT
    //========================================================//

    public void TryShoot()
    {
        if (ctx == null)
            return;

        if (ctx.equipment.weapon == null)
            return;

        // No permitir otro ataque mientras
        // la animación actual está ejecutándose.
        if (isAttacking)
            return;

        //====================================================//
        // AIM
        //====================================================//

        if (requireAimToShoot &&
            (ctx.targeting == null ||
             !ctx.targeting.isAiming))
        {
            return;
        }

        //====================================================//
        // ATTACK SPEED
        //====================================================//

        float attackSpeed =
            ctx.stats.GetStat(StatType.AttackSpeed);

        if (attackSpeed <= 0f)
            return;

        float cooldown =
            1f / attackSpeed;

        if (Time.time < nextShootTime)
            return;

        nextShootTime =
            Time.time + cooldown;

        //====================================================//
        // START ATTACK
        //====================================================//

        isAttacking = true;

        if (ctx.movement != null)
        {
            //ctx.movement.IsMovementLocked = true;
        }

        Debug.Log("DISPARO: NotifyShoot llamado");

        ctx.animationSync.NotifyShoot();
    }


    //========================================================//
    // ANIMATION EVENT - FIRE
    //========================================================//

    public void FireProjectile()
    {
        if (ctx == null)
            return;

        if (ctx.equipment.weapon == null)
            return;

        if (ctx.shooter == null)
        {
            Debug.LogError(
                "PlayerCombat: No se encontró ShootController."
            );

            return;
        }

        // Este método es llamado directamente
        // desde un Animation Event.
        ctx.shooter.Shoot();
    }


    //========================================================//
    // ANIMATION EVENT - END ATTACK
    //========================================================//

    public void EndAttack()
    {
        isAttacking = false;

        if (ctx == null)
            return;

        // Liberar movimiento.
        if (ctx.movement != null)
        {
            ctx.movement.IsMovementLocked = false;
        }

        // Finalizar rotación de ataque.
        if (ctx.rotation != null)
        {
            ctx.rotation.StopAttackRotation();
        }
    }
}

