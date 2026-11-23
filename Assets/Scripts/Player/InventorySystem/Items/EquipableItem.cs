using UnityEngine;


public abstract class EquipableItem : Item
{
    public StatModifier[] modifiers;
    public override bool CanEquip => true;
}
