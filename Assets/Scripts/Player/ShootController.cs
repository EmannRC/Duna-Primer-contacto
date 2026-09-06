using Unity.Netcode;
using UnityEngine;

public class ShootController : NetworkBehaviour
{
   //public Transform firePoint;
    public float cooldown = 1f;
    public AudioSource shootSound;

    private float lastShotTime;
    private PlayerContext ctx;

    //=======================================================//
    void Awake()
    {
        ctx = GetComponentInParent<PlayerContext>();
    }

    //=======================================================//
    public void Shoot(Vector3 direction)
    {
        if (!IsOwner)
            return;

        ShootServerRpc(direction);
    }

    //=======================================================//
    [ServerRpc]
    private void ShootServerRpc(Vector3 direction)
    {
        if (ctx.equipment.weapon == null)
            return;

        Transform firePoint = ctx.equipment.CurrentFirePoint;

        if (firePoint == null)
        {
            Debug.LogError("No se encontró el FirePoint del arma equipada.");
            return;
        }

        float attackSpeed =
            ctx.stats.GetStat(StatType.AttackSpeed);

        float cooldown =
            1f / attackSpeed;

        if (Time.time < lastShotTime + cooldown)
            return;

        if (!ctx.mana.TryUse(
                ctx.equipment.weapon.manaCost))
            return;

        lastShotTime = Time.time;

        GameObject projectile =
            Instantiate(
                ctx.equipment.weapon.projectilePrefab,
                firePoint.position,
                Quaternion.LookRotation(direction)
            );

        projectile
            .GetComponent<Projectile>()
            .Initialize(direction);

        projectile
            .GetComponent<NetworkObject>()
            .Spawn();

        if (shootSound != null)
            shootSound.Play();
    }
}
