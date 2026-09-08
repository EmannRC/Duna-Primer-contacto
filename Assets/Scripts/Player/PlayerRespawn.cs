using System.Globalization;
using Unity.Netcode;
using UnityEngine;

public class PlayerRespawn : NetworkBehaviour
{
    public void RequestRestart()
    {
        if (IsOwner)
            RestartServerRpc();
    }

    [ServerRpc]
    private void RestartServerRpc()
    {
        var spawner = FindFirstObjectByType<PlayerSpawner>();

        if (spawner != null)
        {
            //spawner.RespawnPlayer(OwnerClientId);
        }
    }
}
