using UnityEngine;

[CreateAssetMenu(menuName = "Enemies/Enemy Stats Data")]
public class EnemyStatsData : ScriptableObject
{
    [Header("Base Stats")]
    [SerializeField, Min(1f)] private float baseHealth = 100f;
    [SerializeField, Min(0f)] private float baseDamage = 15f;

    [Header("Level Growth")]
    [SerializeField, Range(0f, 1f)]
    private float healthGrowth = 0.15f;

    [SerializeField, Range(0f, 1f)]
    private float damageGrowth = 0.10f;

    public float GetMaxHealth(int level)
    {
        level = Mathf.Max(1, level);

        return baseHealth * Mathf.Pow(
            1f + healthGrowth,
            level - 1
        );
    }

    public float GetAttackDamage(int level)
    {
        level = Mathf.Max(1, level);

        return baseDamage * Mathf.Pow(
            1f + damageGrowth,
            level - 1
        );
    }
}
