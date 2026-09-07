using Unity.Netcode;
using UnityEditor;
using UnityEngine;

public class Projectile : NetworkBehaviour
{
    [Header("Projectile")]
    [SerializeField] protected float speed = 10f;
    [SerializeField] protected float lifeTime = 3f;
    [SerializeField] protected int damage = 10;
    [SerializeField] protected float radius = 0.2f;

    [Header("Damage")]
    [SerializeField] private LayerMask damageLayers;

    private Vector3 direction;

    //================================================//
    // INITIALIZE
    //================================================//

    public void Initialize(Vector3 direction)
    {
        this.direction = direction.normalized;

        // Orientar el proyectil hacia su dirección de movimiento.
        if (this.direction.sqrMagnitude > 0.001f)
        {
            transform.rotation =
                Quaternion.LookRotation(this.direction);
        }
    }

    //================================================//
    // NETWORK SPAWN
    //================================================//

    public override void OnNetworkSpawn()
    {
        if (!IsServer)
            return;

        Invoke(
            nameof(Despawn),
            lifeTime
        );
    }

    //================================================//
    // FIXED UPDATE
    //================================================//

    private void FixedUpdate()
    {
        if (!IsServer)
            return;

        if (direction.sqrMagnitude < 0.001f)
            return;

        float distance =
            speed * Time.fixedDeltaTime;

        // Detectar impacto antes de mover.
        if (Physics.SphereCast(
            transform.position,
            radius,
            direction,
            out RaycastHit hit,
            distance,
            damageLayers,
            QueryTriggerInteraction.Ignore))
        {
            if (hit.collider.TryGetComponent(
                out HealthController health))
            {
                health.TakeDamage(damage);
            }

            Despawn();
            return;
        }

        // Movimiento recto.
        transform.position +=
            direction * distance;
    }

    //================================================//
    // DESPAWN
    //================================================//

    private void Despawn()
    {
        if (!IsServer)
            return;

        if (NetworkObject.IsSpawned)
        {
            NetworkObject.Despawn();
        }
    }

    //================================================//
    // NETWORK DESPAWN
    //================================================//

    public override void OnNetworkDespawn()
    {
        CancelInvoke(nameof(Despawn));
    }

    //================================================//
    // GIZMOS
    //================================================//

    private void OnDrawGizmos()
    {
        Gizmos.color = Color.red;

        Gizmos.DrawWireSphere(
            transform.position,
            radius
        );

        if (Application.isPlaying &&
            direction.sqrMagnitude > 0.001f)
        {
            Gizmos.color = Color.yellow;

            Gizmos.DrawRay(
                transform.position,
                direction * 2f
            );
        }
    }
}
