using UnityEngine;

[CreateAssetMenu(menuName = "Items/Rune")]
public class Rune : Item
{
    public StatModifier[] modifiers;

    public override void Consume(GameObject target)
    {
        var stats = target.GetComponentInChildren<PlayerStatsManager>();

        foreach (var mod in modifiers)
        {
            stats.AddRuneModifier(mod);
        }
    }
}
