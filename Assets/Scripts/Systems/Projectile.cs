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
    [SerializeField] private LayerMask damageableLayer;
    //[SerializeField] private LayerMask resourceLayers;

    [Header("Effects")]
    [SerializeField] private GameObject destroyEffect;

    private Vector3 direction;

    //================================================//
    // INITIALIZE
    //================================================//

    public void Initialize(Vector3 direction)
    {
        this.direction = direction.normalized;

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

        Invoke(nameof(Despawn), lifeTime);
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

        float distance = speed * Time.fixedDeltaTime;

        if (Physics.SphereCast(
            transform.position,
            radius,
            direction,
            out RaycastHit hit,
            distance,
            damageableLayer,
            QueryTriggerInteraction.Ignore))
        {
            // Comprobar si impactó contra un recurso.
            FarmController resource =
                hit.collider.GetComponentInParent<FarmController>();

            if (resource != null)
            {
                resource.TryGather();
            }
            else
            {
                // Comprobar si impactó contra un enemigo.
                HealthController health =
                    hit.collider.GetComponentInParent<HealthController>();

                if (health != null)
                {
                    health.TakeDamage(damage);
                }
            }

            SpawnDestroyEffectClientRpc(
                hit.point,
                Quaternion.LookRotation(hit.normal)
            );

            Despawn();
            return;
        }

        // Movimiento del proyectil.
        transform.position += direction * distance;

        transform.rotation = Quaternion.LookRotation(direction);
    }

    //================================================//
    // EFFECT
    //================================================//

    [ClientRpc]
    private void SpawnDestroyEffectClientRpc(
        Vector3 position,
        Quaternion rotation)
    {
        if (destroyEffect == null)
            return;

        GameObject effect = Instantiate(
            destroyEffect,
            position,
            rotation
        );

        Destroy(effect, 3f);
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
