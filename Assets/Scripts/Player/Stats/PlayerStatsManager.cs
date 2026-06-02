using System.Collections.Generic;
using UnityEngine;

public class PlayerStatsManager : MonoBehaviour
{
    public float basePower = 10;
    public float baseMoveSpeed = 5;
    public float baseMana = 100;
    public float baseAttackSpeed = 1f;

    private Dictionary<StatType, float> finalStats = new();

    private List<StatModifier> runeModifiers = new();

    public PlayerContext ctx;

    public System.Action OnStatsChanged;

    private void Awake()
    {
        ctx = GetComponentInParent<PlayerContext>();
        RecalculateStats();
    }

    void Start()
    {
        ctx.equipment.OnEquipmentChanged += RecalculateStats;
    }

    //====================================================//

    public void AddRuneModifier(StatModifier mod)
    {
        runeModifiers.Add(mod);
        RecalculateStats();
    }

    //====================================================//

    public void RecalculateStats()
    {
        finalStats.Clear();

        //  BASE
        AddStat(StatType.Power, basePower);
        AddStat(StatType.MoveSpeed, baseMoveSpeed);
        AddStat(StatType.Mana, baseMana);
        AddStat(StatType.AttackSpeed, baseAttackSpeed);

        //  RUNAS
        ApplyModifierList(runeModifiers);

        //  EQUIPO
        if (ctx.equipment != null)
        {
            ApplyModifiers(ctx.equipment.weapon);
            ApplyModifiers(ctx.equipment.armor);
        }

        OnStatsChanged?.Invoke();
    }

    //====================================================//

    void ApplyModifiers(EquipableItem item)
    {
        if (item == null || item.modifiers == null) return;

        foreach (var mod in item.modifiers)
        {
            AddStat(mod.stat, mod.value);
        }
    }

    void ApplyModifierList(List<StatModifier> mods)
    {
        foreach (var mod in mods)
        {
            AddStat(mod.stat, mod.value);
        }
    }

    void AddStat(StatType type, float value)
    {
        if (!finalStats.ContainsKey(type))
            finalStats[type] = 0;

        finalStats[type] += value;
    }

    public float GetStat(StatType type)
    {
        return finalStats.TryGetValue(type, out float value) ? value : 0;
    }



    public void ApplyLevelBonus(int level)
    {
        basePower += 2;

        if (level >= 4)
            baseMoveSpeed += 0.5f;

        if (level >= 8)
            baseMana += 0.2f;

        if (level >= 12)
            basePower += 5;

        RecalculateStats();
    }
}
