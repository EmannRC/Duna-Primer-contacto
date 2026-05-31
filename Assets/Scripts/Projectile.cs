using Unity.Netcode;
using UnityEditor;
using UnityEngine;

public class Projectile : NetworkBehaviour
{
    //public LayerMask hitMask;
    public float speed = 10f;
    public float lifeTime = 3f;
    public int damage = 10;

    private Rigidbody rb;

    protected Vector3 direction;

    //====================================================//
    void Awake()
    {
        rb = GetComponent<Rigidbody>();
    }

    //====================================================//
    public virtual void Initialize(Vector3 dir)
    {
        direction = dir.normalized;

        Invoke(nameof(DespawnProjectile), lifeTime);
    }

    //====================================================//
    protected virtual void FixedUpdate()
    {
        if (!IsServer)
            return;

        rb.linearVelocity = direction * speed;
    }

    //====================================================//
    protected virtual void OnTriggerEnter(Collider other)
    {
        if (!IsServer)
            return;

        //if ((hitMask.value & (1 << other.gameObject.layer)) == 0)
            //return;

        HealthController health =
            other.GetComponent<HealthController>();

        if (health != null)
        {
            health.TakeDamage(damage);
        }

        NetworkObject.Despawn();
    }

    //====================================================//
    void DespawnProjectile()
    {
        if (!IsServer)
            return;

        if (NetworkObject.IsSpawned)
            NetworkObject.Despawn();
    }
}
