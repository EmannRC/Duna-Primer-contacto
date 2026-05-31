using UnityEngine;

[CreateAssetMenu(menuName = "Items/Item")]
public class Item : ScriptableObject
{
    public string itemName;
    public string itemId;

    public Sprite icon;
    [Space]
    public Color iconColor = Color.white;

    public bool isStackable = true;

    public GameObject pickupPrefab;

    public virtual bool CanEquip => false;
    public virtual bool CanConsume => false;
    public virtual bool CanDrop => true;
    public virtual bool CanInspect => true;

    public virtual void Consume(GameObject user) { }
}
