using System.Collections;
using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerCombat : MonoBehaviour
{
    public float shootDelay = 0.5f;
   
    private float nextShootTime;

    private PlayerContext ctx;

    //========================================================//
    void Awake()
    {
        ctx = GetComponentInParent<PlayerContext>();
    }

    //========================================================//
    public void OnShoot(InputAction.CallbackContext context)
    {
        if (!context.performed)
            return;

        if (ctx.equipment.weapon == null)
            return;

        if (Time.time < nextShootTime)
            return;

        float attackSpeed = ctx.stats.GetStat(StatType.AttackSpeed);

        float cooldown = 1f / attackSpeed;

        nextShootTime = Time.time + cooldown;

        ctx.animationSync.NotifyShoot();

        StartCoroutine(ShootRoutine());
    }

    //========================================================//
    IEnumerator ShootRoutine()
    {
        ctx.movement.IsMovementLocked = true;

        yield return new WaitForSeconds(shootDelay);

        Vector3 direction = GetShootDirection();

        ctx.rotation.LookAt(direction);

        ctx.shooter.Shoot(direction);

        ctx.movement.IsMovementLocked = false;

        yield return new WaitForSeconds(0.1f);

        ctx.rotation.StopAttackRotation();
    }

    //========================================================//
    Vector3 GetShootDirection()
    {
        Ray ray = Camera.main.ScreenPointToRay(ctx.crosshair.position);

        if (Physics.Raycast(ray, out RaycastHit hit))
        {
            return (hit.point - ctx.shooter.firePoint.position).normalized;
        }

        return ray.direction;
    }
}

