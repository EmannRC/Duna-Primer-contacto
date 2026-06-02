using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(menuName = "Items/Database")]
public class ItemDatabase : ScriptableObject
{
    public List<Item> items;

    public Item GetByItemId(string itemId)
    {
        return items.Find(i => i.itemId == itemId);
    }
}
