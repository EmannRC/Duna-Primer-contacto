using NUnit.Framework.Interfaces;
using System;
using Duna.QuestSystem;
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

    [Header("Grounding")]
    [SerializeField] private float groundOffset = 1f;
    [SerializeField] private float raycastHeight = 5f;
    [SerializeField] private float raycastDistance = 20f;
    [SerializeField] private LayerMask groundLayer;

    private PickupSpawner spawner;


    public override void OnNetworkSpawn()
    {
        if (!IsServer) return;

        spawner = FindFirstObjectByType<PickupSpawner>();

        SnapToGround();
    }

    //======================================================//
    private void Update()
    {
        transform.Rotate(Vector3.up,rotationSpeed * Time.deltaTime,Space.World);
    }

    //======================================================//
    private void SnapToGround()
    {
        Vector3 rayOrigin = transform.position + Vector3.up * raycastHeight;

        if (Physics.Raycast(
            rayOrigin,
            Vector3.down,
            out RaycastHit hit,
            raycastDistance,
            groundLayer))
        {
            transform.position = hit.point + Vector3.up * groundOffset;
        }
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
            QuestEvents.RaiseCollectItem(itemId, amount);
        }

        //  avisar al spawner
        if (spawner != null)
            spawner.OnPickupCollected(NetworkObject);

        NetworkObject.Despawn();
    }

}
