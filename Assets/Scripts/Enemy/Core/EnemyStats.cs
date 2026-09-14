using Unity.Netcode;
using UnityEngine;

public class EnemyStats : NetworkBehaviour
{
    [Header("Stats")]
    [SerializeField] private EnemyStatsData statsData;
    [SerializeField] private int level = 1;

    public int Level => level;

    public float MaxHealth
    {
        get
        {
            EnemyStatsData.LevelStats stats = statsData.GetStats(level);

            if (stats == null)
                return 0f;

            return stats.maxHealth;
        }
    }

    public float AttackDamage
    {
        get
        {
            EnemyStatsData.LevelStats stats = statsData.GetStats(level);

            if (stats == null)
                return 0f;

            return stats.attackDamage;
        }
    }
}
