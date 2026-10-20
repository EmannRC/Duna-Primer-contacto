using UnityEngine;

[CreateAssetMenu(menuName = "Items/Weapon")]
public class Weapon : EquipableItem
{
    public GameObject weaponPrefab;

    [Header("Combat")]
    public GameObject projectilePrefab;
    public float manaCost;
}
