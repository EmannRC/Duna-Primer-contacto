using Unity.Netcode;
using UnityEngine;

[System.Serializable]
public class DropItem
{
    public NetworkObject prefab;

    [Range(0, 100)]
    public float weight = 10;
}
