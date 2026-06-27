using NUnit.Framework.Interfaces;
using System;
using Unity.Netcode;
using UnityEngine;

public class ItemPickup : NetworkBehaviour
{
    [SerializeField] private string itemId;
    [SerializeField] private int amount = 1;

    [Header("Data Item")]
    [SerializeField] private ItemDatabase itemDatabase;

    [Header("Rotation")]
    [SerializeField] private float rotationSpeed = 60f;

    private PickupSpawner spawner;


    public override void OnNetworkSpawn()
    {
        if (!IsServer) return;

        spawner = FindFirstObjectByType<PickupSpawner>();
    }

    //======================================================//
    private void Update()
    {
        transform.Rotate(Vector3.up,rotationSpeed * Time.deltaTime,Space.World);
    }

    //======================================================//
    private void OnTriggerEnter(Collider other)
    {
        Debug.Log($"Trigger: {other.name}");

        if (!IsServer) return;

        PlayerContext ctx = other.GetComponent<PlayerContext>();

        if (ctx == null) return;

        Debug.Log($"Pickup por: {ctx.name}");

        HandlePickup(ctx);
    }

    //======================================================//
    private void HandlePickup(PlayerContext player)
    {
        Debug.Log($"HandlePickup: {player.name}");

        Item item = itemDatabase.GetByItemId(itemId);

        Debug.Log($"Item encontrado: {item}");

        if (item == null)
        {
            Debug.LogError($"Item no encontrado: {itemId}");
            return;
        }

        if (item is Rune rune)
        {
            rune.Consume(player.gameObject);
        }
        else
        {
            player.inventory.AddItem(itemId, amount);
        }

        //  avisar al spawner
        if (spawner != null)
            spawner.OnPickupCollected(NetworkObject);

        NetworkObject.Despawn();
    }

}
