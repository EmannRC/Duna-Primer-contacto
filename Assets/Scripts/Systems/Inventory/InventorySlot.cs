using System;
using Unity.Collections;
using Unity.Netcode;
using UnityEngine;

[System.Serializable]
public struct InventorySlot : INetworkSerializable, IEquatable<InventorySlot>
{
    public FixedString64Bytes itemId;
    public int amount;

    public InventorySlot(string itemId, int amount)
    {
        this.itemId = itemId;
        this.amount = amount;
    }
    public void NetworkSerialize<T>(BufferSerializer<T> serializer) where T : IReaderWriter
    {
        serializer.SerializeValue(ref itemId);

        serializer.SerializeValue(ref amount);
    }

    public bool Equals(InventorySlot other)
    {
        return
        itemId.Equals(other.itemId) &&
        amount == other.amount;
    }
}
