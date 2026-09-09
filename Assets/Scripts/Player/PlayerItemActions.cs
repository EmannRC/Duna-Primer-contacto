using Unity.Netcode;
using UnityEngine;

public class PlayerItemActions : NetworkBehaviour
{
    private PlayerContext ctx;

    //====================================================//
    private void Awake()
    {
        ctx = GetComponentInParent<PlayerContext>();
    }

    //====================================================//
    [ServerRpc]
    public void RequestConsumeItemServerRpc(string itemId)
    {
        if (!IsServer) return;

        Item item = ctx.inventory.itemDatabase.GetByItemId(itemId);
        if (item == null) return;

        if (!ctx.inventory.HasItem(itemId, 1)) return;

        item.Consume(gameObject);
        ctx.inventory.RemoveItem(itemId, 1);
    }

    //====================================================//
    [ServerRpc]
    public void RequestDropItemServerRpc(string itemId)
    {
        if (!IsServer) return;

        Item item = ctx.inventory.itemDatabase.GetByItemId(itemId);
        if (item == null) return;

        if (!ctx.inventory.HasItem(itemId, 1)) return;

        ctx.inventory.RemoveItem(itemId, 1);

        SpawnDrop(item);
    }

    //====================================================//
    private void SpawnDrop(Item item)
    {
        if (item == null || item.pickupPrefab == null) return;

        Vector3 basePos = transform.position;
        Vector3 offset = Random.insideUnitSphere * 2.5f;
        offset.y = 0;

        Vector3 spawnPos = basePos + offset + Vector3.up * 0.5f;

        GameObject drop = Instantiate(item.pickupPrefab, spawnPos, Quaternion.identity);

        var netObj = drop.GetComponent<NetworkObject>();
        if (netObj != null)
            netObj.Spawn(true);
    }
}
