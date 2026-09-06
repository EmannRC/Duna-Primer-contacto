using Unity.Netcode;
using UnityEngine;

public class EnemyStats : NetworkBehaviour
{
    [field: SerializeField]
    public float MoveSpeed { get; private set; } = 3.5f;

    [field: SerializeField]
    public float AttackDamage { get; private set; } = 10f;

    [field: SerializeField]
    public float AttackRange { get; private set; } = 2f;
}
