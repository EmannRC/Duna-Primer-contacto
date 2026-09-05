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

    public void Initialize(Vector3 direction)
    {
        this.direction = direction.normalized;

        if (IsServer)
            Invoke(nameof(Despawn), lifeTime);
    }

    private void FixedUpdate()
    {
        if (!IsServer)
            return;

        float distance = speed * Time.fixedDeltaTime;

        if (Physics.SphereCast(
            transform.position,
            radius,
            direction,
            out RaycastHit hit,
            distance,
            damageLayers))
        {
            if (hit.collider.TryGetComponent(out HealthController health))
                health.TakeDamage(damage);

            Despawn();
            return;
        }

        transform.position += direction * distance;
    }

    private void Despawn()
    {
        if (NetworkObject.IsSpawned)
            NetworkObject.Despawn();
    }

    private void OnDrawGizmos()
    {
        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(transform.position, radius);
    }
}
