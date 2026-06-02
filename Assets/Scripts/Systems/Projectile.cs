using Unity.Netcode;
using UnityEditor;
using UnityEngine;

public class Projectile : NetworkBehaviour
{
    [SerializeField] protected float speed = 10f;
    [SerializeField] protected float lifeTime = 3f;
    [SerializeField] protected int damage = 10;
    [SerializeField] protected float radius = 0.2f;

    protected Rigidbody rb;
    protected Vector3 direction;

    //======================================================//
    protected virtual void Awake()
    {
        rb = GetComponent<Rigidbody>();
    }

    //======================================================//
    public void Initialize(Vector3 dir)
    {
        direction = dir.normalized;

        if (IsServer)
            Invoke(nameof(Despawn), lifeTime);
    }

    private void FixedUpdate()
    {
        if (!IsServer) return;

        Vector3 move = direction * speed * Time.fixedDeltaTime;

        if (Physics.SphereCast(transform.position, radius, direction, out RaycastHit hit, move.magnitude))
        {
            if (hit.collider.TryGetComponent(out HealthController health))
            {
                health.TakeDamage(damage);
            }

            Despawn();
            return;
        }

        transform.position += move;
    }

    private void Despawn()
    {
        if (!IsServer) return;

        if (NetworkObject.IsSpawned)
            NetworkObject.Despawn();
    }

    private void OnDrawGizmos()
    {
        Gizmos.color = Color.red;

        Vector3 dir = Application.isPlaying ? direction : transform.forward;
        float step = speed * Time.fixedDeltaTime;

        Vector3 start = transform.position;
        Vector3 end = start + dir * step;

        Gizmos.DrawWireSphere(start, radius);
        Gizmos.DrawWireSphere(end, radius);
        Gizmos.DrawLine(start, end);
    }
}
