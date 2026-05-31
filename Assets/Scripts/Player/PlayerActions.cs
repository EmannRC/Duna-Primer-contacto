using Unity.Netcode;
using UnityEngine;

public class PlayerActions : NetworkBehaviour
{
    [SerializeField] private Inventory inventory;

    //====================================================//
    [ServerRpc]
    public void RequestConsumeItemServerRpc(string itemId)
    {
        Item item = inventory.itemDatabase.GetByItemId(itemId);
        if (item == null) return;

        if (!inventory.HasItem(itemId, 1)) return;

        item.Consume(gameObject);
        inventory.RemoveItem(itemId, 1);
    }

    //====================================================//
    [ServerRpc]
    public void RequestDropItemServerRpc(string itemId)
    {
        Item item = inventory.itemDatabase.GetByItemId(itemId);
        if (item == null) return;

        if (!inventory.HasItem(itemId, 1)) return;

        inventory.RemoveItem(itemId, 1);

        SpawnDropClientRpc(itemId);
    }

    //====================================================//
    [ClientRpc]
    private void SpawnDropClientRpc(string itemId)
    {
        Item item = inventory.itemDatabase.GetByItemId(itemId);
        if (item == null || item.pickupPrefab == null) return;

        Vector3 basePos = transform.position;
        Vector3 offset = Random.insideUnitSphere * 2.5f;
        offset.y = 0;

        Vector3 spawnPos = basePos + offset + Vector3.up * 0.5f;

        Instantiate(item.pickupPrefab, spawnPos, Quaternion.identity);
    }
}
