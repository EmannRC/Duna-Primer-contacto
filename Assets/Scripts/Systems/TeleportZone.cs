using Unity.Netcode;
using Unity.Netcode.Components;
using UnityEngine;

public class TeleportZone : NetworkBehaviour
{
    [SerializeField] private Transform destination;
    private Collider triggerCollider;

    private void Awake()
    {
        triggerCollider = GetComponent<Collider>();
    }

    private void OnTriggerEnter(Collider other)
    {
        NetworkObject playerObject = other.GetComponentInParent<NetworkObject>();
        if (playerObject == null || !playerObject.IsPlayerObject)
            return;

        // Only the owning client can request a teleport for its player.
        if (!playerObject.IsOwner)
            return;

        if (IsServer)
        {
            ProcessTeleportRequest(playerObject, NetworkManager.LocalClientId);
            return;
        }

        RequestTeleportRpc(playerObject.NetworkObjectId);
    }

    [Rpc(SendTo.Server, InvokePermission = RpcInvokePermission.Everyone)]
    private void RequestTeleportRpc(ulong playerNetworkObjectId, RpcParams rpcParams = default)
    {
        if (!NetworkManager.SpawnManager.SpawnedObjects.TryGetValue(
                playerNetworkObjectId, out NetworkObject playerObject))
            return;

        ProcessTeleportRequest(playerObject, rpcParams.Receive.SenderClientId);
    }

    private void ProcessTeleportRequest(NetworkObject playerObject, ulong senderClientId)
    {
        if (destination == null || playerObject == null || !playerObject.IsPlayerObject)
            return;

        if (playerObject.OwnerClientId != senderClientId)
            return;

        // Validate the actual collider overlap. A player's root position can sit
        // outside the trigger bounds while its capsule is already inside them.
        if (!IsPlayerOverlappingPortal(playerObject))
            return;

        if (playerObject.TryGetComponent<NetworkTransform>(out var netTransform) &&
            netTransform.IsServerAuthoritative())
        {
            ApplyTeleport(playerObject, destination.position, destination.rotation);
            return;
        }

        TeleportPlayerRpc(
            playerObject.NetworkObjectId,
            destination.position,
            destination.rotation,
            RpcTarget.Single(playerObject.OwnerClientId, RpcTargetUse.Temp));
    }

    private bool IsPlayerOverlappingPortal(NetworkObject playerObject)
    {
        if (triggerCollider == null)
            return false;

        Collider[] playerColliders = playerObject.GetComponentsInChildren<Collider>();
        foreach (Collider playerCollider in playerColliders)
        {
            if (playerCollider == null || !playerCollider.enabled || playerCollider.isTrigger)
                continue;

            if (triggerCollider.bounds.Intersects(playerCollider.bounds))
                return true;
        }

        return false;
    }

    [Rpc(SendTo.SpecifiedInParams)]
    private void TeleportPlayerRpc(
        ulong playerNetworkObjectId,
        Vector3 targetPosition,
        Quaternion targetRotation,
        RpcParams rpcParams = default)
    {
        if (!NetworkManager.SpawnManager.SpawnedObjects.TryGetValue(
                playerNetworkObjectId, out NetworkObject playerObject))
            return;

        if (!playerObject.IsOwner)
            return;

        ApplyTeleport(playerObject, targetPosition, targetRotation);
    }

    private void ApplyTeleport(NetworkObject playerObject, Vector3 targetPosition, Quaternion targetRotation)
    {
        CharacterController controller = playerObject.GetComponent<CharacterController>();
        if (controller != null)
            controller.enabled = false;

        if (playerObject.TryGetComponent<NetworkTransform>(out var netTransform))
        {
            netTransform.Teleport(targetPosition, targetRotation, playerObject.transform.localScale);
        }
        else
        {
            playerObject.transform.SetPositionAndRotation(targetPosition, targetRotation);
        }

        if (controller != null)
            controller.enabled = true;
    }
}
