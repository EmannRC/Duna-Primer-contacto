using Unity.Netcode;
using UnityEngine;

public class ShootController : NetworkBehaviour
{
    [Header("Aim")]
    [SerializeField] private float aimDistance = 100f;
    [SerializeField] private LayerMask aimLayers = ~0;

    [Header("Audio")]
    [SerializeField] private AudioSource shootSound;

    private PlayerContext ctx;


    //========================================================//
    // AWAKE
    //========================================================//

    private void Awake()
    {
        ctx = GetComponentInParent<PlayerContext>();
    }


    //========================================================//
    // SHOOT
    //========================================================//

    public void Shoot()
    {
        if (!IsOwner || ctx == null)
            return;

        if (ctx.equipment.weapon == null)
            return;

        Transform firePoint = ctx.equipment.CurrentFirePoint;

        if (firePoint == null)
            return;

        Vector3 direction = GetDirection(firePoint);

        if (direction.sqrMagnitude < 0.001f)
            return;

        ShootServerRpc(direction);
    }


    //========================================================//
    // GET DIRECTION
    //========================================================//

    private Vector3 GetDirection(Transform firePoint)
    {
        Camera cam = Camera.main;

        if (cam == null || ctx.crosshair == null)
            return firePoint.forward;

        // Ray desde el centro de la mira.
        Ray ray = cam.ScreenPointToRay(
            ctx.crosshair.position
        );

        Vector3 targetPoint;

        // Buscamos exactamente qué está apuntando la mira.
        if (Physics.Raycast(
            ray,
            out RaycastHit hit,
            aimDistance,
            aimLayers,
            QueryTriggerInteraction.Ignore))
        {
            targetPoint = hit.point;
        }
        else
        {
            targetPoint = ray.origin + ray.direction * aimDistance;
        }

        // El proyectil sale del FirePoint
        // hacia el punto donde apunta la mira.
        return (targetPoint - firePoint.position).normalized;
    }


    //========================================================//
    // SERVER
    //========================================================//

    [ServerRpc]
    private void ShootServerRpc(Vector3 direction)
    {
        if (ctx == null)
            return;

        if (ctx.equipment.weapon == null)
            return;

        if (direction.sqrMagnitude < 0.001f)
            return;

        Transform firePoint = ctx.equipment.CurrentFirePoint;

        if (firePoint == null)
            return;

        // Consumir maná.
        if (!ctx.mana.TryUse(
            ctx.equipment.weapon.manaCost))
        {
            return;
        }

        // Crear proyectil.
        GameObject projectile = Instantiate(
            ctx.equipment.weapon.projectilePrefab,
            firePoint.position,
            Quaternion.LookRotation(direction)
        );

        Projectile projectileComponent =
            projectile.GetComponent<Projectile>();

        if (projectileComponent == null)
        {
            Destroy(projectile);
            return;
        }

        projectileComponent.Initialize(direction);

        NetworkObject networkObject =
            projectile.GetComponent<NetworkObject>();

        if (networkObject == null)
        {
            Destroy(projectile);
            return;
        }

        networkObject.Spawn();
    

        if (shootSound != null)
        {
            shootSound.Play();
        }
    }
}
