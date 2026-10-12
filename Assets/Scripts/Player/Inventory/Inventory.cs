using System;
using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;
using UnityEngine.InputSystem;
using Duna.QuestSystem;

public class Inventory : NetworkBehaviour, IQuestItemProvider
{
    public NetworkList<InventorySlot> items;

    public ItemDatabase itemDatabase;

    public Action OnInventoryChanged;

    public int maxSlots = 20;

    private ulong nextRemovalRequestId;
    private readonly Dictionary<ulong, Action<bool>> pendingRemovalCallbacks = new();

    //====================================================//
    void Awake()
    {
        items = new NetworkList<InventorySlot>();
    }

    //====================================================//
    public override void OnNetworkSpawn()
    {
        items.OnListChanged += OnItemsChanged;
    }

    //====================================================//
    public override void OnNetworkDespawn()
    {
        items.OnListChanged -= OnItemsChanged;
    }

    //====================================================//
    private void OnItemsChanged(NetworkListEvent<InventorySlot> change)
    {
        OnInventoryChanged?.Invoke();
    }

    //==============================================================//
    public void AddItem(string itemId, int amount = 1)
    {
        Debug.Log($"AddItem: {itemId} x{amount}");

        if (!IsServer)
            return;

        if (TryStackItem(itemId, amount))
            return;

        if (items.Count >= maxSlots)
        {
            Debug.Log("Inventario lleno");
            return;
        }

        items.Add(new InventorySlot(
            itemId,
            amount
            )
        );
    }

    //==============================================================//
    public void RemoveItem(string itemId, int amount = 1)
    {
        if (!IsServer)
            return;

        for (int i = 0; i < items.Count; i++)
        {
            if (items[i].itemId != itemId)
                continue;

            var slot = items[i];

            slot.amount -= amount;

            if (slot.amount <= 0)
            {
                items.RemoveAt(i);
            }
            else
            {
                items[i] = slot;
            }

            return;
        }
    }

    public bool TryRemoveItem(string itemId, int amount)
    {
        if (!IsServer || string.IsNullOrWhiteSpace(itemId) || amount <= 0)
            return false;

        if (!HasItem(itemId, amount))
            return false;

        RemoveItem(itemId, amount);

        return true;
    }

    public void RequestRemoveItem(string itemId, int amount, Action<bool> onCompleted)
    {
        if (IsServer)
        {
            onCompleted?.Invoke(TryRemoveItem(itemId, amount));
            return;
        }

        if (!IsOwner || string.IsNullOrWhiteSpace(itemId) || amount <= 0)
        {
            onCompleted?.Invoke(false);
            return;
        }

        ulong requestId = ++nextRemovalRequestId;
        pendingRemovalCallbacks[requestId] = onCompleted;
        RemoveItemRpc(itemId, amount, requestId);
    }

    [Rpc(SendTo.Server, InvokePermission = RpcInvokePermission.Owner)]
    private void RemoveItemRpc(string itemId, int amount, ulong requestId)
    {
        bool removed = TryRemoveItem(itemId, amount);
        RemoveItemResultRpc(requestId, removed);
    }

    [Rpc(SendTo.Owner)]
    private void RemoveItemResultRpc(ulong requestId, bool removed)
    {
        if (!pendingRemovalCallbacks.TryGetValue(requestId, out Action<bool> callback))
            return;

        pendingRemovalCallbacks.Remove(requestId);
        callback?.Invoke(removed);
    }

    //==============================================================//
    public bool HasItem(string itemId, int amount)
    {
        foreach (var slot in items)
        {
            if (slot.itemId == itemId)
                return slot.amount >= amount;
        }

        return false;
    }

    //==============================================================//
    private bool TryStackItem(string itemId, int amount)
    {
        var item = itemDatabase.GetByItemId(itemId);

        if (item == null || !item.isStackable)
            return false;

        for (int i = 0; i < items.Count; i++)
        {
            if (items[i].itemId != itemId)
                continue;

            var slot = items[i];

            slot.amount += amount;

            items[i] = slot;

            return true;
        }

        return false;
    }

    //==============================================================//
    public int GetItemAmount(string itemId)
    {
        foreach (var slot in items)
        {
            if (slot.itemId == itemId)
                return slot.amount;
        }

        return 0;
    }

    //================================================//
    // REQUEST ADD ITEM
    //================================================//

    public void RequestAddItem(
        string itemID,
        int amount = 1
    )
    {
        AddItemRpc(
            itemID,
            amount
        );
    }

    //================================================//
    // ADD ITEM RPC
    //================================================//

    [Rpc(SendTo.Server, InvokePermission = RpcInvokePermission.Owner)]
    private void AddItemRpc(string itemID, int amount)
    {
        AddItem(
            itemID,
            amount
        );
    }
}



