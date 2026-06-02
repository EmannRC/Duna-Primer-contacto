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

 
    //======================================================//
    private void Update()
    {
        transform.Rotate(Vector3.up,rotationSpeed * Time.deltaTime,Space.World);
    }

    //======================================================//
    private void OnTriggerEnter(Collider other)
    {
        if (!IsServer) return;

        PlayerContext ctx = other.GetComponent<PlayerContext>();

        if (ctx == null) return;

        HandlePickup(ctx);
    }

    //======================================================//
    private void HandlePickup(PlayerContext player)
    {
        Item item = itemDatabase.GetByItemId(itemId);

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

        NetworkObject.Despawn();
    }

}
