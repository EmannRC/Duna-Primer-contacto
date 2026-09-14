using UnityEngine;

[CreateAssetMenu(menuName = "Enemies/Enemy Stats Data")]
public class EnemyStatsData : ScriptableObject
{
    [System.Serializable]
    public class LevelStats
    {
        public int level;
        public float maxHealth;
        public float attackDamage;
    }

    [SerializeField] private LevelStats[] levels;

    public LevelStats GetStats(int level)
    {
        if (levels == null || levels.Length == 0)
            return null;

        foreach (LevelStats stats in levels)
        {
            if (stats.level == level)
                return stats;
        }

        return null;
    }
}
