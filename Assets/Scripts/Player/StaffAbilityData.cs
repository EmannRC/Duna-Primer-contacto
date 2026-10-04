using UnityEngine;
public enum StaffAbilityType
{
    NatureShield
}

[CreateAssetMenu(menuName = "Duna/Abilities/Staff Ability")]

public class StaffAbilityData : ScriptableObject
{
    [Header("General")]
    public string abilityName;
    public Sprite icon;

    [Header("Cooldown")]
    public float cooldown = 10f;

    [Header("Duration")]
    public float duration = 5f;

    [Header("Ability")]
    public StaffAbilityType abilityType;
}
