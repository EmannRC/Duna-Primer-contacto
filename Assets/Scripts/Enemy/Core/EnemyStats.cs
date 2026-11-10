using Unity.Netcode;
using UnityEngine;

public class EnemyStats : NetworkBehaviour
{
    [Header("Stats")]
    [SerializeField] private EnemyStatsData statsData;
    [SerializeField, Min(1)] private int level = 1;

    public int Level => level;

    public float MaxHealth =>
        statsData != null ? statsData.GetMaxHealth(level) : 0f;

    public float AttackDamage =>
        statsData != null ? statsData.GetAttackDamage(level) : 0f;
}
