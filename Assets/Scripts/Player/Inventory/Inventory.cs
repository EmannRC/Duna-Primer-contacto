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
        if (!IsServer)
            return false;

        if (!HasItem(itemId, amount))
            return false;

        RemoveItem(itemId, amount);

        return true;
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

    public bool TryRemoveItems(IReadOnlyList<QuestObjectiveData.RequiredItem> requiredItems)
    {
        if (!IsServer)
            return false;

        if (requiredItems == null || requiredItems.Count == 0)
            return false;

        //================================================//
        // 1. COMPROBAR TODO ANTES DE MODIFICAR NADA
        //================================================//

        foreach (var requiredItem in requiredItems)
        {
            if (requiredItem == null)
                return false;

            if (requiredItem.Amount <= 0)
                return false;

            if (!HasItem(
                    requiredItem.ItemID,
                    requiredItem.Amount))
            {
                return false;
            }
        }

        //================================================//
        // 2. ELIMINAR TODO
        //================================================//

        foreach (var requiredItem in requiredItems)
        {
            RemoveItem(
                requiredItem.ItemID,
                requiredItem.Amount);
        }

        return true;
    }
}



