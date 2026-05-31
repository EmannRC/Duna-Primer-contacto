using System;
using UnityEngine;

[Serializable]
public class LootDrop
{
    public Item item;

    [Range(0, 100)]
    public float chance;

    public int minAmount = 1;
    public int maxAmount = 1;
}
