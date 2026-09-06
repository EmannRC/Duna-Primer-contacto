using System.Collections.Generic;
using UnityEngine;

public class EnemyFormation : MonoBehaviour
{
    public static readonly List<EnemyFormation> ActiveEnemies = new();

    public int SlotIndex { get; private set; }

    private void OnEnable()
    {
        ActiveEnemies.Add(this);
        RefreshSlots();
    }

    private void OnDisable()
    {
        ActiveEnemies.Remove(this);
        RefreshSlots();
    }

    private static void RefreshSlots()
    {
        for (int Enemies = 0; Enemies < ActiveEnemies.Count; Enemies++)
        {
            ActiveEnemies[Enemies].SlotIndex = Enemies;
        }
    }
}
