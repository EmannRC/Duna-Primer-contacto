using UnityEngine;

public enum AbilityType
{
    Shield
}

[CreateAssetMenu(menuName = "Duna/Abilities/Ability")]
public class AbilityData : ScriptableObject
{
    [Header("General")]
    public string abilityName;
    public Sprite icon;

    [Header("Cooldown")]
    [Min(0f)] public float cooldown = 10f;

    [Header("Duration")]
    [Min(0f)] public float duration = 5f;

    [Header("Effect")]
    public AbilityType abilityType;
}
